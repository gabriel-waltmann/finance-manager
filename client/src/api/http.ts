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
    const message = body.error ?? body.Error ?? body.message ?? body.Message

    if (typeof message === 'string' && message.trim()) {
      return message
    }
  }

  if (typeof body === 'string' && body.trim()) {
    return body
  }

  return fallback || 'Request failed'
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}
