import { computed, type ComputedRef } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import {
  TransactionController,
  type TransactionImportEventHandlers,
} from '../controllers/TransactionController'
import type {
  FileCategory,
  ListTransactionImportParams,
  ListTransactionImportResponse,
  TransactionImportEntity,
} from '../entities/TransactionImportEntity'
import { financeKeys } from './queryKeys'

export interface UploadTransactionVariables {
  file: File
  category: FileCategory
  input: HTMLInputElement
}

interface UploadTransactionMutationOptions {
  onSuccess?: () => void
  onError?: (error: Error) => void
  onSettled?: (variables: UploadTransactionVariables) => void
}

export function useTransactionImportsQuery(params: ComputedRef<ListTransactionImportParams>) {
  return useQuery({
    queryKey: computed(() => financeKeys.importList(params.value)),
    queryFn: ({ signal }) => TransactionController.listImports(params.value, signal),
  })
}

export function useUploadTransactionMutation(options: UploadTransactionMutationOptions = {}) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ file, category }: UploadTransactionVariables) => (
      TransactionController.upload(file, category)
    ),
    onSuccess: async () => {
      options.onSuccess?.()
      await queryClient.invalidateQueries({ queryKey: financeKeys.imports() })
    },
    onError: (error) => {
      options.onError?.(error)
    },
    onSettled: (_result, _error, variables) => {
      options.onSettled?.(variables)
    },
  })
}

export function useTransactionImportCache() {
  const queryClient = useQueryClient()

  function invalidate() {
    return queryClient.invalidateQueries({ queryKey: financeKeys.imports() })
  }

  function update(transactionImport: TransactionImportEntity) {
    queryClient.setQueriesData<ListTransactionImportResponse>(
      { queryKey: financeKeys.imports() },
      (response) => {
        if (!response?.imports.some((item) => item.id === transactionImport.id)) {
          return response
        }

        return {
          ...response,
          imports: response.imports.map((item) =>
            item.id === transactionImport.id ? transactionImport : item,
          ),
        }
      },
    )
  }

  return { invalidate, update }
}

export function openTransactionImportEventStream(handlers: TransactionImportEventHandlers) {
  return TransactionController.openImportEventStream(handlers)
}
