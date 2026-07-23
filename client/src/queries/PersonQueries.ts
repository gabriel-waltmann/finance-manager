import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { PersonController } from '../controllers/PersonController'
import type { PersonPayload } from '../entities/PersonEntity'
import { financeKeys } from './queryKeys'

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

export function usePeopleQuery() {
  return useQuery({
    queryKey: financeKeys.people(),
    queryFn: ({ signal }) => PersonController.list(signal),
  })
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
        queryClient.invalidateQueries({ queryKey: financeKeys.people() }),
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
        queryClient.invalidateQueries({ queryKey: financeKeys.people() }),
        queryClient.invalidateQueries({ queryKey: financeKeys.transactions() }),
        queryClient.invalidateQueries({ queryKey: financeKeys.dashboards() }),
      ])
    },
  })
}
