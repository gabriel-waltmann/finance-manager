import type {
  DashboardParams,
  GetDashboardResponse,
} from '../entities/Dashboard'
import type {
  ListTransactionParams,
  ListTransactionResponse,
  TransactionEntity,
  TransactionPayload,
} from '../entities/TransactionEntity'
import type {
  FileCategory,
  ListTransactionImportParams,
  ListTransactionImportResponse,
  TransactionImportEntity,
} from '../entities/TransactionImportEntity'
import { apiRequest } from '../api/http'

export interface TransactionImportEventHandlers {
  onOpen: () => void
  onError: () => void
  onStatus: (transactionImport: TransactionImportEntity) => void
}

export class TransactionController {
  static getDashboard(
    params: DashboardParams = {},
    signal?: AbortSignal,
  ): Promise<GetDashboardResponse> {
    return apiRequest<GetDashboardResponse>('/dashboard', { params, signal })
  }

  static list(
    params: ListTransactionParams = {},
    signal?: AbortSignal,
  ): Promise<ListTransactionResponse> {
    return apiRequest<ListTransactionResponse>('/transactions', { params, signal })
  }

  static create(payload: TransactionPayload): Promise<TransactionEntity> {
    return apiRequest<TransactionEntity>('/transaction', {
      method: 'POST',
      data: payload,
    })
  }

  static async update(id: string, payload: TransactionPayload): Promise<void> {
    await apiRequest<void>(`/transaction/${id}`, {
      method: 'PUT',
      data: payload,
    })
  }

  static async delete(id: string): Promise<void> {
    await apiRequest<void>(`/transaction/${id}`, { method: 'DELETE' })
  }

  static upload(file: File, category: FileCategory): Promise<TransactionImportEntity> {
    const data = new FormData()
    data.append('File', file)
    data.append('Category', category)

    return apiRequest<TransactionImportEntity>('/transaction/upload', {
      method: 'POST',
      data,
    })
  }

  static listImports(
    params: ListTransactionImportParams = {},
    signal?: AbortSignal,
  ): Promise<ListTransactionImportResponse> {
    return apiRequest<ListTransactionImportResponse>('/transaction-imports', { params, signal })
  }

  static openImportEventStream(handlers: TransactionImportEventHandlers): EventSource {
    const source = new EventSource('/api/transaction-imports/events')

    source.onopen = handlers.onOpen
    source.onerror = handlers.onError
    source.addEventListener('transaction-import-status', (event) => {
      const message = event as MessageEvent<string>

      try {
        handlers.onStatus(JSON.parse(message.data) as TransactionImportEntity)
      } catch {
        // A reconciliation fetch on connect remains the source of truth.
      }
    })

    return source
  }
}
