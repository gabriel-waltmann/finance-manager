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
import type {
  FileCategory,
  FileProcessingStatus,
  TransactionImportEntity,
} from '../../entities/TransactionImportEntity'
import { displayDateTime } from '../../lib/format'
import {
  openTransactionImportEventStream,
  useTransactionImportCache,
  useTransactionImportsQuery,
  useUploadTransactionMutation,
  type TransactionImportQueryParams,
} from '../../queries/TransactionImportQueries'
import { useToast } from '../../stores/toast'
import ImportStatusCell from './components/ImportStatusCell.vue'

const tableHeaders: DataTableHeader[] = [
  { key: 'file', label: 'File', class: 'min-w-64' },
  { key: 'category', label: 'Category', class: 'whitespace-nowrap' },
  { key: 'status', label: 'Status', class: 'whitespace-nowrap' },
  { key: 'imported', label: 'Imported', align: 'right', class: 'whitespace-nowrap' },
  { key: 'submitted', label: 'Submitted', class: 'whitespace-nowrap' },
  { key: 'updated', label: 'Last update', class: 'whitespace-nowrap' },
]

export function useController() {
  const toast = useToast()

  const uploadCategory = ref<FileCategory>('CreditCard')
  const connectionState = ref<'connecting' | 'live' | 'reconnecting'>('connecting')
  const debouncedSearch = ref('')

  const filters = reactive({
    search: '',
    status: '' as '' | FileProcessingStatus,
    order: 'desc' as 'asc' | 'desc',
  })

  let eventSource: EventSource | undefined
  let searchDebounce: ReturnType<typeof window.setTimeout> | undefined
  let reconcileDebounce: ReturnType<typeof window.setTimeout> | undefined

  const importParams = computed<TransactionImportQueryParams>(() => ({
    search: debouncedSearch.value || undefined,
    status: filters.status || undefined,
    order: filters.order,
  }))

  const { query: importsQuery } = useTransactionImportsQuery(importParams)
  const importCache = useTransactionImportCache()

  const uploadMutation = useUploadTransactionMutation({
    onSuccess: () => {
      toast.success('Import submitted')
    },
    onError: (error) => {
      toast.error(readError(error))
    },
    onSettled: (variables) => {
      variables.input.value = ''
    },
  })

  const pages = computed(() => importsQuery.data.value?.pages ?? [])
  const imports = computed(() => pages.value.flatMap((page) => page.imports))
  const firstPage = computed(() => pages.value[0])
  const loading = computed(() => importsQuery.isPending.value)
  const loadingMore = computed(() => importsQuery.isFetchingNextPage.value)
  const loadMoreFailed = computed(() => importsQuery.isFetchNextPageError.value)
  const error = computed(() => readError(importsQuery.error.value))
  const uploadPending = computed(() => uploadMutation.isPending.value)
  const totalRows = computed(() => firstPage.value?.total ?? 0)
  const hasNextPage = computed(() => importsQuery.hasNextPage.value)
  const loadProgress = computed(() => `${imports.value.length} of ${totalRows.value}`)
  const tableRows = computed<DataTableRow[]>(() =>
    imports.value.map((item) => ({
      key: item.id,
      cells: [
        {
          component: DataTableTextCell,
          props: {
            class: 'font-medium text-stone-950',
            text: item.fileName,
          },
        },
        displayCategory(item.category),
        {
          component: ImportStatusCell,
          props: {
            status: item.status,
          },
        },
        {
          component: DataTableTextCell,
          props: {
            class: 'text-stone-700',
            text: String(item.transactionCount),
          },
        },
        displayDateTime(item.createdAt),
        displayDateTime(item.updatedAt),
      ],
    })),
  )

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
    () => importsQuery.error.value,
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
    loadMoreObserver?.disconnect()

    if (reconcileDebounce !== undefined) {
      window.clearTimeout(reconcileDebounce)
    }
  })

  function loadData() {
    void importsQuery.refetch()
  }

  function loadNextPage() {
    if (importsQuery.hasNextPage.value && !importsQuery.isFetchingNextPage.value) {
      void importsQuery.fetchNextPage()
    }
  }

  function setLoadMoreTarget(target: Element | ComponentPublicInstance | null) {
    loadMoreTarget.value = target instanceof HTMLElement ? target : null
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

  return {
    connectionState,
    error,
    filters,
    hasNextPage,
    loadData,
    loadMoreFailed,
    loadNextPage,
    loadProgress,
    loading,
    loadingMore,
    setLoadMoreTarget,
    tableHeaders,
    tableRows,
    uploadCategory,
    uploadFile,
    uploadPending,
  }
}

function displayCategory(category: FileCategory): string {
  return category === 'CreditCard' ? 'Credit card' : 'Extrato'
}

function readError(err: unknown): string {
  if (!err) {
    return ''
  }

  return err instanceof Error ? err.message : 'Something went wrong'
}
