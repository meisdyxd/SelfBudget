import { clearSession, getSession } from '../ui/session.js'
import { fetchJsonWithTimeout } from './http.js'

export const API_BASE = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function apiRequest(path, options) {
  const accessToken = getSession()?.accessToken
  const { response, body, bodyInvalid } = await fetchJsonWithTimeout(`${API_BASE}${path}`, {
    ...options,
    headers: {
      ...options?.headers,
      ...(options?.body ? { 'Content-Type': 'application/json' } : {}),
      ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
    },
  })

  if (response.status === 401) {
    clearSession()
    window.location.assign('/auth?mode=login&reason=expired')
    throw new ApiError('Сеанс завершён. Войдите снова.', response.status)
  }
  if (!response.ok) {
    const message = body?.message || body?.Message || body?.error || body?.title || `Ошибка запроса (${response.status})`
    throw new ApiError(typeof message === 'string' ? message : 'Не удалось выполнить запрос', response.status)
  }
  if (response.status === 204) return null
  if (bodyInvalid) {
    const error = new ApiError('Сервер вернул данные в неожиданном формате.', response.status)
    error.outcomeUnknown = true
    throw error
  }
  return body
}
