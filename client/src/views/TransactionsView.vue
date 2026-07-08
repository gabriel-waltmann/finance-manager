<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { ChevronLeft, ChevronRight, Edit3, FileUp, Plus, RefreshCw, Search, Trash2, X } from 'lucide-vue-next'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import ModalDialog from '../components/ModalDialog.vue'
import {
  createAssignment,
  createTransaction,
  deleteAssignment,
  deleteTransaction,
  listPeople,
  listTransactions,
  updateAssignment,
  updateTransaction,
  uploadTransactions,
} from '../api/finance'
import { displayAmount, displayDate, inputDate, todayInputDate } from '../lib/format'
import { useToast } from '../stores/toast'
import type {
  FileCategory,
  ListTransactionResponse,
  ListTransactionParams,
  Person,
  TransactionPayload,
  TransactionPerson,
  TransactionWithPerson,
} from '../types'

const toast = useToast()

const transactions = ref<TransactionWithPerson[]>([])
const people = ref<Person[]>([])
const pageSizeOptions = [10, 20, 50, 100]
const loading = ref(true)
const saving = ref(false)
const deleting = ref(false)
const uploadPending = ref(false)
const error = ref('')
const formOpen = ref(false)
const editing = ref<TransactionWithPerson | null>(null)
const deleteTarget = ref<TransactionWithPerson | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)
const assignmentSaving = ref<Record<string, boolean>>({})
const uploadCategory = ref<FileCategory>('CreditCard')

const filters = reactive({
  search: '',
  startDate: '',
  endDate: '',
  order: 'desc' as 'asc' | 'desc',
})

const pagination = reactive({
  page: 1,
  limit: 20,
  total: 0,
  totalPages: 0,
})

const form = reactive({
  date: '',
  title: '',
  amount: '',
  personId: '',
})

let searchDebounce: ReturnType<typeof window.setTimeout> | undefined

const assignedCount = computed(
  () => transactions.value.filter((item) => item.transactionPerson !== null).length,
)

const totalAmount = computed(() =>
  transactions.value.reduce((total, item) => total + Number(item.transaction.amount), 0),
)

const visibleTotalPages = computed(() => Math.max(pagination.totalPages, 1))
const hasPreviousPage = computed(() => pagination.page > 1)
const hasNextPage = computed(() => pagination.page < pagination.totalPages)

const pageRange = computed(() => {
  if (pagination.total === 0) {
    return '0 of 0'
  }

  if (transactions.value.length === 0) {
    return `0 of ${pagination.total}`
  }

  const start = (pagination.page - 1) * pagination.limit + 1
  const end = Math.min(start + transactions.value.length - 1, pagination.total)

  return `${start}-${end} of ${pagination.total}`
})

onMounted(() => {
  void loadData()
})

onBeforeUnmount(() => {
  clearSearchDebounce()
})

watch(
  () => filters.search,
  () => {
    clearSearchDebounce()

    searchDebounce = window.setTimeout(() => {
      pagination.page = 1
      void loadData()
    }, 300)
  },
  { flush: 'sync' },
)

async function loadData() {
  loading.value = true
  error.value = ''

  try {
    const [transactionResponse, personRows] = await Promise.all([
      listTransactions(buildListTransactionParams()),
      listPeople(),
    ])

    applyTransactionResponse(transactionResponse)
    people.value = personRows

    if (
      transactionResponse.transactions.length === 0 &&
      transactionResponse.total > 0 &&
      pagination.page > transactionResponse.totalPages
    ) {
      pagination.page = transactionResponse.totalPages
      const adjustedResponse = await listTransactions(buildListTransactionParams())

      applyTransactionResponse(adjustedResponse)
    }
  } catch (err) {
    error.value = readError(err)
    toast.error(error.value)
  } finally {
    loading.value = false
  }
}

function applyTransactionResponse(response: ListTransactionResponse) {
  transactions.value = response.transactions
  pagination.page = response.page
  pagination.limit = response.limit
  pagination.total = response.total
  pagination.totalPages = response.totalPages
}

function buildListTransactionParams(): ListTransactionParams {
  return {
    search: filters.search.trim() || undefined,
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    page: pagination.page,
    limit: pagination.limit,
    order: filters.order,
  }
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
  pagination.page = 1
  void loadData()
}

function applyDateFilter() {
  pagination.page = 1
  void loadData()
}

function changeLimit(event: Event) {
  const select = event.target as HTMLSelectElement
  pagination.limit = Number(select.value)
  pagination.page = 1
  void loadData()
}

function changeOrder(event: Event) {
  const select = event.target as HTMLSelectElement
  filters.order = select.value === 'asc' ? 'asc' : 'desc'
  pagination.page = 1
  void loadData()
}

function goToPreviousPage() {
  if (!hasPreviousPage.value) {
    return
  }

  pagination.page -= 1
  void loadData()
}

function goToNextPage() {
  if (!hasNextPage.value) {
    return
  }

  pagination.page += 1
  void loadData()
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

async function submitForm() {
  const payload: TransactionPayload = {
    date: form.date,
    title: form.title.trim(),
    amount: Number(form.amount),
  }

  saving.value = true

  try {
    if (editing.value) {
      await updateTransaction(editing.value.transaction.id, payload)
      await persistAssignment(editing.value, form.personId)
      toast.success('Transaction updated')
    } else {
      const transaction = await createTransaction(payload)

      if (form.personId) {
        await createAssignment({
          personId: form.personId,
          transactionId: transaction.id,
        })
      }

      toast.success('Transaction created')
    }

    formOpen.value = false
    await loadData()
  } catch (err) {
    toast.error(readError(err))
  } finally {
    saving.value = false
  }
}

async function changeAssignment(item: TransactionWithPerson, event: Event) {
  const select = event.target as HTMLSelectElement
  const previousPersonId = item.person?.id ?? ''
  const nextPersonId = select.value

  if (previousPersonId === nextPersonId) {
    return
  }

  setAssignmentBusy(item.transaction.id, true)

  try {
    const transactionPerson = await persistAssignment(item, nextPersonId)
    updateTransactionAssignment(item.transaction.id, transactionPerson)
    toast.success(nextPersonId ? 'Person assigned' : 'Assignment cleared')
  } catch (err) {
    select.value = previousPersonId
    toast.error(readError(err))
  } finally {
    setAssignmentBusy(item.transaction.id, false)
  }
}

async function persistAssignment(
  item: TransactionWithPerson,
  nextPersonId: string,
): Promise<TransactionPerson | null> {
  const transactionId = item.transaction.id
  const currentAssignment = item.transactionPerson
  const currentPersonId = item.person?.id ?? ''

  if (currentPersonId === nextPersonId) {
    return currentAssignment
  }

  if (!nextPersonId && currentAssignment) {
    await deleteAssignment(currentAssignment.id)
    return null
  }

  if (nextPersonId && currentAssignment) {
    await updateAssignment(currentAssignment.id, {
      personId: nextPersonId,
      transactionId,
    })

    return {
      ...currentAssignment,
      personId: nextPersonId,
      transactionId,
      updated_at: new Date().toISOString(),
    }
  }

  if (nextPersonId) {
    return await createAssignment({
      personId: nextPersonId,
      transactionId,
    })
  }

  return null
}

function updateTransactionAssignment(transactionId: string, transactionPerson: TransactionPerson | null) {
  const person = transactionPerson
    ? people.value.find((item) => item.id === transactionPerson.personId) ?? null
    : null

  transactions.value = transactions.value.map((item) =>
    item.transaction.id === transactionId
      ? {
          ...item,
          transactionPerson,
          person,
        }
      : item,
  )
}

function setAssignmentBusy(transactionId: string, busy: boolean) {
  const next = { ...assignmentSaving.value }

  if (busy) {
    next[transactionId] = true
  } else {
    delete next[transactionId]
  }

  assignmentSaving.value = next
}

function confirmDelete(item: TransactionWithPerson) {
  deleteTarget.value = item
}

function removeTransactionFromView(transactionId: string) {
  transactions.value = transactions.value.filter((item) => item.transaction.id !== transactionId)
  pagination.total = Math.max(pagination.total - 1, 0)
  pagination.totalPages = pagination.total === 0 ? 0 : Math.ceil(pagination.total / pagination.limit)

  if (pagination.total === 0) {
    pagination.page = 1
  } else if (pagination.page > pagination.totalPages) {
    pagination.page = pagination.totalPages
  }
}

async function executeDelete() {
  if (!deleteTarget.value) {
    return
  }

  const transactionId = deleteTarget.value.transaction.id
  deleting.value = true

  try {
    await deleteTransaction(transactionId)
    removeTransactionFromView(transactionId)
    toast.success('Transaction deleted')
    deleteTarget.value = null
  } catch (err) {
    toast.error(readError(err))
  } finally {
    deleting.value = false
  }
}

function chooseFile() {
  fileInput.value?.click()
}

async function uploadFile(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]

  if (!file) {
    return
  }

  uploadPending.value = true

  try {
    await uploadTransactions(file, uploadCategory.value)
    toast.success('Import submitted')
    window.setTimeout(() => {
      void loadData()
    }, 1200)
  } catch (err) {
    toast.error(readError(err))
  } finally {
    input.value = ''
    uploadPending.value = false
  }
}

function readError(err: unknown): string {
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
        <label class="block min-w-36">
          <span class="sr-only">Category</span>
          <select
            v-model="uploadCategory"
            class="w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
            :disabled="uploadPending"
          >
            <option value="CreditCard">Credit card</option>
            <option value="Extrato">Extrato</option>
          </select>
        </label>
        <input
          ref="fileInput"
          class="hidden"
          type="file"
          accept=".csv,text/csv"
          @change="uploadFile"
        />
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
          :disabled="uploadPending"
          @click="chooseFile"
        >
          <FileUp class="size-4" aria-hidden="true" />
          {{ uploadPending ? 'Uploading...' : 'Upload CSV' }}
        </button>
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
      <div class="grid gap-3 md:grid-cols-[minmax(14rem,1.4fr)_minmax(0,1fr)_minmax(0,1fr)_10rem_10rem] md:items-end">
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
