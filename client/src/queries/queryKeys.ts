import type { DashboardParams } from '../entities/Dashboard'
import type { ListPersonParams } from '../entities/PersonEntity'
import type { ListTransactionParams } from '../entities/TransactionEntity'
import type { ListTransactionImportParams } from '../entities/TransactionImportEntity'

export const financeKeys = {
  all: ['finance'] as const,
  person: () => [...financeKeys.all, 'person'] as const,
  personOptions: () => [...financeKeys.person(), 'options'] as const,
  personList: (params: ListPersonParams) => [...financeKeys.person(), 'list', params] as const,
  dashboards: () => [...financeKeys.all, 'dashboard'] as const,
  dashboard: (params: DashboardParams) => [...financeKeys.dashboards(), params] as const,
  transactions: () => [...financeKeys.all, 'transactions'] as const,
  transactionList: (params: ListTransactionParams) => [...financeKeys.transactions(), params] as const,
  imports: () => [...financeKeys.all, 'transaction-imports'] as const,
  importList: (params: ListTransactionImportParams) => [...financeKeys.imports(), params] as const,
  assignmentMutations: () => [...financeKeys.all, 'assignment-mutation'] as const,
}
