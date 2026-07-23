import { computed, type ComputedRef } from 'vue'
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { PersonController } from '../controllers/PersonController'
import type { ListPersonParams, PersonPayload } from '../entities/PersonEntity'
import { financeKeys } from './queryKeys'

export type PersonQueryParams = Omit<ListPersonParams, 'limit' | 'page'>

export const PERSON_PAGE_SIZE = 20

export interface SavePersonVariables {
  id?: string
  payload: PersonPayload
}

interface SavePersonMutationOptions {
  onSuccess?: (variables: SavePersonVariables) => void
  onError?: (error: Error) => void
}

interface DeletePersonMutationOptions {
  onSuccess?: () => void
  onError?: (error: Error) => void
}

export function usePersonOptionsQuery() {
  return useQuery({
    queryKey: financeKeys.personOptions(),
    queryFn: ({ signal }) => PersonController.listOptions(signal),
  })
}

export function usePersonsQuery(params: ComputedRef<PersonQueryParams>) {
  const queryKey = computed(() => financeKeys.personList({
    ...params.value,
    limit: PERSON_PAGE_SIZE,
  }))
  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam, signal }) => PersonController.list({
      ...params.value,
      page: pageParam,
      limit: PERSON_PAGE_SIZE,
    }, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined
    ),
  })

  return { query, queryKey }
}

export function useSavePersonMutation(options: SavePersonMutationOptions = {}) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, payload }: SavePersonVariables) => {
      if (id) {
        await PersonController.update(id, payload)
      } else {
        await PersonController.create(payload)
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
        queryClient.invalidateQueries({ queryKey: financeKeys.person() }),
      ]

      if (variables.id) {
        invalidations.push(
          queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
          queryClient.invalidateQueries({ queryKey: financeKeys.dashboards() }),
        )
      }

      await Promise.all(invalidations)
    },
  })
}

export function useDeletePersonMutation(options: DeletePersonMutationOptions = {}) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => PersonController.delete(id),
    onSuccess: () => {
      options.onSuccess?.()
    },
    onError: (error) => {
      options.onError?.(error)
    },
    onSettled: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: financeKeys.person() }),
        queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
        queryClient.invalidateQueries({ queryKey: financeKeys.dashboards() }),
      ])
    },
  })
}
