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
import type { PersonEntity, PersonPayload } from '../../entities/PersonEntity'
import {
  useDeletePersonMutation,
  usePersonsQuery,
  useSavePersonMutation,
  type PersonQueryParams,
} from '../../queries/PersonQueries'
import { useToast } from '../../stores/toast'
import PersonActionsCell from './components/PersonActionsCell.vue'

const tableHeaders: DataTableHeader[] = [
  { key: 'name', label: 'Name', class: 'min-w-56' },
  { key: 'email', label: 'Email', class: 'min-w-64' },
  { key: 'phone', label: 'Phone', class: 'min-w-44' },
  { key: 'actions', label: 'Actions', align: 'right', class: 'w-40' },
]

export function useController() {
  const toast = useToast()

  const formOpen = ref(false)
  const editing = ref<PersonEntity | null>(null)
  const deleteTarget = ref<PersonEntity | null>(null)
  const debouncedSearch = ref('')

  const filters = reactive({
    search: '',
    order: 'asc' as 'asc' | 'desc',
  })

  const form = reactive({
    name: '',
    email: '',
    phoneNumber: '',
  })

  let searchDebounce: ReturnType<typeof window.setTimeout> | undefined

  const personParams = computed<PersonQueryParams>(() => ({
    search: debouncedSearch.value || undefined,
    order: filters.order,
  }))

  const { query: personQuery } = usePersonsQuery(personParams)

  const pages = computed(() => personQuery.data.value?.pages ?? [])
  const persons = computed(() => pages.value.flatMap((page) => page.persons))
  const firstPage = computed(() => pages.value[0])
  const loading = computed(() => personQuery.isPending.value)
  const loadingMore = computed(() => personQuery.isFetchingNextPage.value)
  const loadMoreFailed = computed(() => personQuery.isFetchNextPageError.value)
  const error = computed(() => readError(personQuery.error.value))
  const totalRows = computed(() => firstPage.value?.total ?? 0)
  const hasNextPage = computed(() => personQuery.hasNextPage.value)
  const loadProgress = computed(() => `${persons.value.length} of ${totalRows.value}`)

  const savePersonMutation = useSavePersonMutation({
    onSuccess: (variables) => {
      toast.success(variables.id ? 'Person updated' : 'Person created')
      formOpen.value = false
    },
    onError: (mutationError) => {
      toast.error(readError(mutationError))
    },
  })

  const deletePersonMutation = useDeletePersonMutation({
    onSuccess: () => {
      toast.success('Person deleted')
      deleteTarget.value = null
    },
    onError: (mutationError) => {
      toast.error(readError(mutationError))
    },
  })

  const saving = computed(() => savePersonMutation.isPending.value)
  const deleting = computed(() => deletePersonMutation.isPending.value)
  const tableRows = computed<DataTableRow[]>(() =>
    persons.value.map((person) => ({
      key: person.id,
      cells: [
        {
          component: DataTableTextCell,
          props: {
            class: 'font-medium text-stone-950',
            text: person.name,
          },
        },
        person.email,
        person.phoneNumber,
        {
          component: PersonActionsCell,
          props: {
            onDeletePerson: () => confirmDelete(person),
            onEdit: () => openEditForm(person),
          },
        },
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
    () => personQuery.error.value,
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
    void personQuery.refetch()
  }

  function loadNextPage() {
    if (personQuery.hasNextPage.value && !personQuery.isFetchingNextPage.value) {
      void personQuery.fetchNextPage()
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
    form.name = ''
    form.email = ''
    form.phoneNumber = ''
    formOpen.value = true
  }

  function openEditForm(person: PersonEntity) {
    editing.value = person
    form.name = person.name
    form.email = person.email
    form.phoneNumber = person.phoneNumber
    formOpen.value = true
  }

  function closeForm() {
    if (!saving.value) {
      formOpen.value = false
    }
  }

  function submitForm() {
    const payload: PersonPayload = {
      name: form.name.trim(),
      email: form.email.trim(),
      phoneNumber: form.phoneNumber.trim(),
    }

    savePersonMutation.mutate({
      id: editing.value?.id,
      payload,
    })
  }

  function confirmDelete(person: PersonEntity) {
    deleteTarget.value = person
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

    deletePersonMutation.mutate(deleteTarget.value.id)
  }

  return {
    cancelDelete,
    closeForm,
    deleteTarget,
    deleting,
    editing,
    error,
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
    saving,
    setLoadMoreTarget,
    submitForm,
    tableHeaders,
    tableRows,
  }
}

function readError(err: unknown): string {
  if (!err) {
    return ''
  }

  return err instanceof Error ? err.message : 'Something went wrong'
}
