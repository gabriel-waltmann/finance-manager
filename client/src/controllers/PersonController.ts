import type {
  ListPersonResponse,
  ListPersonParams,
  PersonEntity,
  PersonPayload,
} from '../entities/PersonEntity'
import { apiRequest } from '../api/http'

export class PersonController {
  static list(
    params: ListPersonParams = {},
    signal?: AbortSignal,
  ): Promise<ListPersonResponse> {
    return apiRequest<ListPersonResponse>('/persons', { params, signal })
  }

  static async listOptions(signal?: AbortSignal): Promise<PersonEntity[]> {
    const response = await PersonController.list({ order: 'asc' }, signal)
    return response.persons
  }

  static create(payload: PersonPayload): Promise<PersonEntity> {
    return apiRequest<PersonEntity>('/person', {
      method: 'POST',
      data: payload,
    })
  }

  static async update(id: string, payload: PersonPayload): Promise<void> {
    await apiRequest<void>(`/person/${id}`, {
      method: 'PUT',
      data: payload,
    })
  }

  static async delete(id: string): Promise<void> {
    await apiRequest<void>(`/person/${id}`, { method: 'DELETE' })
  }
}
