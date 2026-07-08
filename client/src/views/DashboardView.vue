<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ChevronLeft, ChevronRight, RefreshCw } from 'lucide-vue-next'
import HorizontalBarChart from '../components/HorizontalBarChart.vue'
import { getDashboard, listPeople } from '../api/finance'
import { displayAmount, displayDate } from '../lib/format'
import { useToast } from '../stores/toast'
import type { DashboardTopItem, GetDashboardResponse, Person } from '../types'

const toast = useToast()

const defaultRange = getPreviousMonthRange()
const topItems = ref<DashboardTopItem[]>([])
const dashboardTotalAmount = ref(0)
const people = ref<Person[]>([])
const pageSizeOptions = [10, 20, 50, 100]
const loading = ref(true)
const error = ref('')

const filters = reactive({
  startDate: defaultRange.startDate,
  endDate: defaultRange.endDate,
  personId: '',
  order: 'desc' as 'asc' | 'desc',
})

const pagination = reactive({
  page: 1,
  limit: 20,
  total: 0,
  totalPages: 0,
})

const chartItems = computed(() =>
  topItems.value.map((item) => ({
    key: item.title,
    label: item.title,
    value: item.totalAmount,
    detail: `${item.transactionCount} ${item.transactionCount === 1 ? 'transaction' : 'transactions'}`,
  })),
)

const totalSpend = computed(() => dashboardTotalAmount.value)

const totalTransactions = computed(() =>
  topItems.value.reduce((total, item) => total + item.transactionCount, 0),
)

const visibleTotalPages = computed(() => Math.max(pagination.totalPages, 1))
const hasPreviousPage = computed(() => pagination.page > 1)
const hasNextPage = computed(() => pagination.page < pagination.totalPages)

const pageRange = computed(() => {
  if (pagination.total === 0) {
    return '0 of 0'
  }

  if (topItems.value.length === 0) {
    return `0 of ${pagination.total}`
  }

  const start = (pagination.page - 1) * pagination.limit + 1
  const end = Math.min(start + topItems.value.length - 1, pagination.total)

  return `${start}-${end} of ${pagination.total}`
})

const selectedRangeLabel = computed(() => {
  if (filters.startDate && filters.endDate) {
    return `${displayDate(filters.startDate)} to ${displayDate(filters.endDate)}`
  }

  if (filters.startDate) {
    return `From ${displayDate(filters.startDate)}`
  }

  if (filters.endDate) {
    return `Until ${displayDate(filters.endDate)}`
  }

  return 'All dates'
})

onMounted(() => {
  void loadData()
})

async function loadData() {
  loading.value = true
  error.value = ''

  try {
    const [dashboard, personRows] = await Promise.all([
      getDashboard(buildDashboardParams()),
      listPeople(),
    ])

    applyDashboardResponse(dashboard)
    people.value = personRows

    if (
      dashboard.topItems.length === 0 &&
      dashboard.total > 0 &&
      pagination.page > dashboard.totalPages
    ) {
      pagination.page = dashboard.totalPages
      const adjustedDashboard = await getDashboard(buildDashboardParams())

      applyDashboardResponse(adjustedDashboard)
    }
  } catch (err) {
    error.value = readError(err)
    toast.error(error.value)
  } finally {
    loading.value = false
  }
}

async function loadDashboard() {
  loading.value = true
  error.value = ''

  try {
    const dashboard = await getDashboard(buildDashboardParams())

    applyDashboardResponse(dashboard)

    if (
      dashboard.topItems.length === 0 &&
      dashboard.total > 0 &&
      pagination.page > dashboard.totalPages
    ) {
      pagination.page = dashboard.totalPages
      const adjustedDashboard = await getDashboard(buildDashboardParams())

      applyDashboardResponse(adjustedDashboard)
    }
  } catch (err) {
    error.value = readError(err)
    toast.error(error.value)
  } finally {
    loading.value = false
  }
}

function applyDashboardResponse(response: GetDashboardResponse) {
  topItems.value = response.topItems
  dashboardTotalAmount.value = response.totalAmount
  pagination.page = response.page
  pagination.limit = response.limit
  pagination.total = response.total
  pagination.totalPages = response.totalPages
}

function buildDashboardParams() {
  return {
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    personId: filters.personId || undefined,
    page: pagination.page,
    limit: pagination.limit,
    order: filters.order,
  }
}

function applyFilters() {
  pagination.page = 1
  void loadDashboard()
}

function changeLimit(event: Event) {
  const select = event.target as HTMLSelectElement
  pagination.limit = Number(select.value)
  pagination.page = 1
  void loadDashboard()
}

function goToPreviousPage() {
  if (!hasPreviousPage.value) {
    return
  }

  pagination.page -= 1
  void loadDashboard()
}

function goToNextPage() {
  if (!hasNextPage.value) {
    return
  }

  pagination.page += 1
  void loadDashboard()
}

function getPreviousMonthRange() {
  const now = new Date()
  const firstDay = new Date(now.getFullYear(), now.getMonth() - 1, 1)
  const lastDay = new Date(now.getFullYear(), now.getMonth(), 0)

  return {
    startDate: toInputDate(firstDay),
    endDate: toInputDate(lastDay),
  }
}

function toInputDate(date: Date): string {
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)

  return localDate.toISOString().slice(0, 10)
}

function readError(err: unknown): string {
  return err instanceof Error ? err.message : 'Something went wrong'
}
</script>

<template>
  <section class="space-y-5">
    <div class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
      <div>
        <p class="text-sm font-medium text-emerald-700">Spending dashboard</p>
        <h1 class="mt-1 text-2xl font-semibold text-stone-950">Dashboard</h1>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="loading"
        @click="loadDashboard"
      >
        <RefreshCw class="size-4" aria-hidden="true" />
        Refresh
      </button>
    </div>

    <div class="grid gap-3 md:grid-cols-3">
      <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
        <p class="text-xs font-medium uppercase text-stone-500">Top items</p>
        <p class="mt-1 text-xl font-semibold text-stone-950">{{ pagination.total }}</p>
      </div>
      <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
        <p class="text-xs font-medium uppercase text-stone-500">Transactions</p>
        <p class="mt-1 text-xl font-semibold text-stone-950">{{ totalTransactions }}</p>
      </div>
      <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
        <p class="text-xs font-medium uppercase text-stone-500">Spend</p>
        <p class="mt-1 text-xl font-semibold text-stone-950">{{ displayAmount(totalSpend) }}</p>
      </div>
    </div>

    <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
      <div class="grid gap-3 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_10rem_8rem] md:items-end">
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Start date</span>
          <input
            v-model="filters.startDate"
            class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            type="date"
            @change="applyFilters"
          />
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">End date</span>
          <input
            v-model="filters.endDate"
            class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            type="date"
            @change="applyFilters"
          />
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Person</span>
          <select
            v-model="filters.personId"
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            @change="applyFilters"
          >
            <option value="">All people</option>
            <option v-for="person in people" :key="person.id" :value="person.id">
              {{ person.name }}
            </option>
          </select>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Order</span>
          <select
            v-model="filters.order"
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            @change="applyFilters"
          >
            <option value="desc">Highest</option>
            <option value="asc">Lowest</option>
          </select>
        </label>
        <label class="block">
          <span class="text-sm font-medium text-stone-700">Page size</span>
          <select
            :value="pagination.limit"
            class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
            @change="changeLimit"
          >
            <option v-for="option in pageSizeOptions" :key="option" :value="option">
              {{ option }}
            </option>
          </select>
        </label>
      </div>
    </div>

    <div v-if="error" class="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ error }}
    </div>

    <div class="rounded-lg border border-stone-200 bg-white">
      <div class="border-b border-stone-200 px-4 py-3">
        <h2 class="text-base font-semibold text-stone-950">Top spend items</h2>
        <p class="mt-1 text-sm text-stone-500">{{ selectedRangeLabel }}</p>
      </div>
      <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">Loading dashboard...</div>
      <div v-else class="px-4 py-4">
        <HorizontalBarChart
          :items="chartItems"
          empty-label="No spend found for this range."
          :value-formatter="displayAmount"
        />
        <div class="mt-4 flex items-center justify-between border-t border-stone-200 pt-4">
          <span class="text-sm font-medium text-stone-600">Total spend</span>
          <span class="text-base font-semibold text-stone-950">{{ displayAmount(totalSpend) }}</span>
        </div>
        <div class="mt-4 flex flex-col gap-3 border-t border-stone-200 pt-4 text-sm text-stone-600 md:flex-row md:items-center md:justify-between">
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
    </div>
  </section>
</template>
