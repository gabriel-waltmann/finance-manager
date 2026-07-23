import type { Id, TimestampedEntity } from './Entity'

export interface TransactionPersonEntity extends TimestampedEntity {
  personId: Id
  transactionId: Id
}

export interface TransactionPersonPayload {
  personId: Id
  transactionId: Id
}
