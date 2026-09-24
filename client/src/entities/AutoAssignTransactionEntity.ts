import type { Id } from './Entity'

export type AutoAssignPersonAction = 'unchanged' | 'set' | 'clear'
export type AutoAssignCategoryAction = 'unchanged' | 'add' | 'replace' | 'clear'

export interface AutoAssignTransactionFilter {
  title?: string
  startDate?: string
  endDate?: string
  personId?: Id
  unassigned?: boolean
  categoryId?: Id
  uncategorized?: boolean
}

export interface AutoAssignTransactionsPayload {
  filter: AutoAssignTransactionFilter
  personAction: AutoAssignPersonAction
  targetPersonId?: Id
  categoryAction: AutoAssignCategoryAction
  targetCategoryIds: Id[]
}

export interface AutoAssignTransactionsResponse {
  matchedCount: number
  personChangedCount: number
  categoryChangedCount: number
}
