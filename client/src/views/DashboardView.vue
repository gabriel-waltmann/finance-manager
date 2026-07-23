<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { ChevronLeft, ChevronRight, RefreshCw } from 'lucide-vue-next'
import HorizontalBarChart from '../components/HorizontalBarChart.vue'
import { displayAmount, displayDate } from '../lib/format'
import { useDashboardQuery } from '../queries/DashboardQueries'
import { usePeopleQuery } from '../queries/PersonQueries'
import { useToast } from '../stores/toast'
import type { DashboardParams } from '../entities/Dashboard'

const toast = useToast()

const defaultRange = getPreviousMonthRange()
const pageSizeOptions = [10, 20, 50, 100]
const page = ref(1)
const limit = ref(20)

const filters = reactive({
  startDate: defaultRange.startDate,
  endDate: defaultRange.endDate,
  personId: '',
  order: 'desc' as 'asc' | 'desc',
})

const dashboardParams = computed<DashboardParams>(() => ({
  startDate: filters.startDate || undefined,
  endDate: filters.endDate || undefined,
  personId: filters.personId || undefined,
  page: page.value,
  limit: limit.value,
  order: filters.order,
}))

const dashboardQuery = useDashboardQuery(dashboardParams)
const peopleQuery = usePeopleQuery()

const topItems = computed(() => dashboardQuery.data.value?.topItems ?? [])
const fixedSpends = computed(() => dashboardQuery.data.value?.fixedSpends ?? [])
const dashboardTotalAmount = computed(() => dashboardQuery.data.value?.totalAmount ?? 0)
const people = computed(() => peopleQuery.data.value ?? [])
const loading = computed(() => dashboardQuery.isPending.value)
const refreshing = computed(() => dashboardQuery.isFetching.value)
const error = computed(() => readError(dashboardQuery.error.value ?? peopleQuery.error.value))

const pagination = computed(() => ({
  page: dashboardQuery.data.value?.page ?? page.value,
  limit: dashboardQuery.data.value?.limit ?? limit.value,
  total: dashboardQuery.data.value?.total ?? 0,
  totalPages: dashboardQuery.data.value?.totalPages ?? 0,
}))

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

const visibleTotalPages = computed(() => Math.max(pagination.value.totalPages, 1))
const hasPreviousPage = computed(() => pagination.value.page > 1)
const hasNextPage = computed(() => pagination.value.page < pagination.value.totalPages)

const pageRange = computed(() => {
  if (pagination.value.total === 0) {
    return '0 of 0'
  }

  if (topItems.value.length === 0) {
    return `0 of ${pagination.value.total}`
  }

  const start = (pagination.value.page - 1) * pagination.value.limit + 1
  const end = Math.min(start + topItems.value.length - 1, pagination.value.total)

  return `${start}-${end} of ${pagination.value.total}`
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

watch(
  () => dashboardQuery.data.value,
  (dashboard) => {
    if (
      dashboard &&
      dashboard.topItems.length === 0 &&
      dashboard.total > 0 &&
      page.value > dashboard.totalPages
    ) {
      page.value = dashboard.totalPages
    }
  },
)

watch(
  () => dashboardQuery.error.value,
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

function loadDashboard() {
  void dashboardQuery.refetch()
}

function applyFilters() {
  page.value = 1
}

function changeLimit(event: Event) {
  const select = event.target as HTMLSelectElement
  limit.value = Number(select.value)
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

function displayMonth(value: string): string {
  const [year, month] = value.split('-')

  if (!year || !month) {
    return value
  }

  return `${month}/${year}`
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
        <p class="text-sm font-medium text-emerald-700">Spending dashboard</p>
        <h1 class="mt-1 text-2xl font-semibold text-stone-950">Dashboard</h1>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="refreshing"
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

    <div class="rounded-lg border border-stone-200 bg-white">
      <div class="border-b border-stone-200 px-4 py-3">
        <h2 class="text-base font-semibold text-stone-950">Fixed spends</h2>
        <p class="mt-1 text-sm text-stone-500">{{ selectedRangeLabel }}</p>
      </div>
      <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">Loading fixed spends...</div>
      <div v-else-if="fixedSpends.length === 0" class="px-4 py-12 text-center text-sm text-stone-500">
        No fixed spends found for this range.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full table-fixed divide-y divide-stone-200 text-sm">
          <colgroup>
            <col class="w-full" />
            <col class="w-20" />
            <col class="w-28" />
            <col class="w-28" />
          </colgroup>
          <thead class="bg-stone-50 text-left text-xs font-semibold uppercase text-stone-500">
            <tr>
              <th scope="col" class="px-4 py-3">Title</th>
              <th scope="col" class="px-4 py-3">Months</th>
              <th scope="col" class="px-4 py-3">Last month</th>
              <th scope="col" class="px-4 py-3 text-right">Amount</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100 bg-white">
            <tr v-for="item in fixedSpends" :key="item.title" class="hover:bg-stone-50">
              <td class="max-w-0 px-4 py-3 font-medium text-stone-950">
                <span class="block truncate">{{ item.title }}</span>
              </td>
              <td class="whitespace-nowrap px-4 py-3 text-stone-600">{{ item.monthCount }}</td>
              <td class="whitespace-nowrap px-4 py-3 text-stone-600">{{ displayMonth(item.lastMonth) }}</td>
              <td class="whitespace-nowrap px-4 py-3 text-right font-semibold text-stone-900">
                {{ displayAmount(item.lastAmount) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>
