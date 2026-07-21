import { apiRequest } from './http'
import type {
  DashboardParams,
  FileCategory,
  GetDashboardResponse,
  ListPersonResponse,
  ListTransactionImportParams,
  ListTransactionImportResponse,
  ListTransactionParams,
  ListTransactionResponse,
  Person,
  PersonPayload,
  Transaction,
  TransactionImport,
  TransactionPayload,
  TransactionPerson,
  TransactionPersonPayload,
} from '../types'

export async function getDashboard(params: DashboardParams = {}): Promise<GetDashboardResponse> {
  const queryString = buildDashboardQuery(params)
  return apiRequest<GetDashboardResponse>(queryString ? `/dashboard?${queryString}` : '/dashboard')
}

function buildDashboardQuery(params: DashboardParams): string {
  const query = new URLSearchParams()

  if (params.startDate) {
    query.set('startDate', params.startDate)
  }

  if (params.endDate) {
    query.set('endDate', params.endDate)
  }

  if (params.personId) {
    query.set('personId', params.personId)
  }

  if (params.page !== undefined) {
    query.set('page', String(params.page))
  }

  if (params.limit !== undefined) {
    query.set('limit', String(params.limit))
  }

  if (params.order) {
    query.set('order', params.order)
  }

  return query.toString()
}

export async function listTransactions(params: ListTransactionParams = {}): Promise<ListTransactionResponse> {
  const queryString = buildListTransactionsQuery(params)
  return apiRequest<ListTransactionResponse>(queryString ? `/transactions?${queryString}` : '/transactions')
}

function buildListTransactionsQuery(params: ListTransactionParams): string {
  const query = new URLSearchParams()

  if (params.search) {
    query.set('search', params.search)
  }

  if (params.startDate) {
    query.set('startDate', params.startDate)
  }

  if (params.endDate) {
    query.set('endDate', params.endDate)
  }

  if (params.personId) {
    query.set('personId', params.personId)
  }

  if (params.unassigned !== undefined) {
    query.set('unassigned', String(params.unassigned))
  }

  if (params.page !== undefined) {
    query.set('page', String(params.page))
  }

  if (params.limit !== undefined) {
    query.set('limit', String(params.limit))
  }

  if (params.order) {
    query.set('order', params.order)
  }

  if (params.withDeleted !== undefined) {
    query.set('withDeleted', String(params.withDeleted))
  }

  return query.toString()
}

export async function createTransaction(payload: TransactionPayload): Promise<Transaction> {
  return apiRequest<Transaction>('/transaction', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function updateTransaction(id: string, payload: TransactionPayload): Promise<void> {
  await apiRequest<void>(`/transaction/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
}

export async function deleteTransaction(id: string): Promise<void> {
  await apiRequest<void>(`/transaction/${id}`, {
    method: 'DELETE',
  })
}

export async function uploadTransactions(file: File, category: FileCategory): Promise<TransactionImport> {
  const body = new FormData()
  body.append('File', file)
  body.append('Category', category)

  return apiRequest<TransactionImport>('/transaction/upload', {
    method: 'POST',
    body,
  })
}

export async function listTransactionImports(
  params: ListTransactionImportParams = {},
): Promise<ListTransactionImportResponse> {
  const query = new URLSearchParams()

  if (params.search) {
    query.set('search', params.search)
  }

  if (params.status) {
    query.set('status', params.status)
  }

  if (params.page !== undefined) {
    query.set('page', String(params.page))
  }

  if (params.limit !== undefined) {
    query.set('limit', String(params.limit))
  }

  if (params.order) {
    query.set('order', params.order)
  }

  const queryString = query.toString()
  return apiRequest<ListTransactionImportResponse>(
    queryString ? `/transaction-imports?${queryString}` : '/transaction-imports',
  )
}

export interface TransactionImportEventHandlers {
  onOpen: () => void
  onError: () => void
  onStatus: (transactionImport: TransactionImport) => void
}

export function openTransactionImportEventStream(
  handlers: TransactionImportEventHandlers,
): EventSource {
  const source = new EventSource('/api/transaction-imports/events')

  source.onopen = handlers.onOpen
  source.onerror = handlers.onError
  source.addEventListener('transaction-import-status', (event) => {
    const message = event as MessageEvent<string>

    try {
      handlers.onStatus(JSON.parse(message.data) as TransactionImport)
    } catch {
      // A reconciliation fetch on connect remains the source of truth.
    }
  })

  return source
}

export async function listPeople(): Promise<Person[]> {
  const response = await apiRequest<ListPersonResponse>('/persons')
  return response.persons
}

export async function createPerson(payload: PersonPayload): Promise<Person> {
  return apiRequest<Person>('/person', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function updatePerson(id: string, payload: PersonPayload): Promise<void> {
  await apiRequest<void>(`/person/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
}

export async function deletePerson(id: string): Promise<void> {
  await apiRequest<void>(`/person/${id}`, {
    method: 'DELETE',
  })
}

export async function createAssignment(payload: TransactionPersonPayload): Promise<TransactionPerson> {
  return apiRequest<TransactionPerson>('/transaction-person', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export async function updateAssignment(id: string, payload: TransactionPersonPayload): Promise<void> {
  await apiRequest<void>(`/transaction-person/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
}

export async function deleteAssignment(id: string): Promise<void> {
  await apiRequest<void>(`/transaction-person/${id}`, {
    method: 'DELETE',
  })
}
