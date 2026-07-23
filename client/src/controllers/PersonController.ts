import type {
  ListPersonResponse,
  PersonEntity,
  PersonPayload,
} from '../entities/PersonEntity'
import { apiRequest } from '../api/http'

export class PersonController {
  static async list(signal?: AbortSignal): Promise<PersonEntity[]> {
    const response = await apiRequest<ListPersonResponse>('/persons', { signal })
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
