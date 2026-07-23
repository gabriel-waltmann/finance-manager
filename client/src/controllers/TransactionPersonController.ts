import type {
  TransactionPersonEntity,
  TransactionPersonPayload,
} from '../entities/TransactionPersonEntity'
import { apiRequest } from '../api/http'

export class TransactionPersonController {
  static create(payload: TransactionPersonPayload): Promise<TransactionPersonEntity> {
    return apiRequest<TransactionPersonEntity>('/transaction-person', {
      method: 'POST',
      data: payload,
    })
  }

  static async update(id: string, payload: TransactionPersonPayload): Promise<void> {
    await apiRequest<void>(`/transaction-person/${id}`, {
      method: 'PUT',
      data: payload,
    })
  }

  static async delete(id: string): Promise<void> {
    await apiRequest<void>(`/transaction-person/${id}`, { method: 'DELETE' })
  }
}
