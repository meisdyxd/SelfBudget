const REQUEST_TIMEOUT_MS = 20_000

export async function fetchJsonWithTimeout(url, options) {
  const controller = new AbortController()
  let timedOut = false
  const timeoutId = setTimeout(() => {
    timedOut = true
    controller.abort()
  }, REQUEST_TIMEOUT_MS)

  try {
    const response = await fetch(url, { ...options, signal: controller.signal })
    let body = null
    let bodyInvalid = false
    if (response.status !== 204 && response.status !== 401) {
      try {
        body = await response.json()
      } catch (error) {
        if (timedOut || error?.name === 'AbortError') throw error
        bodyInvalid = true
      }
    }
    if (timedOut) throw new Error('Request timed out')
    return { response, body, bodyInvalid }
  } catch (error) {
    if (timedOut || error?.name === 'AbortError') {
      throw new Error('Сервер не ответил за 20 секунд. Проверьте соединение и повторите попытку.')
    }
    throw new Error('Не удалось связаться с сервером. Проверьте соединение и повторите попытку.')
  } finally {
    clearTimeout(timeoutId)
  }
}
