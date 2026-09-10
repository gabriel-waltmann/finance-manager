import { computed, type ComputedRef } from 'vue'
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { CategoryController } from '../controllers/CategoryController'
import type { CategoryPayload, ListCategoryParams } from '../entities/CategoryEntity'
import { financeKeys } from './queryKeys'

export type CategoryQueryParams = Omit<ListCategoryParams, 'limit' | 'page'>

export const CATEGORY_PAGE_SIZE = 20

export interface SaveCategoryVariables {
  id?: string
  payload: CategoryPayload
}

interface SaveCategoryMutationOptions {
  onSuccess?: (variables: SaveCategoryVariables) => void
  onError?: (error: Error) => void
}

interface DeleteCategoryMutationOptions {
  onSuccess?: () => void
  onError?: (error: Error) => void
}

export function useCategoryOptionsQuery() {
  return useQuery({
    queryKey: financeKeys.categoryOptions(),
    queryFn: ({ signal }) => CategoryController.listOptions(signal),
  })
}

export function useCategoriesQuery(params: ComputedRef<CategoryQueryParams>) {
  const queryKey = computed(() => financeKeys.categoryList({
    ...params.value,
    limit: CATEGORY_PAGE_SIZE,
  }))
  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam, signal }) => CategoryController.list({
      ...params.value,
      page: pageParam,
      limit: CATEGORY_PAGE_SIZE,
    }, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined
    ),
  })

  return { query, queryKey }
}

export function useSaveCategoryMutation(options: SaveCategoryMutationOptions = {}) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, payload }: SaveCategoryVariables) => {
      if (id) {
        await CategoryController.update(id, payload)
      } else {
        await CategoryController.create(payload)
      }
    },
    onSuccess: (_result, variables) => {
      options.onSuccess?.(variables)
    },
    onError: (error) => {
      options.onError?.(error)
    },
    onSettled: async (_result, _error, variables) => {
      const invalidations = [
        queryClient.invalidateQueries({ queryKey: financeKeys.category() }),
      ]

      if (variables.id) {
        invalidations.push(
          queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
        )
      }

      await Promise.all(invalidations)
    },
  })
}

export function useDeleteCategoryMutation(options: DeleteCategoryMutationOptions = {}) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => CategoryController.delete(id),
    onSuccess: () => {
      options.onSuccess?.()
    },
    onError: (error) => {
      options.onError?.(error)
    },
    onSettled: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: financeKeys.category() }),
        queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
      ])
    },
  })
}
