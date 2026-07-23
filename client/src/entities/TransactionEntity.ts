import type { Id, TimestampedEntity } from './Entity'
import type { PersonEntity } from './PersonEntity'
import type { TransactionPersonEntity } from './TransactionPersonEntity'

export interface TransactionEntity extends TimestampedEntity {
  date: string
  title: string
  amount: number
}

export interface TransactionWithPerson {
  transaction: TransactionEntity
  transactionPerson: TransactionPersonEntity | null
  person: PersonEntity | null
}

export interface ListTransactionResponse {
  transactions: TransactionWithPerson[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface ListTransactionParams {
  search?: string
  startDate?: string
  endDate?: string
  personId?: Id
  unassigned?: boolean
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
