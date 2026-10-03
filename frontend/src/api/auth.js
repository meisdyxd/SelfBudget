import { API_BASE } from './client.js'
import { fetchJsonWithTimeout } from './http.js'

async function submitAuthRequest(path, payload) {
  const { response, body, bodyInvalid } = await fetchJsonWithTimeout(`${API_BASE}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    if (response.status === 409) throw new Error('Аккаунт с такой почтой уже существует. Войдите или укажите другую почту.')
    const validationErrors = Object.values(body?.errors || {}).flat().filter(Boolean)
    const message = validationErrors[0] || body?.detail || body?.message || body?.Message || body?.title
    throw new Error(typeof message === 'string' ? message : 'Не удалось выполнить запрос. Проверьте данные и повторите попытку.')
  }
  if (bodyInvalid) throw new Error('Сервер вернул ответ в неожиданном формате. Попробуйте позже.')

  return body
}

export async function registerUser({ name, email, birthdate, password }) {
  const user = await submitAuthRequest('/api/auth/register', { name, email, birthdate, password })
  if (!user || typeof user !== 'object' || Array.isArray(user)
    || typeof user.id !== 'string' || typeof user.name !== 'string' || typeof user.email !== 'string') {
    throw new Error('Сервер не вернул данные созданного аккаунта в ожидаемом формате.')
  }
  return user
}

export async function loginUser({ email, password }) {
  const session = await submitAuthRequest('/api/auth/login', { email, password })
  if (!session || typeof session !== 'object' || Array.isArray(session)
    || typeof session.accessToken !== 'string' || !session.accessToken
    || typeof session.expiresAt !== 'string'
    || !session.user || typeof session.user !== 'object'
    || typeof session.user.id !== 'string' || typeof session.user.name !== 'string'
    || typeof session.user.email !== 'string') {
    throw new Error('Сервер не вернул данные сеанса в ожидаемом формате.')
  }
  return session
}
