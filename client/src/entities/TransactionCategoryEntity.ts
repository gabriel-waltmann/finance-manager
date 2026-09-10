import type { CategoryEntity } from './CategoryEntity'
import type { Id, TimestampedEntity } from './Entity'

export interface TransactionCategoryEntity extends TimestampedEntity {
  transactionId: Id
  categoryId: Id
}

export interface SetTransactionCategoriesPayload {
  categoryIds: Id[]
}

export interface SetTransactionCategoriesResponse {
  transactionCategories: TransactionCategoryEntity[]
  categories: CategoryEntity[]
}
