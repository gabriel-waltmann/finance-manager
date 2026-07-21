export class ApiError extends Error {
  readonly status: number
  readonly details: unknown

  constructor(message: string, status: number, details: unknown) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.details = details
  }
}

const apiBase = '/api'

export async function apiRequest<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${apiBase}${path}`, {
    ...options,
    headers: buildHeaders(options),
  })

  const text = await response.text()
  const body = parseBody(text)

  if (!response.ok) {
    throw new ApiError(readErrorMessage(body, response.statusText), response.status, body)
  }

  return body as T
}

function buildHeaders(options: RequestInit): HeadersInit {
  const headers = new Headers(options.headers)

  if (!(options.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  return headers
}

function parseBody(text: string): unknown {
  if (!text) {
    return undefined
  }

  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
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
