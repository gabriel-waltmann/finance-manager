import api from ".."
import axios from 'axios'
import { ErrorEntity } from "../../entities/ErrorEntity"

function readFirstValidationError(errors: unknown): string | undefined {
  if (!isRecord(errors)) {
    return undefined
  }

  for (const value of Object.values(errors)) {
    if (Array.isArray(value)) {
      const message = value.find((item): item is string => typeof item === 'string' && Boolean(item.trim()))

      if (message) {
        return message
      }
    }

    if (typeof value === 'string' && value.trim()) {
      return value
    }
  }

  return undefined
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function readErrorMessage(body: unknown, fallback: string): string {
  if (isRecord(body)) {
    const validationMessage = readFirstValidationError(body.errors)

    if (validationMessage) {
      return validationMessage
    }

    const message = body.error ?? body.Error ?? body.message ?? body.Message

    if (typeof message === 'string' && message.trim()) {
      return message
    }

    const problemMessage = body.detail ?? body.title

    if (typeof problemMessage === 'string' && problemMessage.trim()) {
      return problemMessage
    }
  }

  if (typeof body === 'string' && body.trim()) {
    return body
  }

  return fallback || 'Request failed'
}

function useInterceptor(error:unknown): Promise<unknown> {
  if (axios.isCancel(error)) {
    return Promise.reject(error)
  }

  if (axios.isAxiosError(error)) {
    if (error.response) {
      return Promise.reject(
        new ErrorEntity(
          readErrorMessage(error.response.data, error.response.statusText),
          error.response.status,
          error.response.data,
        ),
      )
    }

    return Promise.reject(new Error(error.message || 'Request failed'))
  }

  return Promise.reject(error)
}

api.interceptors.response.use(
  (response) => response,
  useInterceptor,
)