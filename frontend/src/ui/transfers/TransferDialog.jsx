import { useEffect, useRef, useState } from 'react'
import { ArrowDownLeft, CircleHelp, LoaderCircle, Send, X } from 'lucide-react'
import { apiRequest as api } from '../../api/client.js'
import { currencyFractionDigits, formatMoney, parsePositiveMoneyAmount } from '../../domain/money.js'
import './transfers.css'

function accountCurrency(account) {
  return account?.currencyCode || 'RUB'
}

function makeIdempotencyKey() {
  if (globalThis.crypto?.randomUUID) return globalThis.crypto.randomUUID()
  throw new Error('Для безопасной отправки перевода нужен современный браузер.')
}

function getTransferErrorMessage(error) {
  if (error.status === 400) return 'Сервер отклонил сумму или выбранные счета. Проверьте данные перевода.'
  if (error.status === 404) return 'Один из счетов недоступен. Обновите список и выберите счета снова.'
  if (error.status === 409) return 'Сервер не провёл перевод. Проверьте доступный остаток и повторите попытку.'
  return error.message || 'Не удалось связаться с сервером. Повторите запрос.'
}

function isMatchingReceipt(receipt, { source, destination, currency, submittedAmount }) {
  if (!receipt || typeof receipt !== 'object' || Array.isArray(receipt)) return false
  if (typeof receipt.id !== 'string' || !receipt.id.trim()
    || receipt.fromAccountId !== source.id || receipt.toAccountId !== destination.id
    || typeof receipt.fromAccountName !== 'string' || !receipt.fromAccountName.trim()
    || typeof receipt.toAccountName !== 'string' || !receipt.toAccountName.trim()
    || receipt.currencyCode !== currency || receipt.status !== 'completed'
    || (receipt.note != null && typeof receipt.note !== 'string')) return false

  if (typeof receipt.amount !== 'string' || !/^\d+(?:\.\d+)?$/.test(receipt.amount)) return false
  const receiptAmount = parsePositiveMoneyAmount(receipt.amount, currency)
  if (!receiptAmount || receiptAmount.minorUnits !== submittedAmount.minorUnits) return false

  return typeof receipt.createdAt === 'string'
    && /(?:Z|\+00:00)$/i.test(receipt.createdAt)
    && Number.isFinite(Date.parse(receipt.createdAt))
}

export default function TransferDialog({ accounts, initialSourceId, attemptRef, apiReady, onClose, onSuccess }) {
  const initialSource = accounts.find((account) => account.id === initialSourceId)
  const initialDestination = accounts.find((account) => account.id !== initialSource?.id
    && accountCurrency(account) === accountCurrency(initialSource))
  const [form, setForm] = useState({
    from: initialSource?.id || '',
    to: initialDestination?.id || '',
    amount: '',
    note: '',
  })
  const [sending, setSending] = useState(false)
  const [retryAvailable, setRetryAvailable] = useState(false)
  const [error, setError] = useState('')
  const dialogRef = useRef(null)
  const amountRef = useRef(null)
  const previousFocusRef = useRef(null)

  const source = accounts.find((account) => account.id === form.from)
  const destinations = source
    ? accounts.filter((account) => account.id !== source.id && accountCurrency(account) === accountCurrency(source))
    : []
  const destination = destinations.find((account) => account.id === form.to)
  const currency = accountCurrency(source)
  const fractionDigits = currencyFractionDigits(currency) ?? 2

  useEffect(() => {
    previousFocusRef.current = document.activeElement
    amountRef.current?.focus()
    return () => previousFocusRef.current?.focus?.()
  }, [])

  function updateForm(field, value) {
    if (sending || retryAvailable || attemptRef.current?.unresolved) return
    setForm((current) => ({ ...current, [field]: value }))
    setError('')
    setRetryAvailable(false)
  }

  function selectSource(accountId) {
    if (sending || retryAvailable || attemptRef.current?.unresolved) return
    const nextSource = accounts.find((account) => account.id === accountId)
    const nextDestination = accounts.find((account) => account.id !== accountId
      && accountCurrency(account) === accountCurrency(nextSource))
    setForm((current) => ({ ...current, from: accountId, to: nextDestination?.id || '' }))
    setError('')
    setRetryAvailable(false)
  }

  function handleDialogKeyDown(event) {
    if (event.key === 'Escape') {
      event.preventDefault()
      if (!sending && !retryAvailable && !attemptRef.current?.unresolved) onClose()
      return
    }

    if (event.key !== 'Tab') return
    const focusable = [...(dialogRef.current?.querySelectorAll('button:not(:disabled), input:not(:disabled), select:not(:disabled)') || [])]
    if (focusable.length === 0) return
    const first = focusable[0]
    const last = focusable[focusable.length - 1]
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault()
      last.focus()
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault()
      first.focus()
    }
  }

  async function submitTransfer(event) {
    event.preventDefault()
    setError('')
    setRetryAvailable(false)

    if (!source || !destination) {
      setError('Выберите два разных счёта в одной валюте.')
      return
    }
    const parsedAmount = parsePositiveMoneyAmount(form.amount, currency)
    if (!parsedAmount) {
      setError(`Введите сумму больше нуля, не более ${fractionDigits} знаков после запятой.`)
      amountRef.current?.focus()
      return
    }

    const payload = {
      fromAccountId: source.id,
      toAccountId: destination.id,
      amount: parsedAmount.decimal,
      note: form.note.trim() || null,
    }
    const fingerprint = JSON.stringify(payload)
    let attempt = attemptRef.current
    if (attempt?.unresolved && attempt.fingerprint !== fingerprint) {
      setError('Есть перевод с неизвестным результатом. Сначала повторите его с прежними параметрами.')
      setRetryAvailable(true)
      return
    }
    if (!attempt || attempt.fingerprint !== fingerprint) {
      try {
        if (attempt?.unresolved) {
          setError('Есть перевод с неизвестным результатом. Повторите его до создания нового перевода.')
          setRetryAvailable(true)
          return
        }
        attempt = { fingerprint, key: makeIdempotencyKey(), payload, unresolved: false }
        attemptRef.current = attempt
      } catch (keyError) {
        setError(keyError.message)
        return
      }
    }

    attempt = { ...attempt, payload, unresolved: true }
    attemptRef.current = attempt
    setSending(true)
    try {
      const receipt = await api('/api/Transfers', {
        method: 'POST',
        headers: { 'Idempotency-Key': attempt.key },
        body: JSON.stringify(payload),
      })
      if (!isMatchingReceipt(receipt, { source, destination, currency, submittedAmount: parsedAmount })) {
        setError('Ответ не подтвердил этот перевод полной квитанцией. Повторите тот же запрос с тем же ключом; параметры заблокированы до подтверждения результата.')
        setRetryAvailable(true)
        return
      }
      attemptRef.current = null
      await onSuccess(receipt)
    } catch (requestError) {
      setError(getTransferErrorMessage(requestError))
      const outcomeUnknown = Boolean(requestError.outcomeUnknown || !requestError.status || requestError.status >= 500)
      if (outcomeUnknown) {
        attemptRef.current = { ...attempt, payload, unresolved: true }
      } else {
        attemptRef.current = null
      }
      setRetryAvailable(outcomeUnknown)
    } finally {
      setSending(false)
    }
  }

  return (
    <div
      className="modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !sending && !retryAvailable && !attemptRef.current?.unresolved) onClose()
      }}
    >
      <section
        className="transfer-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="transfer-title"
        aria-describedby="transfer-description"
        tabIndex={-1}
        ref={dialogRef}
        onKeyDown={handleDialogKeyDown}
      >
        <header className="modal-header">
          <div>
            <div className="section-kicker">МЕЖДУ ВАШИМИ СЧЕТАМИ</div>
            <h2 id="transfer-title">Новый перевод</h2>
          </div>
          <button className="icon-button" type="button" aria-label="Закрыть" onClick={onClose} disabled={sending || retryAvailable || Boolean(attemptRef.current?.unresolved)}><X size={19} /></button>
        </header>
        <p id="transfer-description" className="transfer-description">Перевод проходит только между вашими счетами в одной валюте.</p>
        <form onSubmit={submitTransfer}>
          <label className="field-label">Откуда
            <select value={form.from} onChange={(event) => selectSource(event.target.value)} required disabled={sending || retryAvailable}>
              <option value="" disabled>Выберите счёт</option>
              {accounts.map((account) => <option key={account.id} value={account.id}>{account.name} · {formatMoney(account.balance, accountCurrency(account))}</option>)}
            </select>
          </label>
          <div className="swap-indicator" aria-hidden="true"><ArrowDownLeft size={16} /></div>
          <label className="field-label">Куда
            <select value={form.to} onChange={(event) => updateForm('to', event.target.value)} required disabled={sending || retryAvailable || !source || destinations.length === 0}>
              <option value="" disabled>{source ? `Нет другого счёта в ${currency}` : 'Сначала выберите счёт отправителя'}</option>
              {destinations.map((account) => <option key={account.id} value={account.id}>{account.name} · {formatMoney(account.balance, accountCurrency(account))}</option>)}
            </select>
          </label>
          <label className="field-label">Сумма
            <div className="amount-input">
              <input
                ref={amountRef}
                autoFocus
                autoComplete="off"
                inputMode="decimal"
                type="text"
                placeholder={fractionDigits === 0 ? '0' : `0,${'0'.repeat(fractionDigits)}`}
                value={form.amount}
                onChange={(event) => updateForm('amount', event.target.value)}
                aria-describedby="transfer-amount-hint"
                required
                disabled={sending || retryAvailable}
              />
              <span>{currency}</span>
            </div>
          </label>
          <p className="transfer-amount-hint" id="transfer-amount-hint">Можно указать до {fractionDigits} знаков после запятой.</p>
          <label className="field-label">Комментарий <span className="optional">необязательно</span>
            <input type="text" maxLength="240" placeholder="Например, на накопительный счёт" value={form.note} onChange={(event) => updateForm('note', event.target.value)} disabled={sending || retryAvailable} />
          </label>
          {error && <div className="form-error" role="alert">{error}{retryAvailable && <span className="retry-hint"> Ключ запроса сохранён. Повтор безопасен; параметры нельзя менять до ответа сервера.</span>}</div>}
          <div className="modal-hint"><CircleHelp size={15} /><span>Остаток и возможность перевода окончательно проверяет сервер.</span></div>
          <button className="button button-primary submit-button" type="submit" disabled={sending || !apiReady || !destination}>
            {sending ? <><LoaderCircle size={17} className="spinning" /> Выполняем перевод…</> : <><Send size={16} />{retryAvailable ? 'Безопасно повторить' : 'Перевести'}</>}
          </button>
          {!apiReady && <p className="disabled-hint">Подключите сервер, чтобы выполнить перевод.</p>}
          {source && destinations.length === 0 && <p className="disabled-hint">Для перевода нужен ещё один счёт в валюте {currency}.</p>}
        </form>
      </section>
    </div>
  )
}
