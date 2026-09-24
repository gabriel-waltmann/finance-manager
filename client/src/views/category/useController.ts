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
import type { CategoryEntity, CategoryPayload } from '@/entities/CategoryEntity'
import {
  useCategoriesQuery,
  useDeleteCategoryMutation,
  useSaveCategoryMutation,
  type CategoryQueryParams,
} from '@/queries/CategoryQueries'
import { useToast } from '@/stores/toast'
import CategoryActionsCell from './components/CategoryActionsCell.vue'

const tableHeaders: DataTableHeader[] = [
  { key: 'title', label: 'Title', class: 'min-w-56' },
  { key: 'description', label: 'Description', class: 'min-w-80' },
  { key: 'actions', label: 'Actions', align: 'right', class: 'w-40' },
]

export function useController() {
  const toast = useToast()
  const formOpen = ref(false)
  const editing = ref<CategoryEntity | null>(null)
  const deleteTarget = ref<CategoryEntity | null>(null)
  const debouncedSearch = ref('')
  const filters = reactive({ search: '', order: 'asc' as 'asc' | 'desc' })
  const form = reactive({ title: '', description: '' })
  let searchDebounce: ReturnType<typeof window.setTimeout> | undefined

  const categoryParams = computed<CategoryQueryParams>(() => ({
    search: debouncedSearch.value || undefined,
    order: filters.order,
  }))
  const { query: categoryQuery } = useCategoriesQuery(categoryParams)
  const pages = computed(() => categoryQuery.data.value?.pages ?? [])
  const categories = computed(() => pages.value.flatMap((page) => page.categories))
  const firstPage = computed(() => pages.value[0])
  const loading = computed(() => categoryQuery.isPending.value)
  const loadingMore = computed(() => categoryQuery.isFetchingNextPage.value)
  const loadMoreFailed = computed(() => categoryQuery.isFetchNextPageError.value)
  const error = computed(() => readError(categoryQuery.error.value))
  const totalRows = computed(() => firstPage.value?.total ?? 0)
  const hasNextPage = computed(() => categoryQuery.hasNextPage.value)
  const loadProgress = computed(() => `${categories.value.length} of ${totalRows.value}`)

  const saveCategoryMutation = useSaveCategoryMutation({
    onSuccess: (variables) => {
      toast.success(variables.id ? 'Category updated' : 'Category created')
      formOpen.value = false
    },
    onError: (mutationError) => toast.error(readError(mutationError)),
  })
  const deleteCategoryMutation = useDeleteCategoryMutation({
    onSuccess: () => {
      toast.success('Category deleted')
      deleteTarget.value = null
    },
    onError: (mutationError) => toast.error(readError(mutationError)),
  })
  const saving = computed(() => saveCategoryMutation.isPending.value)
  const deleting = computed(() => deleteCategoryMutation.isPending.value)
  const tableRows = computed<DataTableRow[]>(() => categories.value.map((category) => ({
    key: category.id,
    cells: [
      {
        component: DataTableTextCell,
        props: { class: 'font-medium text-stone-950', text: category.title },
      },
      {
        component: DataTableTextCell,
        props: {
          class: category.description ? 'whitespace-pre-wrap' : 'text-stone-400',
          text: category.description ?? '—',
        },
      },
      {
        component: CategoryActionsCell,
        props: {
          onDeleteCategory: () => confirmDelete(category),
          onEdit: () => openEditForm(category),
        },
      },
    ],
  })))

  const loadMoreTarget = ref<HTMLElement | null>(null)
  const loadMoreVisible = ref(false)
  let loadMoreObserver: IntersectionObserver | undefined

  function loadMoreIfNeeded() {
    const bounds = loadMoreTarget.value?.getBoundingClientRect()
    const nearViewport = bounds !== undefined &&
      bounds.top <= window.innerHeight + 200 && bounds.bottom >= -200

    if (loadMoreVisible.value && nearViewport && hasNextPage.value &&
      !loadingMore.value && !loadMoreFailed.value) {
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

  watch(() => filters.search, () => {
    clearSearchDebounce()
    searchDebounce = window.setTimeout(() => {
      debouncedSearch.value = filters.search.trim()
    }, 300)
  }, { flush: 'sync' })

  watch(() => categoryQuery.error.value, (queryError) => {
    if (queryError) toast.error(readError(queryError))
  })

  onMounted(() => {
    loadMoreObserver = new IntersectionObserver(([entry]) => {
      loadMoreVisible.value = entry?.isIntersecting ?? false
      loadMoreIfNeeded()
    }, { rootMargin: '200px 0px' })
    if (loadMoreTarget.value) loadMoreObserver.observe(loadMoreTarget.value)
  })

  onBeforeUnmount(() => {
    clearSearchDebounce()
    loadMoreObserver?.disconnect()
  })

  function loadData() { void categoryQuery.refetch() }
  function loadNextPage() {
    if (categoryQuery.hasNextPage.value && !categoryQuery.isFetchingNextPage.value) {
      void categoryQuery.fetchNextPage()
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
    form.title = ''
    form.description = ''
    formOpen.value = true
  }
  function openEditForm(category: CategoryEntity) {
    editing.value = category
    form.title = category.title
    form.description = category.description ?? ''
    formOpen.value = true
  }
  function closeForm() {
    if (!saving.value) formOpen.value = false
  }
  function submitForm() {
    const description = form.description.trim()
    const payload: CategoryPayload = {
      title: form.title.trim(),
      description: description || null,
    }
    saveCategoryMutation.mutate({ id: editing.value?.id, payload })
  }
  function confirmDelete(category: CategoryEntity) { deleteTarget.value = category }
  function cancelDelete() {
    if (!deleting.value) deleteTarget.value = null
  }
  function executeDelete() {
    if (deleteTarget.value) deleteCategoryMutation.mutate(deleteTarget.value.id)
  }

  return {
    cancelDelete, closeForm, deleteTarget, deleting, editing, error, executeDelete,
    filters, form, formOpen, hasNextPage, loadData, loadMoreFailed, loadNextPage,
    loadProgress, loading, loadingMore, openCreateForm, saving, setLoadMoreTarget,
    submitForm, tableHeaders, tableRows,
  }
}

function readError(err: unknown): string {
  if (!err) return ''
  return err instanceof Error ? err.message : 'Something went wrong'
}
