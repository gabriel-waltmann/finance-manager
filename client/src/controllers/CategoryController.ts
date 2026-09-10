import { apiRequest } from '../api/http'
import type {
  CategoryEntity,
  CategoryPayload,
  ListCategoryParams,
  ListCategoryResponse,
} from '../entities/CategoryEntity'

export class CategoryController {
  static list(
    params: ListCategoryParams = {},
    signal?: AbortSignal,
  ): Promise<ListCategoryResponse> {
    return apiRequest<ListCategoryResponse>('/categories', { params, signal })
  }

  static async listOptions(signal?: AbortSignal): Promise<CategoryEntity[]> {
    const response = await CategoryController.list({ order: 'asc' }, signal)
    return response.categories
  }

  static create(payload: CategoryPayload): Promise<CategoryEntity> {
    return apiRequest<CategoryEntity>('/category', {
      method: 'POST',
      data: payload,
    })
  }

  static async update(id: string, payload: CategoryPayload): Promise<void> {
    await apiRequest<void>(`/category/${id}`, {
      method: 'PUT',
      data: payload,
    })
  }

  static async delete(id: string): Promise<void> {
    await apiRequest<void>(`/category/${id}`, { method: 'DELETE' })
  }
}
