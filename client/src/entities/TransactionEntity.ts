import type { Id, TimestampedEntity } from './Entity'
import type { PersonEntity } from './PersonEntity'
import type { CategoryEntity } from './CategoryEntity'
import type { TransactionCategoryEntity } from './TransactionCategoryEntity'
import type { TransactionPersonEntity } from './TransactionPersonEntity'

export interface TransactionEntity extends TimestampedEntity {
  date: string
  title: string
  amount: number
}

export interface TransactionWithAssignments {
  transaction: TransactionEntity
  transactionPerson: TransactionPersonEntity | null
  person: PersonEntity | null
  transactionCategories: TransactionCategoryEntity[]
  categories: CategoryEntity[]
}

export interface ListTransactionResponse {
  transactions: TransactionWithAssignments[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface ListTransactionParams {
  search?: string
  title?: string
  startDate?: string
  endDate?: string
  personId?: Id
  unassigned?: boolean
  categoryId?: Id
  uncategorized?: boolean
  page?: number
  limit?: number
  order?: 'asc' | 'desc'
  withDeleted?: boolean
}

export interface TransactionPayload {
  date: string
  title: string
  amount: number
}
