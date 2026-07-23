<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { ChevronLeft, ChevronRight, FileUp, RefreshCw, Search, X } from 'lucide-vue-next'
import { displayDateTime } from '../lib/format'
import {
  openTransactionImportEventStream,
  useTransactionImportCache,
  useTransactionImportsQuery,
  useUploadTransactionMutation,
} from '../queries/TransactionImportQueries'
import { useToast } from '../stores/toast'
import type {
  FileCategory,
  FileProcessingStatus,
  ListTransactionImportParams,
  TransactionImportEntity,
} from '../entities/TransactionImportEntity'

const toast = useToast()
const fileInput = ref<HTMLInputElement | null>(null)
const uploadCategory = ref<FileCategory>('CreditCard')
const connectionState = ref<'connecting' | 'live' | 'reconnecting'>('connecting')
const pageSizeOptions = [10, 20, 50, 100]
const page = ref(1)
const limit = ref(20)
const debouncedSearch = ref('')

const filters = reactive({
  search: '',
  status: '' as '' | FileProcessingStatus,
  order: 'desc' as 'asc' | 'desc',
})

let eventSource: EventSource | undefined
let searchDebounce: ReturnType<typeof window.setTimeout> | undefined
let reconcileDebounce: ReturnType<typeof window.setTimeout> | undefined

const importParams = computed<ListTransactionImportParams>(() => ({
  search: debouncedSearch.value || undefined,
  status: filters.status || undefined,
  page: page.value,
  limit: limit.value,
  order: filters.order,
}))

const importsQuery = useTransactionImportsQuery(importParams)
const importCache = useTransactionImportCache()

const uploadMutation = useUploadTransactionMutation({
  onSuccess: () => {
    toast.success('Import submitted')
    page.value = 1
  },
  onError: (error) => {
    toast.error(readError(error))
  },
  onSettled: (variables) => {
    variables.input.value = ''
  },
})

const imports = computed(() => importsQuery.data.value?.imports ?? [])
const loading = computed(() => importsQuery.isPending.value)
const uploadPending = computed(() => uploadMutation.isPending.value)
const error = computed(() => readError(importsQuery.error.value))
const pagination = computed(() => ({
  page: importsQuery.data.value?.page ?? page.value,
  limit: importsQuery.data.value?.limit ?? limit.value,
  total: importsQuery.data.value?.total ?? 0,
  totalPages: importsQuery.data.value?.totalPages ?? 0,
}))

const visibleTotalPages = computed(() => Math.max(pagination.value.totalPages, 1))
const hasPreviousPage = computed(() => pagination.value.page > 1)
const hasNextPage = computed(() => pagination.value.page < pagination.value.totalPages)

const pageRange = computed(() => {
  if (pagination.value.total === 0) {
    return '0 of 0'
  }

  if (imports.value.length === 0) {
    return `0 of ${pagination.value.total}`
  }

  const start = (pagination.value.page - 1) * pagination.value.limit + 1
  const end = Math.min(start + imports.value.length - 1, pagination.value.total)
  return `${start}-${end} of ${pagination.value.total}`
})

onMounted(() => {
  eventSource = openTransactionImportEventStream({
    onOpen: () => {
      connectionState.value = 'live'
      void importCache.invalidate()
    },
    onError: () => {
      connectionState.value = 'reconnecting'
    },
    onStatus: applyStatusUpdate,
  })
})

onBeforeUnmount(() => {
  eventSource?.close()
  clearSearchDebounce()

  if (reconcileDebounce !== undefined) {
    window.clearTimeout(reconcileDebounce)
  }
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
  () => importsQuery.data.value,
  (response) => {
    if (
      response &&
      response.imports.length === 0 &&
      response.total > 0 &&
      page.value > response.totalPages
    ) {
      page.value = response.totalPages
    }
  },
)

watch(
  () => importsQuery.error.value,
  (queryError) => {
    if (queryError) {
      toast.error(readError(queryError))
    }
  },
)

function loadImports() {
  void importsQuery.refetch()
}

function applyStatusUpdate(transactionImport: TransactionImportEntity) {
  importCache.update(transactionImport)

  if (reconcileDebounce !== undefined) {
    window.clearTimeout(reconcileDebounce)
  }

  reconcileDebounce = window.setTimeout(() => {
    void importCache.invalidate()
  }, 150)
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

function applyStatusFilter() {
  page.value = 1
}

function changeLimit(event: Event) {
  limit.value = Number((event.target as HTMLSelectElement).value)
  page.value = 1
}

function changeOrder(event: Event) {
  filters.order = (event.target as HTMLSelectElement).value === 'asc' ? 'asc' : 'desc'
  page.value = 1
}

function goToPreviousPage() {
  if (hasPreviousPage.value) {
    page.value -= 1
  }
}

function goToNextPage() {
  if (hasNextPage.value) {
    page.value += 1
  }
}

function chooseFile() {
  fileInput.value?.click()
}

function uploadFile(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]

  if (!file) {
    return
  }

  uploadMutation.mutate({
    file,
    category: uploadCategory.value,
    input,
  })
}

function displayCategory(category: FileCategory): string {
  return category === 'CreditCard' ? 'Credit card' : 'Extrato'
}

function statusClasses(status: FileProcessingStatus): string {
  switch (status) {
    case 'Submitted':
      return 'bg-sky-50 text-sky-700 ring-sky-600/20'
    case 'Processing':
      return 'bg-amber-50 text-amber-700 ring-amber-600/20'
    case 'Finished':
      return 'bg-emerald-50 text-emerald-700 ring-emerald-600/20'
    case 'Failed':
      return 'bg-rose-50 text-rose-700 ring-rose-600/20'
  }
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
        <p class="text-sm font-medium text-emerald-700">Import queue</p>
        <h1 class="mt-1 text-2xl font-semibold text-stone-950">Transaction imports</h1>
      </div>

      <div class="flex flex-wrap gap-3">
        <div
          class="inline-flex items-center gap-2 rounded-md border border-stone-200 bg-white px-3 py-2 text-sm text-stone-600"
          role="status"
          aria-live="polite"
        >
          <span
            class="size-2 rounded-full"
            :class="connectionState === 'live' ? 'bg-emerald-500' : 'animate-pulse bg-amber-500'"
            aria-hidden="true"
          />
          {{ connectionState === 'live' ? 'Live updates' : connectionState === 'connecting' ? 'Connecting' : 'Reconnecting' }}
        </div>
        <label class="block min-w-36">
          <span class="sr-only">Import category</span>
          <select
            v-model="uploadCategory"
            class="w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
            :disabled="uploadPending"
          >
            <option value="CreditCard">Credit card</option>
            <option value="Extrato">Extrato</option>
          </select>
        </label>
        <input ref="fileInput" class="hidden" type="file" accept=".csv,text/csv" @change="uploadFile" />
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md bg-emerald-600 px-3 py-2 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
          :disabled="uploadPending"
          @click="chooseFile"
        >
          <FileUp class="size-4" aria-hidden="true" />
          {{ uploadPending ? 'Uploading...' : 'Upload CSV' }}
        </button>
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          @click="loadImports()"
        >
          <RefreshCw class="size-4" aria-hidden="true" />
          Refresh
        </button>
      </div>
    </div>

    <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
      <div class="grid gap-3 md:grid-cols-2 lg:grid-cols-[minmax(14rem,1fr)_minmax(0,1fr)_10rem_10rem] lg:items-end">
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Search</span>
          <span class="relative mt-1 block">
            <Search class="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-stone-400" aria-hidden="true" />
            <input
              v-model="filters.search"
              class="w-full rounded-md border border-stone-300 py-2 pl-9 pr-10 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              type="search"
              placeholder="File name"
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
          <span class="text-sm font-medium text-stone-700">Status</span>
          <select
            v-model="filters.status"
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            @change="applyStatusFilter"
          >
            <option value="">All statuses</option>
            <option value="Submitted">Submitted</option>
            <option value="Processing">Processing</option>
            <option value="Finished">Finished</option>
            <option value="Failed">Failed</option>
          </select>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Rows</span>
          <select
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            :value="pagination.limit"
            @change="changeLimit"
          >
            <option v-for="option in pageSizeOptions" :key="option" :value="option">{{ option }}</option>
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
      <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">Loading imports...</div>
      <div v-else-if="imports.length === 0" class="px-4 py-12 text-center text-sm text-stone-500">No imports found.</div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-stone-200 text-left text-sm">
          <thead class="bg-stone-100 text-xs uppercase text-stone-500">
            <tr>
              <th class="min-w-64 px-4 py-3 font-semibold">File</th>
              <th class="px-4 py-3 font-semibold">Category</th>
              <th class="px-4 py-3 font-semibold">Status</th>
              <th class="px-4 py-3 text-right font-semibold">Imported</th>
              <th class="px-4 py-3 font-semibold">Submitted</th>
              <th class="px-4 py-3 font-semibold">Last update</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr v-for="item in imports" :key="item.id" class="hover:bg-stone-50">
              <td class="px-4 py-3 font-medium text-stone-950">{{ item.fileName }}</td>
              <td class="whitespace-nowrap px-4 py-3 text-stone-600">{{ displayCategory(item.category) }}</td>
              <td class="whitespace-nowrap px-4 py-3">
                <span class="inline-flex rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset" :class="statusClasses(item.status)">
                  {{ item.status }}
                </span>
              </td>
              <td class="whitespace-nowrap px-4 py-3 text-right text-stone-700">{{ item.transactionCount }}</td>
              <td class="whitespace-nowrap px-4 py-3 text-stone-600">{{ displayDateTime(item.createdAt) }}</td>
              <td class="whitespace-nowrap px-4 py-3 text-stone-600">{{ displayDateTime(item.updatedAt) }}</td>
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
          <span class="min-w-20 text-center font-medium text-stone-700">{{ pagination.page }} / {{ visibleTotalPages }}</span>
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
</template>
