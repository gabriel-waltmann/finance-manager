import type { TimestampedEntity } from './Entity'

export interface PersonEntity extends TimestampedEntity {
  name: string
  email: string
  phoneNumber: string
}

export interface ListPersonResponse {
  persons: PersonEntity[]
}

export interface PersonPayload {
  name: string
  email: string
  phoneNumber: string
}
