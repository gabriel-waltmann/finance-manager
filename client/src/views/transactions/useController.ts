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
import DataTableTextCell from '../../components/tables/DataTableTextCell.vue'
import type {
  DataTableHeader,
  DataTableRow,
} from '../../components/tables/types'
import { displayAmount, displayDate, inputDate, todayInputDate } from '../../lib/format'
import { usePeopleQuery } from '../../queries/PersonQueries'
import {
  useAssignmentMutation,
  useDeleteTransactionMutation,
  usePendingAssignmentIds,
  useSaveTransactionMutation,
  useTransactionsQuery,
  type TransactionQueryParams,
} from '../../queries/TransactionQueries'
import { useToast } from '../../stores/toast'
import type {
  TransactionPayload,
  TransactionWithPerson,
} from '../../entities/TransactionEntity'
import TransactionActionsCell from './components/TransactionActionsCell.vue'
import TransactionPersonCell from './components/TransactionPersonCell.vue'

const tableHeaders: DataTableHeader[] = [
  { key: 'date', label: 'Date', class: 'whitespace-nowrap' },
  { key: 'title', label: 'Title', class: 'min-w-64' },
  { key: 'amount', label: 'Amount', align: 'right', class: 'whitespace-nowrap' },
  { key: 'person', label: 'Person', class: 'min-w-56' },
  { key: 'actions', label: 'Actions', align: 'right', class: 'w-40' },
]

export function useController() {
  const toast = useToast()

  const formOpen = ref(false)
  const editing = ref<TransactionWithPerson | null>(null)
  const deleteTarget = ref<TransactionWithPerson | null>(null)
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

  const transactionParams = computed<TransactionQueryParams>(() => ({
    search: debouncedSearch.value || undefined,
    startDate: filters.startDate || undefined,
    endDate: filters.endDate || undefined,
    personId: filters.personFilter && filters.personFilter !== 'unassigned'
      ? filters.personFilter
      : undefined,
    unassigned: filters.personFilter === 'unassigned' ? true : undefined,
    order: filters.order,
  }))

  const { query: transactionQuery, queryKey: transactionQueryKey } = useTransactionsQuery(
    transactionParams,
  )
  const peopleQuery = usePeopleQuery()

  const pages = computed(() => transactionQuery.data.value?.pages ?? [])
  const transactions = computed(() => pages.value.flatMap((page) => page.transactions))
  const firstPage = computed(() => pages.value[0])
  const people = computed(() => peopleQuery.data.value ?? [])
  const loading = computed(() => transactionQuery.isPending.value)
  const loadingMore = computed(() => transactionQuery.isFetchingNextPage.value)
  const loadMoreFailed = computed(() => transactionQuery.isFetchNextPageError.value)
  const transactionError = computed(() => readError(transactionQuery.error.value))
  const peopleError = computed(() => readError(peopleQuery.error.value))
  const totalRows = computed(() => firstPage.value?.total ?? 0)

  const hasNextPage = computed(() => transactionQuery.hasNextPage.value)
  const loadProgress = computed(() => `${transactions.value.length} of ${totalRows.value}`)
  const loadMoreTarget = ref<HTMLElement | null>(null)
  const loadMoreVisible = ref(false)
  let loadMoreObserver: IntersectionObserver | undefined

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

  const deleteTransactionMutation = useDeleteTransactionMutation(transactionQueryKey, {
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
  const tableRows = computed<DataTableRow[]>(() =>
    transactions.value.map((item) => ({
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
        {
          component: TransactionPersonCell,
          props: {
            disabled: Boolean(assignmentSaving.value[item.transaction.id]),
            people: people.value,
            personId: item.person?.id ?? '',
            onChange: (event: Event) => changeAssignment(item, event),
          },
        },
        {
          component: TransactionActionsCell,
          props: {
            onDeleteTransaction: () => confirmDelete(item),
            onEdit: () => openEditForm(item),
          },
        },
      ],
    })),
  )

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

  watch(
    () => filters.search,
    () => {
      clearSearchDebounce()

      searchDebounce = window.setTimeout(() => {
        debouncedSearch.value = filters.search.trim()
      }, 300)
    },
    { flush: 'sync' },
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
    clearSearchDebounce()
    loadMoreObserver?.disconnect()
  })

  function loadData() {
    void Promise.all([
      transactionQuery.refetch(),
      peopleQuery.refetch(),
    ])
  }

  function loadNextPage() {
    if (transactionQuery.hasNextPage.value && !transactionQuery.isFetchingNextPage.value) {
      void transactionQuery.fetchNextPage()
    }
  }

  function setLoadMoreTarget(target: Element | ComponentPublicInstance | null) {
    loadMoreTarget.value = target instanceof HTMLElement ? target : null
  }

  function clearSearchDebounce() {
    if (searchDebounce !== undefined) {
      window.clearTimeout(searchDebounce)
      searchDebounce = undefined
    }
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

  function cancelDelete() {
    if (!deleting.value) {
      deleteTarget.value = null
    }
  }

  function executeDelete() {
    if (!deleteTarget.value) {
      return
    }

    deleteTransactionMutation.mutate(deleteTarget.value.transaction.id)
  }

  return {
    cancelDelete,
    closeForm,
    deleteTarget,
    deleting,
    editing,
    executeDelete,
    filters,
    form,
    formOpen,
    hasNextPage,
    loadData,
    loadMoreFailed,
    loadNextPage,
    loadProgress,
    loading,
    loadingMore,
    openCreateForm,
    people,
    peopleError,
    saving,
    setLoadMoreTarget,
    submitForm,
    tableHeaders,
    tableRows,
    transactionError,
  }
}

function readError(err: unknown): string {
  if (!err) {
    return ''
  }

  return err instanceof Error ? err.message : 'Something went wrong'
}
