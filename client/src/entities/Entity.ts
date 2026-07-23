export type Id = string

export interface TimestampedEntity {
  id: Id
  created_at: string
  updated_at: string | null
  deleted_at: string | null
}
