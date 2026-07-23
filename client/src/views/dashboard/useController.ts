import { computed, reactive, ref, watch } from 'vue'
import type { DashboardParams } from '../../entities/Dashboard'
import { displayAmount } from '../../lib/format'
import { useDashboardQuery } from '../../queries/DashboardQueries'
import { usePeopleQuery } from '../../queries/PersonQueries'
import { useToast } from '../../stores/toast'

export function useController() {
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
  const totalSpend = computed(() => dashboardQuery.data.value?.totalAmount ?? 0)
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

  function changeLimit(value: number) {
    limit.value = value
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

  return {
    applyFilters,
    changeLimit,
    chartItems,
    displayAmount,
    error,
    filters,
    goToNextPage,
    goToPreviousPage,
    hasNextPage,
    hasPreviousPage,
    loadDashboard,
    loading,
    pageRange,
    pageSizeOptions,
    pagination,
    people,
    refreshing,
    totalSpend,
    visibleTotalPages,
  }
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
  if (!err) {
    return ''
  }

  return err instanceof Error ? err.message : 'Something went wrong'
}
