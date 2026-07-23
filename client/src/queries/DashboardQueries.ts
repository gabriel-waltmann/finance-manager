import { computed, type ComputedRef } from 'vue'
import { useInfiniteQuery } from '@tanstack/vue-query'
import { TransactionController } from '../controllers/TransactionController'
import type { DashboardParams } from '../entities/Dashboard'
import { financeKeys } from './queryKeys'

export type DashboardQueryParams = Omit<DashboardParams, 'page'>

export function useDashboardQuery(params: ComputedRef<DashboardQueryParams>) {
  return useInfiniteQuery({
    queryKey: computed(() => financeKeys.dashboard(params.value)),
    queryFn: ({ pageParam, signal }) =>
      TransactionController.getDashboard({ ...params.value, page: pageParam }, signal),
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
  })
}
