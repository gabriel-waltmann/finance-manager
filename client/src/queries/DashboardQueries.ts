import { computed, type ComputedRef } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { TransactionController } from '../controllers/TransactionController'
import type { DashboardParams } from '../entities/Dashboard'
import { financeKeys } from './queryKeys'

export function useDashboardQuery(params: ComputedRef<DashboardParams>) {
  return useQuery({
    queryKey: computed(() => financeKeys.dashboard(params.value)),
    queryFn: ({ signal }) => TransactionController.getDashboard(params.value, signal),
  })
}
