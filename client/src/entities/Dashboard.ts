import type { Id } from './Entity'

export interface DashboardTopItem {
  title: string
  totalAmount: number
  transactionCount: number
}

export interface DashboardFixedSpend {
  title: string
  monthCount: number
  lastMonth: string
  lastAmount: number
}

export interface GetDashboardResponse {
  topItems: DashboardTopItem[]
  fixedSpends: DashboardFixedSpend[]
  totalAmount: number
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface DashboardParams {
  startDate?: string
  endDate?: string
  personId?: Id
  page?: number
  limit?: number
  order?: 'asc' | 'desc'
}
