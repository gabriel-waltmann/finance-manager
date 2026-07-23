import type { Id } from './Entity'

export type FileCategory = 'CreditCard' | 'Extrato'

export type FileProcessingStatus = 'Submitted' | 'Processing' | 'Finished' | 'Failed'

export interface TransactionImportEntity {
  id: Id
  fileName: string
  category: FileCategory
  status: FileProcessingStatus
  transactionCount: number
  createdAt: string
  updatedAt: string | null
}

export interface ListTransactionImportResponse {
  imports: TransactionImportEntity[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface ListTransactionImportParams {
  search?: string
  status?: FileProcessingStatus
  page?: number
  limit?: number
  order?: 'asc' | 'desc'
}
