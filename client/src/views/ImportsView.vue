<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { ChevronLeft, ChevronRight, FileUp, RefreshCw, Search, X } from 'lucide-vue-next'
import {
  listTransactionImports,
  openTransactionImportEventStream,
  uploadTransactions,
} from '../api/finance'
import { displayDateTime } from '../lib/format'
import { useToast } from '../stores/toast'
import type {
  FileCategory,
  FileProcessingStatus,
  ListTransactionImportParams,
  ListTransactionImportResponse,
  TransactionImport,
} from '../types'

const toast = useToast()
const imports = ref<TransactionImport[]>([])
const loading = ref(true)
const uploadPending = ref(false)
const error = ref('')
const fileInput = ref<HTMLInputElement | null>(null)
const uploadCategory = ref<FileCategory>('CreditCard')
const connectionState = ref<'connecting' | 'live' | 'reconnecting'>('connecting')
const pageSizeOptions = [10, 20, 50, 100]

const filters = reactive({
  search: '',
  status: '' as '' | FileProcessingStatus,
  order: 'desc' as 'asc' | 'desc',
})

const pagination = reactive({
  page: 1,
  limit: 20,
  total: 0,
  totalPages: 0,
})

let eventSource: EventSource | undefined
let searchDebounce: ReturnType<typeof window.setTimeout> | undefined
let reconcileDebounce: ReturnType<typeof window.setTimeout> | undefined
let loadSequence = 0

const visibleTotalPages = computed(() => Math.max(pagination.totalPages, 1))
const hasPreviousPage = computed(() => pagination.page > 1)
const hasNextPage = computed(() => pagination.page < pagination.totalPages)

const pageRange = computed(() => {
  if (pagination.total === 0) {
    return '0 of 0'
  }

  if (imports.value.length === 0) {
    return `0 of ${pagination.total}`
  }

  const start = (pagination.page - 1) * pagination.limit + 1
  const end = Math.min(start + imports.value.length - 1, pagination.total)
  return `${start}-${end} of ${pagination.total}`
})

onMounted(() => {
  void loadImports()
  eventSource = openTransactionImportEventStream({
    onOpen: () => {
      connectionState.value = 'live'
      void loadImports(false)
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
      pagination.page = 1
      void loadImports()
    }, 300)
  },
  { flush: 'sync' },
)

async function loadImports(showLoading = true) {
  const sequence = ++loadSequence

  if (showLoading) {
    loading.value = true
  }

  error.value = ''

  try {
    const response = await listTransactionImports(buildListParams())

    if (sequence !== loadSequence) {
      return
    }

    applyResponse(response)

    if (
      response.imports.length === 0 &&
      response.total > 0 &&
      pagination.page > response.totalPages
    ) {
      pagination.page = response.totalPages
      const adjustedResponse = await listTransactionImports(buildListParams())

      if (sequence !== loadSequence) {
        return
      }

      applyResponse(adjustedResponse)
    }
  } catch (err) {
    if (sequence !== loadSequence) {
      return
    }

    error.value = readError(err)

    if (showLoading) {
      toast.error(error.value)
    }
  } finally {
    if (sequence === loadSequence) {
      loading.value = false
    }
  }
}

function applyResponse(response: ListTransactionImportResponse) {
  imports.value = response.imports
  pagination.page = response.page
  pagination.limit = response.limit
  pagination.total = response.total
  pagination.totalPages = response.totalPages
}

function buildListParams(): ListTransactionImportParams {
  return {
    search: filters.search.trim() || undefined,
    status: filters.status || undefined,
    page: pagination.page,
    limit: pagination.limit,
    order: filters.order,
  }
}

function applyStatusUpdate(transactionImport: TransactionImport) {
  imports.value = imports.value.map((item) =>
    item.id === transactionImport.id ? transactionImport : item,
  )

  if (reconcileDebounce !== undefined) {
    window.clearTimeout(reconcileDebounce)
  }

  reconcileDebounce = window.setTimeout(() => {
    void loadImports(false)
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
  pagination.page = 1
  void loadImports()
}

function applyStatusFilter() {
  pagination.page = 1
  void loadImports()
}

function changeLimit(event: Event) {
  pagination.limit = Number((event.target as HTMLSelectElement).value)
  pagination.page = 1
  void loadImports()
}

function changeOrder(event: Event) {
  filters.order = (event.target as HTMLSelectElement).value === 'asc' ? 'asc' : 'desc'
  pagination.page = 1
  void loadImports()
}

function goToPreviousPage() {
  if (hasPreviousPage.value) {
    pagination.page -= 1
    void loadImports()
  }
}

function goToNextPage() {
  if (hasNextPage.value) {
    pagination.page += 1
    void loadImports()
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
    pagination.page = 1
    await loadImports(false)
  } catch (err) {
    toast.error(readError(err))
  } finally {
    input.value = ''
    uploadPending.value = false
  }
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
