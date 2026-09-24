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
import DataTableTextCell from '@/components/tables/DataTableTextCell.vue'
import type { DataTableHeader, DataTableRow } from '@/components/tables/types'
import type {
  AutoAssignCategoryAction,
  AutoAssignPersonAction,
  AutoAssignTransactionsPayload,
} from '@/entities/AutoAssignTransactionEntity'
import { displayAmount, displayDate } from '@/lib/format'
import { useCategoryOptionsQuery } from '@/queries/CategoryQueries'
import { usePersonOptionsQuery } from '@/queries/PersonQueries'
import {
  useAutoAssignTransactionsMutation,
  useTransactionsQuery,
  type TransactionQueryParams,
} from '@/queries/TransactionQueries'
import { useToast } from '@/stores/toast'

const tableHeaders: DataTableHeader[] = [
  { key: 'date', label: 'Date', class: 'whitespace-nowrap' },
  { key: 'title', label: 'Title', class: 'min-w-64' },
  { key: 'amount', label: 'Amount', align: 'right', class: 'whitespace-nowrap' },
  { key: 'person', label: 'Person', class: 'min-w-48' },
  { key: 'categories', label: 'Categories', class: 'min-w-64' },
]

export function useController() {
  const toast = useToast()
  const confirmOpen = ref(false)
  const debouncedTitle = ref('')

  const filters = reactive({
    title: '',
    startDate: '',
    endDate: '',
    personFilter: '',
    categoryFilter: '',
  })

  const assignment = reactive({
    personAction: 'unchanged' as AutoAssignPersonAction,
    targetPersonId: '',
    categoryAction: 'unchanged' as AutoAssignCategoryAction,
    targetCategoryIds: [] as string[],
  })

  let titleDebounce: ReturnType<typeof window.setTimeout> | undefined

  const normalizedTitle = computed(() => filters.title.trim())
  const titleFilterPending = computed(() => normalizedTitle.value !== debouncedTitle.value)

  const hasActiveFilter = computed(() => Boolean(
    normalizedTitle.value ||
    filters.startDate ||
    filters.endDate ||
    filters.personFilter ||
    filters.categoryFilter,
  ))
  const datesValid = computed(() => (
    !filters.startDate || !filters.endDate || filters.startDate <= filters.endDate
  ))
  const previewEnabled = computed(() => (
    hasActiveFilter.value && datesValid.value && !titleFilterPending.value
  ))
  const filterError = computed(() => (
    hasActiveFilter.value && !datesValid.value
      ? 'Start date must be before or equal to end date.'
      : ''
  ))
  const previewEmptyLabel = computed(() => (
    titleFilterPending.value
      ? 'Updating title filter...'
      : hasActiveFilter.value
      ? 'No matching transactions found.'
      : 'Choose at least one filter to preview transactions.'
  ))

  const transactionParams = computed<TransactionQueryParams>(() => ({
    title: debouncedTitle.value || undefined,
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    personId: filters.personFilter && filters.personFilter !== 'unassigned'
      ? filters.personFilter
      : undefined,
    unassigned: filters.personFilter === 'unassigned' ? true : undefined,
    categoryId: filters.categoryFilter && filters.categoryFilter !== 'uncategorized'
      ? filters.categoryFilter
      : undefined,
    uncategorized: filters.categoryFilter === 'uncategorized' ? true : undefined,
    order: 'desc',
  }))

  const { query: transactionQuery } = useTransactionsQuery(transactionParams, previewEnabled)
  const personOptionsQuery = usePersonOptionsQuery()
  const categoryOptionsQuery = useCategoryOptionsQuery()

  const autoAssignMutation = useAutoAssignTransactionsMutation({
    onSuccess: (response) => {
      confirmOpen.value = false
      toast.success(
        `Matched ${response.matchedCount}; changed person on ${response.personChangedCount} ` +
        `and categories on ${response.categoryChangedCount}.`,
      )
    },
    onError: (error) => {
      toast.error(readError(error))
    },
  })

  const pages = computed(() => (
    previewEnabled.value ? (transactionQuery.data.value?.pages ?? []) : []
  ))
  const transactions = computed(() => pages.value.flatMap((page) => page.transactions))
  const firstPage = computed(() => pages.value[0])
  const persons = computed(() => personOptionsQuery.data.value ?? [])
  const categories = computed(() => categoryOptionsQuery.data.value ?? [])
  const totalRows = computed(() => firstPage.value?.total ?? 0)
  const loading = computed(() => previewEnabled.value && transactionQuery.isPending.value)
  const loadingMore = computed(() => transactionQuery.isFetchingNextPage.value)
  const loadMoreFailed = computed(() => transactionQuery.isFetchNextPageError.value)
  const transactionError = computed(() => (
    previewEnabled.value ? readError(transactionQuery.error.value) : ''
  ))
  const personError = computed(() => readError(personOptionsQuery.error.value))
  const categoryError = computed(() => readError(categoryOptionsQuery.error.value))
  const applying = computed(() => autoAssignMutation.isPending.value)
  const hasNextPage = computed(() => previewEnabled.value && transactionQuery.hasNextPage.value)
  const loadProgress = computed(() => previewEnabled.value
    ? `${transactions.value.length} of ${totalRows.value}`
    : undefined)
  const summaryLabel = computed(() => previewEnabled.value ? 'Total matches' : undefined)
  const summaryValue = computed(() => previewEnabled.value ? String(totalRows.value) : undefined)

  const assignmentValid = computed(() => {
    const hasAction = assignment.personAction !== 'unchanged' ||
      assignment.categoryAction !== 'unchanged'
    const personValid = assignment.personAction !== 'set' || Boolean(assignment.targetPersonId)
    const categoriesValid = !['add', 'replace'].includes(assignment.categoryAction) ||
      assignment.targetCategoryIds.length > 0

    return hasAction && personValid && categoriesValid
  })
  const canApply = computed(() => (
    previewEnabled.value &&
    assignmentValid.value &&
    totalRows.value > 0 &&
    !transactionError.value &&
    !transactionQuery.isFetching.value &&
    !applying.value
  ))
  const assignmentDisabled = computed(() => (
    !previewEnabled.value || totalRows.value === 0 || Boolean(transactionError.value)
  ))

  const tableRows = computed<DataTableRow[]>(() => transactions.value.map((item) => ({
    key: item.transaction.id,
    cells: [
      displayDate(item.transaction.date),
      {
        component: DataTableTextCell,
        props: {
          class: 'font-medium text-stone-950',
          text: item.transaction.title,
        },
      },
      {
        component: DataTableTextCell,
        props: {
          class: 'text-stone-700',
          text: displayAmount(item.transaction.amount),
        },
      },
      item.person?.name ?? 'Unassigned',
      item.categories.length > 0
        ? item.categories.map((category) => category.title).join(', ')
        : 'Uncategorized',
    ],
  })))

  const confirmationMessage = computed(() => (
    `Apply ${describeAssignment()} to all ${totalRows.value} matching ` +
    `${totalRows.value === 1 ? 'transaction' : 'transactions'}?`
  ))

  const loadMoreTarget = ref<HTMLElement | null>(null)
  const loadMoreVisible = ref(false)
  let loadMoreObserver: IntersectionObserver | undefined

  function loadMoreIfNeeded() {
    const target = loadMoreTarget.value
    const targetBounds = target?.getBoundingClientRect()
    const targetIsNearViewport = targetBounds !== undefined &&
      targetBounds.top <= window.innerHeight + 200 &&
      targetBounds.bottom >= -200

    if (
      previewEnabled.value &&
      loadMoreVisible.value &&
      targetIsNearViewport &&
      hasNextPage.value &&
      !loadingMore.value &&
      !loadMoreFailed.value
    ) {
      loadNextPage()
    }
  }

  watch(loadMoreTarget, (target, previousTarget) => {
    if (previousTarget) loadMoreObserver?.unobserve(previousTarget)
    if (target) loadMoreObserver?.observe(target)
  }, { flush: 'post' })

  watch(loadingMore, async (isLoadingMore, wasLoadingMore) => {
    if (wasLoadingMore && !isLoadingMore) {
      await nextTick()
      loadMoreIfNeeded()
    }
  })

  watch(() => assignment.personAction, (action) => {
    if (action !== 'set') assignment.targetPersonId = ''
  })

  watch(() => assignment.categoryAction, (action) => {
    if (action !== 'add' && action !== 'replace') assignment.targetCategoryIds = []
  })

  watch(
    () => filters.title,
    () => {
      clearTitleDebounce()
      titleDebounce = window.setTimeout(() => {
        debouncedTitle.value = normalizedTitle.value
      }, 300)
    },
    { flush: 'sync' },
  )

  onMounted(() => {
    loadMoreObserver = new IntersectionObserver(([entry]) => {
      loadMoreVisible.value = entry?.isIntersecting ?? false
      loadMoreIfNeeded()
    }, { rootMargin: '200px 0px' })

    if (loadMoreTarget.value) loadMoreObserver.observe(loadMoreTarget.value)
  })

  onBeforeUnmount(() => {
    clearTitleDebounce()
    loadMoreObserver?.disconnect()
  })

  function loadData() {
    const requests: Promise<unknown>[] = [
      personOptionsQuery.refetch(),
      categoryOptionsQuery.refetch(),
    ]

    if (previewEnabled.value) requests.push(transactionQuery.refetch())
    void Promise.all(requests)
  }

  function loadNextPage() {
    if (transactionQuery.hasNextPage.value && !transactionQuery.isFetchingNextPage.value) {
      void transactionQuery.fetchNextPage()
    }
  }

  function setLoadMoreTarget(target: Element | ComponentPublicInstance | null) {
    loadMoreTarget.value = target instanceof HTMLElement ? target : null
  }

  function clearTitleDebounce() {
    if (titleDebounce !== undefined) {
      window.clearTimeout(titleDebounce)
      titleDebounce = undefined
    }
  }

  function openConfirmation() {
    if (canApply.value) confirmOpen.value = true
  }

  function closeConfirmation() {
    if (!applying.value) confirmOpen.value = false
  }

  function applyAssignments() {
    if (!canApply.value) return

    const payload: AutoAssignTransactionsPayload = {
      filter: {
        title: debouncedTitle.value || undefined,
        startDate: filters.startDate || undefined,
        endDate: filters.endDate || undefined,
        personId: filters.personFilter && filters.personFilter !== 'unassigned'
          ? filters.personFilter
          : undefined,
        unassigned: filters.personFilter === 'unassigned' ? true : undefined,
        categoryId: filters.categoryFilter && filters.categoryFilter !== 'uncategorized'
          ? filters.categoryFilter
          : undefined,
        uncategorized: filters.categoryFilter === 'uncategorized' ? true : undefined,
      },
      personAction: assignment.personAction,
      targetPersonId: assignment.personAction === 'set'
        ? assignment.targetPersonId
        : undefined,
      categoryAction: assignment.categoryAction,
      targetCategoryIds: ['add', 'replace'].includes(assignment.categoryAction)
        ? [...assignment.targetCategoryIds]
        : [],
    }

    autoAssignMutation.mutate(payload)
  }

  function describeAssignment() {
    const descriptions: string[] = []

    if (assignment.personAction === 'set') {
      const person = persons.value.find((item) => item.id === assignment.targetPersonId)
      descriptions.push(`person “${person?.name ?? 'selected person'}”`)
    } else if (assignment.personAction === 'clear') {
      descriptions.push('clearing the person')
    }

    if (assignment.categoryAction === 'add') {
      descriptions.push(`adding ${describeCategories()}`)
    } else if (assignment.categoryAction === 'replace') {
      descriptions.push(`replacing categories with ${describeCategories()}`)
    } else if (assignment.categoryAction === 'clear') {
      descriptions.push('clearing categories')
    }

    return descriptions.join(' and ')
  }

  function describeCategories() {
    const titles = categories.value
      .filter((category) => assignment.targetCategoryIds.includes(category.id))
      .map((category) => category.title)

    return titles.length === 1 ? `category “${titles[0]}”` : `${titles.length} categories`
  }

  return {
    applying,
    applyAssignments,
    assignment,
    assignmentDisabled,
    canApply,
    categories,
    categoryError,
    closeConfirmation,
    confirmationMessage,
    confirmOpen,
    filterError,
    filters,
    hasNextPage,
    loadData,
    loadMoreFailed,
    loadNextPage,
    loadProgress,
    loading,
    loadingMore,
    openConfirmation,
    personError,
    persons,
    previewEmptyLabel,
    setLoadMoreTarget,
    summaryLabel,
    summaryValue,
    tableHeaders,
    tableRows,
    transactionError,
  }
}

function readError(error: unknown) {
  return error instanceof Error ? error.message : ''
}
