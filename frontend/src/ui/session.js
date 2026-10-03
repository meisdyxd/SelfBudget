const SESSION_KEY = 'selfbudget.session.v1'
const MAX_TIMEOUT_MS = 2_147_000_000

function getSessionStorage() {
  try {
    return window.sessionStorage
  } catch {
    return null
  }
}

export function clearSession() {
  try {
    getSessionStorage()?.removeItem(SESSION_KEY)
  } catch {
    // A blocked storage area must not prevent the app from ending a session.
  }
}

export function saveSession(session) {
  const expiresAt = Date.parse(session?.expiresAt)
  if (!session?.accessToken || !Number.isFinite(expiresAt) || expiresAt <= Date.now()) {
    throw new Error('Сервер вернул некорректную или уже истёкшую сессию. Попробуйте войти ещё раз.')
  }

  try {
    const storage = getSessionStorage()
    if (!storage) throw new Error('Storage is unavailable')
    storage.setItem(SESSION_KEY, JSON.stringify({
      accessToken: session.accessToken,
      expiresAt: session.expiresAt,
      user: session.user || null,
    }))
  } catch {
    throw new Error('Браузер не разрешил сохранить временный сеанс. Включите поддержку хранилища сайта и войдите снова.')
  }
}

export function getSession() {
  let value
  try {
    value = getSessionStorage()?.getItem(SESSION_KEY) || null
  } catch {
    return null
  }
  if (!value) return null

  try {
    const session = JSON.parse(value)
    const expiresAt = Date.parse(session?.expiresAt)
    if (!session?.accessToken || !Number.isFinite(expiresAt) || expiresAt <= Date.now()) {
      clearSession()
      return null
    }
    return session
  } catch {
    clearSession()
    return null
  }
}

export function monitorSessionExpiration() {
  let timeoutId

  function scheduleCheck() {
    const session = getSession()
    if (!session) {
      const isPublicRoute = window.location.pathname === '/auth' || window.location.pathname === '/roadmap'
      if (!isPublicRoute) window.location.replace('/auth?mode=login&reason=expired')
      return
    }

    const timeRemaining = Date.parse(session.expiresAt) - Date.now()
    timeoutId = window.setTimeout(scheduleCheck, Math.min(Math.max(timeRemaining, 1), MAX_TIMEOUT_MS))
  }

  scheduleCheck()
  return () => window.clearTimeout(timeoutId)
}
