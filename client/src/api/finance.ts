import { apiRequest } from './http'
import type {
  ListPersonResponse,
  ListTransactionResponse,
  Person,
  PersonPayload,
  Transaction,
  TransactionPayload,
  TransactionPerson,
  TransactionPersonPayload,
  TransactionWithPerson,
} from '../types'

export async function listTransactions(): Promise<TransactionWithPerson[]> {
  const response = await apiRequest<ListTransactionResponse>('/transactions')
  return response.transactions
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

export async function uploadTransactions(file: File): Promise<void> {
  const body = new FormData()
  body.append('File', file)

  await apiRequest<void>('/transaction/upload', {
    method: 'POST',
    body,
  })
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
