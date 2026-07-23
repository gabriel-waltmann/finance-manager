import { type AxiosRequestConfig } from 'axios'
import api from '.'

export async function apiRequest<T>(path: string, config: AxiosRequestConfig = {}): Promise<T> {
  const response = await api.request<T>({
    ...config,
    url: path,
  })

  return response.data
}



