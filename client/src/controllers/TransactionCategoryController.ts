import { apiRequest } from '../api/http'
import type {
  SetTransactionCategoriesPayload,
  SetTransactionCategoriesResponse,
} from '../entities/TransactionCategoryEntity'

export class TransactionCategoryController {
  static set(
    transactionId: string,
    payload: SetTransactionCategoriesPayload,
  ): Promise<SetTransactionCategoriesResponse> {
    return apiRequest<SetTransactionCategoriesResponse>(`/transaction/${transactionId}/categories`, {
      method: 'PUT',
      data: payload,
    })
  }
}
