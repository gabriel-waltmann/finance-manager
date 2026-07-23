import type { TimestampedEntity } from './Entity'

export interface PersonEntity extends TimestampedEntity {
  name: string
  email: string
  phoneNumber: string
}

export interface ListPersonResponse {
  persons: PersonEntity[]
  page: number
  limit: number
  total: number
  totalPages: number
}

export interface ListPersonParams {
  search?: string
  order?: 'asc' | 'desc'
  page?: number
  limit?: number
  withDeleted?: boolean
}

export interface PersonPayload {
  name: string
  email: string
  phoneNumber: string
}
