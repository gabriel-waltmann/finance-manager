import { computed, type ComputedRef } from 'vue'
import {
  type InfiniteData,
  useInfiniteQuery,
  useMutation,
  useMutationState,
  useQueryClient,
} from '@tanstack/vue-query'
import { TransactionController } from '../controllers/TransactionController'
import { TransactionPersonController } from '../controllers/TransactionPersonController'
import { TransactionCategoryController } from '../controllers/TransactionCategoryController'
import type { PersonEntity } from '../entities/PersonEntity'
import type {
  AutoAssignTransactionsPayload,
  AutoAssignTransactionsResponse,
} from '../entities/AutoAssignTransactionEntity'
import type {
  ListTransactionParams,
  ListTransactionResponse,
  TransactionPayload,
  TransactionWithAssignments,
} from '../entities/TransactionEntity'
import type { TransactionPersonEntity } from '../entities/TransactionPersonEntity'
import type { SetTransactionCategoriesResponse } from '../entities/TransactionCategoryEntity'
import { financeKeys } from './queryKeys'

type TransactionQueryKey = ReturnType<typeof financeKeys.transactionList>

export type TransactionQueryParams = Omit<ListTransactionParams, 'limit' | 'page'>

export const TRANSACTION_PAGE_SIZE = 20

export interface SaveTransactionVariables {
  editing: TransactionWithAssignments | null
  payload: TransactionPayload
  personId: string
  categoryIds: string[]
}

export interface AssignmentVariables {
  item: TransactionWithAssignments
  nextPersonId: string
  previousPersonId: string
  select: HTMLSelectElement
}

export interface CategoryAssignmentVariables {
  item: TransactionWithAssignments
  nextCategoryIds: string[]
}

interface AutoAssignTransactionsMutationOptions {
  onSuccess?: (response: AutoAssignTransactionsResponse) => void
  onError?: (error: Error) => void
}

interface SaveTransactionMutationOptions {
  onSuccess?: (variables: SaveTransactionVariables) => void
  onError?: (error: Error) => void
}

interface AssignmentMutationOptions {
  onSuccess?: (variables: AssignmentVariables) => void
  onError?: (error: Error, variables: AssignmentVariables) => void
}

interface CategoryAssignmentMutationOptions {
  onSuccess?: (variables: CategoryAssignmentVariables) => void
  onError?: (error: Error, variables: CategoryAssignmentVariables) => void
}

interface DeleteTransactionMutationOptions {
  onSuccess?: () => void
  onError?: (error: Error) => void
}

export function useTransactionsQuery(
  params: ComputedRef<TransactionQueryParams>,
  enabled?: ComputedRef<boolean>,
) {
  const queryKey = computed(() => financeKeys.transactionList({
    ...params.value,
    limit: TRANSACTION_PAGE_SIZE,
  }))
  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam, signal }) => TransactionController.list({
      ...params.value,
      page: pageParam,
      limit: TRANSACTION_PAGE_SIZE,
    }, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined
    ),
    enabled,
  })

  return { query, queryKey }
}

export function useAutoAssignTransactionsMutation(
  options: AutoAssignTransactionsMutationOptions = {},
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AutoAssignTransactionsPayload) => (
      TransactionController.autoAssign(payload)
    ),
    onSuccess: async (response) => {
      await invalidateTransactionData(queryClient)
      options.onSuccess?.(response)
    },
    onError: (error) => {
      options.onError?.(error)
    },
  })
}

export function useSaveTransactionMutation(options: SaveTransactionMutationOptions = {}) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (variables: SaveTransactionVariables) => {
      if (variables.editing) {
        await TransactionController.update(variables.editing.transaction.id, variables.payload)
        await persistAssignment(variables.editing, variables.personId)
        await TransactionCategoryController.set(variables.editing.transaction.id, {
          categoryIds: variables.categoryIds,
        })
        return
      }

      const transaction = await TransactionController.create(variables.payload)

      if (variables.personId) {
        await TransactionPersonController.create({
          personId: variables.personId,
          transactionId: transaction.id,
        })
      }

      if (variables.categoryIds.length > 0) {
        await TransactionCategoryController.set(transaction.id, {
          categoryIds: variables.categoryIds,
        })
      }
    },
    onSuccess: (_result, variables) => {
      options.onSuccess?.(variables)
    },
    onError: (error) => {
      options.onError?.(error)
    },
    onSettled: async () => {
      await invalidateTransactionData(queryClient)
    },
  })
}

export function useAssignmentMutation(
  persons: ComputedRef<PersonEntity[]>,
  transactionQueryKey: ComputedRef<TransactionQueryKey>,
  options: AssignmentMutationOptions = {},
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationKey: financeKeys.assignmentMutations(),
    mutationFn: ({ item, nextPersonId }: AssignmentVariables) => (
      persistAssignment(item, nextPersonId)
    ),
    onSuccess: (transactionPerson, variables) => {
      updateTransactionAssignment(
        queryClient,
        transactionQueryKey.value,
        persons.value,
        variables.item.transaction.id,
        transactionPerson,
      )
      options.onSuccess?.(variables)
    },
    onError: (error, variables) => {
      options.onError?.(error, variables)
    },
    onSettled: async () => {
      await invalidateTransactionData(queryClient)
    },
  })
}

export function usePendingAssignmentIds() {
  return useMutationState<string>({
    filters: {
      mutationKey: financeKeys.assignmentMutations(),
      status: 'pending',
    },
    select: (mutation) => (
      mutation.state.variables as AssignmentVariables
    ).item.transaction.id,
  })
}

export function useCategoryAssignmentMutation(
  transactionQueryKey: ComputedRef<TransactionQueryKey>,
  options: CategoryAssignmentMutationOptions = {},
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationKey: financeKeys.categoryAssignmentMutations(),
    mutationFn: ({ item, nextCategoryIds }: CategoryAssignmentVariables) => (
      TransactionCategoryController.set(item.transaction.id, { categoryIds: nextCategoryIds })
    ),
    onSuccess: (response, variables) => {
      updateTransactionCategories(
        queryClient,
        transactionQueryKey.value,
        variables.item.transaction.id,
        response,
      )
      options.onSuccess?.(variables)
    },
    onError: (error, variables) => options.onError?.(error, variables),
    onSettled: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
        queryClient.invalidateQueries({ queryKey: financeKeys.dashboards() }),
      ])
    },
  })
}

export function usePendingCategoryAssignmentIds() {
  return useMutationState<string>({
    filters: {
      mutationKey: financeKeys.categoryAssignmentMutations(),
      status: 'pending',
    },
    select: (mutation) => (
      mutation.state.variables as CategoryAssignmentVariables
    ).item.transaction.id,
  })
}

export function useDeleteTransactionMutation(
  transactionQueryKey: ComputedRef<TransactionQueryKey>,
  options: DeleteTransactionMutationOptions = {},
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (transactionId: string) => TransactionController.delete(transactionId),
    onSuccess: (_result, transactionId) => {
      removeTransactionFromCache(queryClient, transactionQueryKey.value, transactionId)
      options.onSuccess?.()
    },
    onError: (error) => {
      options.onError?.(error)
    },
    onSettled: async () => {
      await invalidateTransactionData(queryClient)
    },
  })
}

async function persistAssignment(
  item: TransactionWithAssignments,
  nextPersonId: string,
): Promise<TransactionPersonEntity | null> {
  const transactionId = item.transaction.id
  const currentAssignment = item.transactionPerson
  const currentPersonId = item.person?.id ?? ''

  if (currentPersonId === nextPersonId) {
    return currentAssignment
  }

  if (!nextPersonId && currentAssignment) {
    await TransactionPersonController.delete(currentAssignment.id)
    return null
  }

  if (nextPersonId && currentAssignment) {
    await TransactionPersonController.update(currentAssignment.id, {
      personId: nextPersonId,
      transactionId,
    })

    return {
      ...currentAssignment,
      personId: nextPersonId,
      transactionId,
      updated_at: new Date().toISOString(),
    }
  }

  if (nextPersonId) {
    return TransactionPersonController.create({
      personId: nextPersonId,
      transactionId,
    })
  }

  return null
}

function updateTransactionCategories(
  queryClient: ReturnType<typeof useQueryClient>,
  queryKey: TransactionQueryKey,
  transactionId: string,
  response: SetTransactionCategoriesResponse,
) {
  queryClient.setQueryData<InfiniteData<ListTransactionResponse>>(queryKey, (data) => (
    data
      ? {
          ...data,
          pages: data.pages.map((page) => ({
            ...page,
            transactions: page.transactions.map((item) =>
              item.transaction.id === transactionId
                ? {
                    ...item,
                    transactionCategories: response.transactionCategories,
                    categories: response.categories,
                  }
                : item,
            ),
          })),
        }
      : data
  ))
}

function updateTransactionAssignment(
  queryClient: ReturnType<typeof useQueryClient>,
  queryKey: TransactionQueryKey,
  persons: PersonEntity[],
  transactionId: string,
  transactionPerson: TransactionPersonEntity | null,
) {
  const person = transactionPerson
    ? persons.find((item) => item.id === transactionPerson.personId) ?? null
    : null

  queryClient.setQueryData<InfiniteData<ListTransactionResponse>>(queryKey, (response) => (
    response
      ? {
          ...response,
          pages: response.pages.map((page) => ({
            ...page,
            transactions: page.transactions.map((item) =>
              item.transaction.id === transactionId
                ? { ...item, transactionPerson, person }
                : item,
            ),
          })),
        }
      : response
  ))
}

function removeTransactionFromCache(
  queryClient: ReturnType<typeof useQueryClient>,
  queryKey: TransactionQueryKey,
  transactionId: string,
) {
  queryClient.setQueryData<InfiniteData<ListTransactionResponse>>(queryKey, (response) => {
    if (!response) {
      return response
    }

    const containsTransaction = response.pages.some((page) =>
      page.transactions.some((item) => item.transaction.id === transactionId),
    )

    if (!containsTransaction) {
      return response
    }

    return {
      ...response,
      pages: response.pages.map((page) => {
        const nextTotal = Math.max(page.total - 1, 0)

        return {
          ...page,
          transactions: page.transactions.filter(
            (item) => item.transaction.id !== transactionId,
          ),
          total: nextTotal,
          totalPages: nextTotal === 0 ? 0 : Math.ceil(nextTotal / page.limit),
        }
      }),
    }
  })
}

async function invalidateTransactionData(queryClient: ReturnType<typeof useQueryClient>) {
  await Promise.all([
    queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
    queryClient.invalidateQueries({ queryKey: financeKeys.dashboards() }),
  ])
}
