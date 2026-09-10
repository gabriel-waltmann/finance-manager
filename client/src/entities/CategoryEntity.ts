import type { TimestampedEntity } from './Entity'

export interface CategoryEntity extends TimestampedEntity {
  title: string
  description: string | null
}

export interface ListCategoryResponse {
  categories: CategoryEntity[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface ListCategoryParams {
  search?: string
  order?: 'asc' | 'desc'
  page?: number
  limit?: number
  withDeleted?: boolean
}

export interface CategoryPayload {
  title: string
  description: string | null
}
