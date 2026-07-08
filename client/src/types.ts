export type Id = string

export interface TimestampedEntity {
  id: Id
  created_at: string
  updated_at: string | null
  deleted_at: string | null
}

export interface Transaction extends TimestampedEntity {
  date: string
  title: string
  amount: number
}

export interface Person extends TimestampedEntity {
  name: string
  email: string
  phoneNumber: string
}

export interface TransactionPerson extends TimestampedEntity {
  personId: Id
  transactionId: Id
}

export interface TransactionWithPerson {
  transaction: Transaction
  transactionPerson: TransactionPerson | null
  person: Person | null
}

export interface ListTransactionResponse {
  transactions: TransactionWithPerson[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface ListTransactionParams {
  startDate?: string
  endDate?: string
  page?: number
  limit?: number
  order?: 'asc' | 'desc'
  withDeleted?: boolean
}

export interface DashboardTopItem {
  title: string
  totalAmount: number
  transactionCount: number
}

export interface GetDashboardResponse {
  topItems: DashboardTopItem[]
  totalAmount: number
}

export interface DashboardParams {
  startDate?: string
  endDate?: string
  personId?: Id
  order?: 'asc' | 'desc'
}

export type FileCategory = 'CreditCard' | 'Extrato'

export interface ListPersonResponse {
  persons: Person[]
}

export interface TransactionPayload {
  date: string
  title: string
  amount: number
}

export interface PersonPayload {
  name: string
  email: string
  phoneNumber: string
}

export interface TransactionPersonPayload {
  personId: Id
  transactionId: Id
}
