<script setup lang="ts">
import { computed, onBeforeUnmount, reactive, ref, watch } from 'vue'
import { ChevronLeft, ChevronRight, Edit3, Plus, RefreshCw, Search, Trash2, X } from 'lucide-vue-next'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import ModalDialog from '../components/ModalDialog.vue'
import { displayAmount, displayDate, inputDate, todayInputDate } from '../lib/format'
import { usePeopleQuery } from '../queries/PersonQueries'
import {
  useAssignmentMutation,
  useDeleteTransactionMutation,
  usePendingAssignmentIds,
  useSaveTransactionMutation,
  useTransactionsQuery,
} from '../queries/TransactionQueries'
import { useToast } from '../stores/toast'
import type {
  ListTransactionParams,
  TransactionPayload,
  TransactionWithPerson,
} from '../entities/TransactionEntity.ts'

const toast = useToast()

const pageSizeOptions = [10, 20, 50, 100]
const formOpen = ref(false)
const editing = ref<TransactionWithPerson | null>(null)
const deleteTarget = ref<TransactionWithPerson | null>(null)
const page = ref(1)
const limit = ref(20)
const debouncedSearch = ref('')

const filters = reactive({
  search: '',
  startDate: '',
  endDate: '',
  personFilter: '',
  order: 'desc' as 'asc' | 'desc',
})

const form = reactive({
  date: '',
  title: '',
  amount: '',
  personId: '',
})

let searchDebounce: ReturnType<typeof window.setTimeout> | undefined

const transactionParams = computed<ListTransactionParams>(() => ({
  search: debouncedSearch.value || undefined,
  startDate: filters.startDate || undefined,
  endDate: filters.endDate || undefined,
  personId: filters.personFilter && filters.personFilter !== 'unassigned'
    ? filters.personFilter
    : undefined,
  unassigned: filters.personFilter === 'unassigned' ? true : undefined,
  page: page.value,
  limit: limit.value,
  order: filters.order,
}))

const { query: transactionQuery, queryKey: transactionQueryKey } = useTransactionsQuery(transactionParams)
const peopleQuery = usePeopleQuery()

const transactions = computed(() => transactionQuery.data.value?.transactions ?? [])
const people = computed(() => peopleQuery.data.value ?? [])
const loading = computed(() => transactionQuery.isPending.value || peopleQuery.isPending.value)
const error = computed(() => readError(transactionQuery.error.value ?? peopleQuery.error.value))
const pagination = computed(() => ({
  page: transactionQuery.data.value?.page ?? page.value,
  limit: transactionQuery.data.value?.limit ?? limit.value,
  total: transactionQuery.data.value?.total ?? 0,
  totalPages: transactionQuery.data.value?.totalPages ?? 0,
}))

const assignedCount = computed(
  () => transactions.value.filter((item) => item.transactionPerson !== null).length,
)

const totalAmount = computed(() =>
  transactions.value.reduce((total, item) => total + Number(item.transaction.amount), 0),
)

const visibleTotalPages = computed(() => Math.max(pagination.value.totalPages, 1))
const hasPreviousPage = computed(() => pagination.value.page > 1)
const hasNextPage = computed(() => pagination.value.page < pagination.value.totalPages)

const pageRange = computed(() => {
  if (pagination.value.total === 0) {
    return '0 of 0'
  }

  if (transactions.value.length === 0) {
    return `0 of ${pagination.value.total}`
  }

  const start = (pagination.value.page - 1) * pagination.value.limit + 1
  const end = Math.min(start + transactions.value.length - 1, pagination.value.total)

  return `${start}-${end} of ${pagination.value.total}`
})

const saveTransactionMutation = useSaveTransactionMutation({
  onSuccess: (variables) => {
    toast.success(variables.editing ? 'Transaction updated' : 'Transaction created')
    formOpen.value = false
  },
  onError: (error) => {
    toast.error(readError(error))
  },
})

const assignmentMutation = useAssignmentMutation(people, transactionQueryKey, {
  onSuccess: (variables) => {
    toast.success(variables.nextPersonId ? 'Person assigned' : 'Assignment cleared')
  },
  onError: (error, variables) => {
    variables.select.value = variables.previousPersonId
    toast.error(readError(error))
  },
})

const pendingAssignmentIds = usePendingAssignmentIds()

const deleteTransactionMutation = useDeleteTransactionMutation(transactionQueryKey, page, {
  onSuccess: () => {
    toast.success('Transaction deleted')
    deleteTarget.value = null
  },
  onError: (error) => {
    toast.error(readError(error))
  },
})

const saving = computed(() => saveTransactionMutation.isPending.value)
const deleting = computed(() => deleteTransactionMutation.isPending.value)
const assignmentSaving = computed<Record<string, boolean>>(() => Object.fromEntries(
  pendingAssignmentIds.value.map((transactionId) => [transactionId, true]),
))

onBeforeUnmount(() => {
  clearSearchDebounce()
})

watch(
  () => filters.search,
  () => {
    clearSearchDebounce()

    searchDebounce = window.setTimeout(() => {
      page.value = 1
      debouncedSearch.value = filters.search.trim()
    }, 300)
  },
  { flush: 'sync' },
)

watch(
  () => transactionQuery.data.value,
  (response) => {
    if (
      response &&
      response.transactions.length === 0 &&
      response.total > 0 &&
      page.value > response.totalPages
    ) {
      page.value = response.totalPages
    }
  },
)

watch(
  () => transactionQuery.error.value,
  (queryError) => {
    if (queryError) {
      toast.error(readError(queryError))
    }
  },
)

watch(
  () => peopleQuery.error.value,
  (queryError) => {
    if (queryError) {
      toast.error(readError(queryError))
    }
  },
)

function loadData() {
  void Promise.all([
    transactionQuery.refetch(),
    peopleQuery.refetch(),
  ])
}

function clearSearchDebounce() {
  if (searchDebounce !== undefined) {
    window.clearTimeout(searchDebounce)
    searchDebounce = undefined
  }
}

function clearSearch() {
  if (!filters.search) {
    return
  }

  filters.search = ''
  clearSearchDebounce()
  page.value = 1
  debouncedSearch.value = ''
}

function applyDateFilter() {
  page.value = 1
}

function applyPersonFilter() {
  page.value = 1
}

function changeLimit(event: Event) {
  const select = event.target as HTMLSelectElement
  limit.value = Number(select.value)
  page.value = 1
}

function changeOrder(event: Event) {
  const select = event.target as HTMLSelectElement
  filters.order = select.value === 'asc' ? 'asc' : 'desc'
  page.value = 1
}

function goToPreviousPage() {
  if (!hasPreviousPage.value) {
    return
  }

  page.value -= 1
}

function goToNextPage() {
  if (!hasNextPage.value) {
    return
  }

  page.value += 1
}

function openCreateForm() {
  editing.value = null
  form.date = todayInputDate()
  form.title = ''
  form.amount = ''
  form.personId = ''
  formOpen.value = true
}

function openEditForm(item: TransactionWithPerson) {
  editing.value = item
  form.date = inputDate(item.transaction.date)
  form.title = item.transaction.title
  form.amount = String(item.transaction.amount)
  form.personId = item.person?.id ?? ''
  formOpen.value = true
}

function closeForm() {
  if (!saving.value) {
    formOpen.value = false
  }
}

function submitForm() {
  const payload: TransactionPayload = {
    date: form.date,
    title: form.title.trim(),
    amount: Number(form.amount),
  }

  saveTransactionMutation.mutate({
    editing: editing.value,
    payload,
    personId: form.personId,
  })
}

function changeAssignment(item: TransactionWithPerson, event: Event) {
  const select = event.target as HTMLSelectElement
  const previousPersonId = item.person?.id ?? ''
  const nextPersonId = select.value

  if (previousPersonId === nextPersonId) {
    return
  }

  assignmentMutation.mutate({ item, nextPersonId, previousPersonId, select })
}

function confirmDelete(item: TransactionWithPerson) {
  deleteTarget.value = item
}

function executeDelete() {
  if (!deleteTarget.value) {
    return
  }

  deleteTransactionMutation.mutate(deleteTarget.value.transaction.id)
}

function readError(err: unknown): string {
  if (!err) {
    return ''
  }

  return err instanceof Error ? err.message : 'Something went wrong'
}
</script>

<template>
  <section class="space-y-5">
    <div class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
      <div>
        <p class="text-sm font-medium text-emerald-700">Transaction desk</p>
        <h1 class="mt-1 text-2xl font-semibold text-stone-950">Transactions</h1>
      </div>

      <div class="flex flex-wrap gap-3">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          @click="loadData"
        >
          <RefreshCw class="size-4" aria-hidden="true" />
          Refresh
        </button>
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md bg-emerald-600 px-3 py-2 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          @click="openCreateForm"
        >
          <Plus class="size-4" aria-hidden="true" />
          New transaction
        </button>
      </div>
    </div>

    <div class="grid gap-3 md:grid-cols-3">
      <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
        <p class="text-xs font-medium uppercase text-stone-500">Rows</p>
        <p class="mt-1 text-xl font-semibold text-stone-950">{{ pagination.total }}</p>
      </div>
      <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
        <p class="text-xs font-medium uppercase text-stone-500">Assigned</p>
        <p class="mt-1 text-xl font-semibold text-stone-950">{{ assignedCount }}</p>
      </div>
      <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
        <p class="text-xs font-medium uppercase text-stone-500">Amount</p>
        <p class="mt-1 text-xl font-semibold text-stone-950">{{ displayAmount(totalAmount) }}</p>
      </div>
    </div>

    <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
      <div class="grid gap-3 md:grid-cols-2 lg:grid-cols-[minmax(14rem,1.4fr)_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_10rem_10rem] lg:items-end">
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Search</span>
          <span class="relative mt-1 block">
            <Search
              class="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-stone-400"
              aria-hidden="true"
            />
            <input
              v-model="filters.search"
              class="w-full rounded-md border border-stone-300 py-2 pl-9 pr-10 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              type="search"
              placeholder="Title or person"
            />
            <button
              v-if="filters.search"
              type="button"
              class="absolute right-1.5 top-1/2 inline-flex size-7 -translate-y-1/2 items-center justify-center rounded-md text-stone-400 hover:bg-stone-100 hover:text-stone-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
              title="Clear search"
              @click="clearSearch"
            >
              <X class="size-4" aria-hidden="true" />
              <span class="sr-only">Clear search</span>
            </button>
          </span>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Start date</span>
          <input
            v-model="filters.startDate"
            class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            type="date"
            @change="applyDateFilter"
          />
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">End date</span>
          <input
            v-model="filters.endDate"
            class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            type="date"
            @change="applyDateFilter"
          />
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Person</span>
          <select
            v-model="filters.personFilter"
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            @change="applyPersonFilter"
          >
            <option value="">All people</option>
            <option value="unassigned">Unassigned</option>
            <option v-for="person in people" :key="person.id" :value="person.id">
              {{ person.name }}
            </option>
          </select>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Rows</span>
          <select
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            :value="pagination.limit"
            @change="changeLimit"
          >
            <option v-for="option in pageSizeOptions" :key="option" :value="option">
              {{ option }}
            </option>
          </select>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Order</span>
          <select
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            :value="filters.order"
            @change="changeOrder"
          >
            <option value="desc">Newest</option>
            <option value="asc">Oldest</option>
          </select>
        </label>
      </div>
    </div>

    <div v-if="error" class="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ error }}
    </div>

    <div class="overflow-hidden rounded-lg border border-stone-200 bg-white">
      <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">Loading transactions...</div>
      <div v-else-if="transactions.length === 0" class="px-4 py-12 text-center text-sm text-stone-500">
        No transactions found.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-stone-200 text-left text-sm">
          <thead class="bg-stone-100 text-xs uppercase text-stone-500">
            <tr>
              <th class="px-4 py-3 font-semibold">Date</th>
              <th class="min-w-64 px-4 py-3 font-semibold">Title</th>
              <th class="px-4 py-3 text-right font-semibold">Amount</th>
              <th class="min-w-56 px-4 py-3 font-semibold">Person</th>
              <th class="w-28 px-4 py-3 text-right font-semibold">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr v-for="item in transactions" :key="item.transaction.id" class="hover:bg-stone-50">
              <td class="whitespace-nowrap px-4 py-3 text-stone-600">
                {{ displayDate(item.transaction.date) }}
              </td>
              <td class="px-4 py-3 font-medium text-stone-950">{{ item.transaction.title }}</td>
              <td class="whitespace-nowrap px-4 py-3 text-right text-stone-700">
                {{ displayAmount(item.transaction.amount) }}
              </td>
              <td class="px-4 py-3">
                <select
                  class="w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
                  :value="item.person?.id ?? ''"
                  :disabled="Boolean(assignmentSaving[item.transaction.id])"
                  @change="changeAssignment(item, $event)"
                >
                  <option value="">Unassigned</option>
                  <option v-for="person in people" :key="person.id" :value="person.id">
                    {{ person.name }}
                  </option>
                </select>
              </td>
              <td class="px-4 py-3">
                <div class="flex justify-end gap-2">
                  <button
                    type="button"
                    class="inline-flex size-8 items-center justify-center rounded-md text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    title="Edit transaction"
                    @click="openEditForm(item)"
                  >
                    <Edit3 class="size-4" aria-hidden="true" />
                    <span class="sr-only">Edit transaction</span>
                  </button>
                  <button
                    type="button"
                    class="inline-flex size-8 items-center justify-center rounded-md text-stone-500 hover:bg-rose-50 hover:text-rose-700 focus:outline-none focus:ring-2 focus:ring-rose-500"
                    title="Delete transaction"
                    @click="confirmDelete(item)"
                  >
                    <Trash2 class="size-4" aria-hidden="true" />
                    <span class="sr-only">Delete transaction</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div
        v-if="!loading"
        class="flex flex-col gap-3 border-t border-stone-200 px-4 py-3 text-sm text-stone-600 md:flex-row md:items-center md:justify-between"
      >
        <p>{{ pageRange }}</p>
        <div class="flex items-center gap-3">
          <button
            type="button"
            class="inline-flex size-8 items-center justify-center rounded-md border border-stone-300 text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-50"
            title="Previous page"
            :disabled="!hasPreviousPage"
            @click="goToPreviousPage"
          >
            <ChevronLeft class="size-4" aria-hidden="true" />
            <span class="sr-only">Previous page</span>
          </button>
          <span class="min-w-20 text-center font-medium text-stone-700">
            {{ pagination.page }} / {{ visibleTotalPages }}
          </span>
          <button
            type="button"
            class="inline-flex size-8 items-center justify-center rounded-md border border-stone-300 text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-50"
            title="Next page"
            :disabled="!hasNextPage"
            @click="goToNextPage"
          >
            <ChevronRight class="size-4" aria-hidden="true" />
            <span class="sr-only">Next page</span>
          </button>
        </div>
      </div>
    </div>
  </section>

  <ModalDialog :open="formOpen" :title="editing ? 'Edit transaction' : 'New transaction'" @close="closeForm">
    <form class="space-y-4" @submit.prevent="submitForm">
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Date</span>
        <input
          v-model="form.date"
          class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
          type="date"
          required
        />
      </label>
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Title</span>
        <input
          v-model="form.title"
          class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
          type="text"
          required
        />
      </label>
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Amount</span>
        <input
          v-model="form.amount"
          class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
          type="number"
          step="0.01"
          required
        />
      </label>
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Person</span>
        <select
          v-model="form.personId"
          class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
        >
          <option value="">Unassigned</option>
          <option v-for="person in people" :key="person.id" :value="person.id">
            {{ person.name }}
          </option>
        </select>
      </label>
      <div class="flex justify-end gap-3 pt-2">
        <button
          type="button"
          class="rounded-md border border-stone-300 px-4 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          :disabled="saving"
          @click="closeForm"
        >
          Cancel
        </button>
        <button
          type="submit"
          class="rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
          :disabled="saving"
        >
          {{ saving ? 'Saving...' : 'Save' }}
        </button>
      </div>
    </form>
  </ModalDialog>

  <ConfirmDialog
    :open="deleteTarget !== null"
    title="Delete transaction"
    message="This transaction will be marked as deleted."
    :busy="deleting"
    @cancel="deleteTarget = null"
    @confirm="executeDelete"
  />
</template>
