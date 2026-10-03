import { useCallback, useEffect, useState } from 'react'
import { ArrowLeft, CircleHelp, CreditCard, RefreshCw, Wallet } from 'lucide-react'
import { apiRequest as api } from '../../api/client.js'
import { formatMoney } from '../../domain/money.js'
import './account-details.css'

export default function AccountDetails({ accountId }) {
  const [account, setAccount] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const loadAccount = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await api(`/api/Accounts/${encodeURIComponent(accountId)}`)
      if (!result || typeof result !== 'object') throw new Error('Сервер вернул пустой ответ.')
      setAccount(result)
    } catch (loadError) {
      setAccount(null)
      setError(loadError)
    } finally {
      setLoading(false)
    }
  }, [accountId])

  useEffect(() => { loadAccount() }, [loadAccount])

  const currency = account?.currencyCode || 'RUB'
  const unavailable = error?.status === 404

  return (
    <main className="account-details-page">
      <header className="account-details-header">
        <a className="account-details-back" href="/#accounts">
          <ArrowLeft size={18} /> К моим счетам
        </a>
        <a className="account-details-brand" href="/" aria-label="SelfBudget — главная">
          <span><Wallet size={19} /></span> SelfBudget
        </a>
      </header>

      <div className="account-details-content">
        <div className="account-details-eyebrow">МОЁ ПРОСТРАНСТВО <span>/</span> СЧЕТА</div>
        <h1>Детали счёта</h1>
        <p className="account-details-intro">Информация о вашем счёте и доступных средствах.</p>

        {loading ? (
          <section className="account-details-card account-details-loading" aria-busy="true" aria-live="polite">
            <div className="details-skeleton details-skeleton-short" />
            <div className="details-skeleton details-skeleton-title" />
            <div className="details-skeleton details-skeleton-balance" />
            <div className="details-skeleton details-skeleton-row" />
            <div className="details-skeleton details-skeleton-row" />
            <span className="sr-only">Загружаем данные счёта…</span>
          </section>
        ) : error ? (
          <section className="account-details-card account-details-error" role="alert">
            <span className="account-details-error-icon"><CircleHelp size={22} /></span>
            <h2>{unavailable ? 'Счёт недоступен' : 'Не удалось загрузить счёт'}</h2>
            <p>{unavailable ? 'Счёт не найден или у вас нет к нему доступа.' : 'Проверьте соединение и попробуйте загрузить данные ещё раз.'}</p>
            <div className="account-details-error-actions">
              {!unavailable && <button className="details-retry" type="button" onClick={loadAccount}><RefreshCw size={16} /> Повторить</button>}
              <a className="details-return" href="/#accounts">Вернуться к счетам</a>
            </div>
          </section>
        ) : (
          <section className="account-details-card">
            <div className="account-details-card-heading">
              <span className="account-details-icon"><CreditCard size={22} /></span>
              <div>
                <span className="account-details-type">{account.type || 'Счёт'}</span>
                <h2>{account.name || 'Без названия'}</h2>
              </div>
            </div>
            <div className="account-details-balance-label">ОСТАТОК НА СЧЁТЕ</div>
            <div className="account-details-balance">{formatMoney(account.balance, currency)}</div>
            <div className="account-details-code">Валюта счёта <b>{currency}</b></div>
            <div className="account-details-limit">
              <span>Лимит овердрафта</span>
              <b>{account.overdraftLimit == null ? '—' : formatMoney(account.overdraftLimit, currency)}</b>
            </div>
            <a className="details-return details-return-primary" href="/#accounts">Вернуться к моим счетам</a>
          </section>
        )}
      </div>
    </main>
  )
}
