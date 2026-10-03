import { useEffect, useRef } from 'react'
import { ArrowLeftRight, Check, X } from 'lucide-react'
import { formatMoney } from '../../domain/money.js'
import './transfers.css'

function formatReceiptDate(value) {
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? 'Время операции не указано'
    : new Intl.DateTimeFormat('ru-RU', { dateStyle: 'long', timeStyle: 'short' }).format(date)
}

export default function TransferReceiptDialog({ receipt, onClose }) {
  const closeButtonRef = useRef(null)
  const previousFocusRef = useRef(null)
  const dialogRef = useRef(null)

  useEffect(() => {
    previousFocusRef.current = document.activeElement
    closeButtonRef.current?.focus()
    return () => previousFocusRef.current?.focus?.()
  }, [])

  function handleDialogKeyDown(event) {
    if (event.key === 'Escape') {
      event.preventDefault()
      onClose()
      return
    }
    if (event.key !== 'Tab') return
    const focusable = [...(dialogRef.current?.querySelectorAll('button:not(:disabled), a[href], input:not(:disabled), select:not(:disabled)') || [])]
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

  const currency = receipt.currencyCode || 'RUB'
  const fromName = receipt.fromAccountName || 'Счёт отправителя'
  const toName = receipt.toAccountName || 'Счёт получателя'

  return (
    <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) onClose() }}>
      <section
        className="transfer-modal transfer-receipt-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="transfer-receipt-title"
        aria-describedby="transfer-receipt-status"
        tabIndex={-1}
        ref={dialogRef}
        onKeyDown={handleDialogKeyDown}
      >
        <header className="modal-header">
          <div className="receipt-heading">
            <span className="receipt-success-icon"><Check size={20} /></span>
            <div>
              <div className="section-kicker">КВИТАНЦИЯ СЕРВЕРА</div>
              <h2 id="transfer-receipt-title">Перевод выполнен</h2>
            </div>
          </div>
          <button ref={closeButtonRef} className="icon-button" type="button" aria-label="Закрыть квитанцию" onClick={onClose}><X size={19} /></button>
        </header>
        <p className="receipt-status" id="transfer-receipt-status">Операция подтверждена сервером SelfBudget.</p>
        <div className="receipt-amount">{formatMoney(receipt.amount, currency)}</div>
        <div className="receipt-route">
          <div className="receipt-account"><span>ОТКУДА</span><b>{fromName}</b></div>
          <span className="receipt-transfer-icon" aria-hidden="true"><ArrowLeftRight size={17} /></span>
          <div className="receipt-account"><span>КУДА</span><b>{toName}</b></div>
        </div>
        {receipt.note && <div className="receipt-note"><span>Комментарий</span><b>{receipt.note}</b></div>}
        <dl className="receipt-meta">
          <div><dt>Номер операции</dt><dd>{receipt.id}</dd></div>
          <div><dt>Дата и время</dt><dd>{formatReceiptDate(receipt.createdAt)}</dd></div>
          <div><dt>Валюта</dt><dd>{currency}</dd></div>
        </dl>
        <button className="button button-primary receipt-close" type="button" data-receipt-first-focus onClick={onClose}>Готово</button>
      </section>
    </div>
  )
}
