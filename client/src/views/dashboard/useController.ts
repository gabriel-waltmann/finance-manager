import {
  computed,
  type ComponentPublicInstance,
  nextTick,
  onBeforeUnmount,
  onMounted,
  reactive,
  ref,
  watch,
} from 'vue'
import { displayAmount } from '../../lib/format'
import {
  useDashboardQuery,
  type DashboardQueryParams,
} from '../../queries/DashboardQueries'
import { usePeopleQuery } from '../../queries/PersonQueries'
import { useToast } from '../../stores/toast'

export function useController() {
  const toast = useToast()

  const defaultRange = getPreviousMonthRange()

  const filters = reactive({
    startDate: defaultRange.startDate,
    endDate: defaultRange.endDate,
    personId: '',
    order: 'desc' as 'asc' | 'desc',
  })

  const dashboardParams = computed<DashboardQueryParams>(() => ({
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    personId: filters.personId || undefined,
    limit: 20,
    order: filters.order,
  }))

  const dashboardQuery = useDashboardQuery(dashboardParams)
  const peopleQuery = usePeopleQuery()

  const pages = computed(() => dashboardQuery.data.value?.pages ?? [])
  const topItems = computed(() => pages.value.flatMap((page) => page.topItems))
  const firstPage = computed(() => pages.value[0])
  const totalSpend = computed(() => firstPage.value?.totalAmount ?? 0)
  const totalItems = computed(() => firstPage.value?.total ?? 0)
  const people = computed(() => peopleQuery.data.value ?? [])
  const loading = computed(() => dashboardQuery.isPending.value)
  const loadingMore = computed(() => dashboardQuery.isFetchingNextPage.value)
  const loadMoreFailed = computed(() => dashboardQuery.isFetchNextPageError.value)
  const refreshing = computed(() => dashboardQuery.isRefetching.value)
  const error = computed(() => readError(dashboardQuery.error.value ?? peopleQuery.error.value))

  const chartItems = computed(() =>
    topItems.value.map((item) => ({
      key: item.title,
      label: item.title,
      value: item.totalAmount,
      detail: `${item.transactionCount} ${item.transactionCount === 1 ? 'transaction' : 'transactions'}`,
    })),
  )

  const hasNextPage = computed(() => dashboardQuery.hasNextPage.value)
  const loadProgress = computed(() => `${topItems.value.length} of ${totalItems.value}`)
  const loadMoreTarget = ref<HTMLElement | null>(null)
  const loadMoreVisible = ref(false)
  let loadMoreObserver: IntersectionObserver | undefined

  function loadMoreIfNeeded() {
    const target = loadMoreTarget.value
    const targetBounds = target?.getBoundingClientRect()
    const targetIsNearViewport =
      targetBounds !== undefined &&
      targetBounds.top <= window.innerHeight + 200 &&
      targetBounds.bottom >= -200

    if (
      loadMoreVisible.value &&
      targetIsNearViewport &&
      hasNextPage.value &&
      !loadingMore.value &&
      !loadMoreFailed.value
    ) {
      loadNextPage()
    }
  }

  watch(
    loadMoreTarget,
    (target, previousTarget) => {
      if (previousTarget) {
        loadMoreObserver?.unobserve(previousTarget)
      }

      if (target) {
        loadMoreObserver?.observe(target)
      }
    },
    { flush: 'post' },
  )

  watch(loadingMore, async (isLoadingMore, wasLoadingMore) => {
    if (wasLoadingMore && !isLoadingMore) {
      await nextTick()
      loadMoreIfNeeded()
    }
  })

  onMounted(() => {
    loadMoreObserver = new IntersectionObserver(
      ([entry]) => {
        loadMoreVisible.value = entry?.isIntersecting ?? false
        loadMoreIfNeeded()
      },
      { rootMargin: '200px 0px' },
    )

    if (loadMoreTarget.value) {
      loadMoreObserver.observe(loadMoreTarget.value)
    }
  })

  onBeforeUnmount(() => {
    loadMoreObserver?.disconnect()
  })

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

  function setLoadMoreTarget(target: Element | ComponentPublicInstance | null) {
    loadMoreTarget.value = target instanceof HTMLElement ? target : null
  }

  function loadNextPage() {
    if (dashboardQuery.hasNextPage.value && !dashboardQuery.isFetchingNextPage.value) {
      void dashboardQuery.fetchNextPage()
    }
  }

  return {
    chartItems,
    displayAmount,
    error,
    filters,
    hasNextPage,
    loadDashboard,
    loadMoreFailed,
    loadNextPage,
    loadProgress,
    loading,
    loadingMore,
    people,
    refreshing,
    setLoadMoreTarget,
    totalSpend,
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
