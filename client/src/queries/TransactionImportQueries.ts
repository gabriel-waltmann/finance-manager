import { computed, type ComputedRef } from 'vue'
import {
  type InfiniteData,
  useInfiniteQuery,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
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

export type TransactionImportQueryParams = Omit<ListTransactionImportParams, 'limit' | 'page'>

export const TRANSACTION_IMPORT_PAGE_SIZE = 20

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

export function useTransactionImportsQuery(params: ComputedRef<TransactionImportQueryParams>) {
  const queryKey = computed(() => financeKeys.importList({
    ...params.value,
    limit: TRANSACTION_IMPORT_PAGE_SIZE,
  }))
  const query = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam, signal }) => TransactionController.listImports({
      ...params.value,
      page: pageParam,
      limit: TRANSACTION_IMPORT_PAGE_SIZE,
    }, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined
    ),
  })

  return { query, queryKey }
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
    queryClient.setQueriesData<InfiniteData<ListTransactionImportResponse>>(
      { queryKey: financeKeys.imports() },
      (response) => {
        if (!response?.pages.some((page) =>
          page.imports.some((item) => item.id === transactionImport.id),
        )) {
          return response
        }

        return {
          ...response,
          pages: response.pages.map((page) => ({
            ...page,
            imports: page.imports.map((item) =>
              item.id === transactionImport.id ? transactionImport : item,
            ),
          })),
        }
      },
    )
  }

  return { invalidate, update }
}

export function openTransactionImportEventStream(handlers: TransactionImportEventHandlers) {
  return TransactionController.openImportEventStream(handlers)
}
