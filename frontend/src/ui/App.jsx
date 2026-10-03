import { useCallback, useEffect, useRef, useState } from 'react'
import {
  ArrowLeftRight, ArrowRight, Bell, Check,
  CircleHelp, ClipboardList, CreditCard, Eye, EyeOff, History, LayoutDashboard,
  Plus, RefreshCw, Settings2, Sparkles, Wallet,
} from 'lucide-react'
import { clearSession, getSession } from './session.js'
import { apiRequest as api, API_BASE } from '../api/client.js'
import { formatMoney as money, getCurrencyTotals } from '../domain/money.js'
import AccountCard from './accounts/AccountCard.jsx'
import TransferDialog from './transfers/TransferDialog.jsx'
import TransferReceiptDialog from './transfers/TransferReceiptDialog.jsx'
import './money.css'

const dateLabel = (value) => new Intl.DateTimeFormat('ru-RU', {
  day: 'numeric', month: 'long', hour: '2-digit', minute: '2-digit',
}).format(new Date(value))

function App() {
  const [accounts, setAccounts] = useState([])
  const [loading, setLoading] = useState(true)
  const [apiError, setApiError] = useState('')
  const [apiReady, setApiReady] = useState(false)
  const [transfers, setTransfers] = useState([])
  const [transferCount, setTransferCount] = useState(0)
  const [modalOpen, setModalOpen] = useState(false)
  const [receiptOpen, setReceiptOpen] = useState(false)
  const [receipt, setReceipt] = useState(null)
  const [transferSourceId, setTransferSourceId] = useState('')
  const transferAttemptRef = useRef(null)
  const [balanceHidden, setBalanceHidden] = useState(false)
  const [toast, setToast] = useState('')
  const todayLabel = new Intl.DateTimeFormat('ru-RU', {
    weekday: 'long', day: 'numeric', month: 'long',
  }).format(new Date()).toLocaleUpperCase('ru-RU')

  const loadAccounts = useCallback(async () => {
    setLoading(true)
    try {
      const result = await api('/api/Accounts')
      if (!Array.isArray(result)) throw new Error('Сервер вернул список счетов в неожиданном формате.')
      setAccounts(result)
      setApiError('')
      setApiReady(true)
    } catch (error) {
      setApiReady(false)
      setApiError(error.message || 'Не удалось подключиться к серверу')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { loadAccounts() }, [loadAccounts])
  useEffect(() => {
    function warnAboutPendingTransfer(event) {
      if (!transferAttemptRef.current?.unresolved) return
      event.preventDefault()
      event.returnValue = ''
    }

    window.addEventListener('beforeunload', warnAboutPendingTransfer)
    return () => window.removeEventListener('beforeunload', warnAboutPendingTransfer)
  }, [])
  useEffect(() => {
    if (!toast) return undefined
    const timer = setTimeout(() => setToast(''), 3600)
    return () => clearTimeout(timer)
  }, [toast])

  const currencyTotals = getCurrencyTotals(accounts)
  const currencies = new Set(accounts.map((account) => account.currencyCode || 'RUB'))
  const singleCurrencyTotal = currencyTotals[0] || { currency: 'RUB', label: money(0, 'RUB') }

  const accountById = (id) => accounts.find((account) => account.id === id)
  const hasSameCurrencyDestination = (source) => accounts.some((account) => account.id !== source.id
    && (account.currencyCode || 'RUB') === (source.currencyCode || 'RUB'))
  const canTransfer = accounts.some(hasSameCurrencyDestination)
  const openTransfer = (account) => {
    const source = account && hasSameCurrencyDestination(account)
      ? account
      : accounts.find(hasSameCurrencyDestination)
    setTransferSourceId(source?.id || '')
    setModalOpen(true)
  }

  const handleTransferSuccess = useCallback(async (transferReceipt) => {
    setTransfers((current) => [transferReceipt, ...current].slice(0, 5))
    setTransferCount((count) => count + 1)
    setReceipt(transferReceipt)
    setReceiptOpen(true)
    setModalOpen(false)
    await loadAccounts()
  }, [loadAccounts])

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <a className="brand" href="#home" aria-label="SelfBudget — главная">
          <span className="brand-mark"><Wallet size={21} strokeWidth={2.2} /></span>
          <span>self<span>budget</span></span>
        </a>
        <div className="workspace-label">МОЁ ПРОСТРАНСТВО</div>
        <nav className="side-nav" aria-label="Главная навигация">
          <a className="nav-item active" href="#home" aria-label="Обзор"><LayoutDashboard size={18} /><span>Обзор</span><span className="nav-dot" /></a>
          <button className="nav-item" type="button" aria-label="Мои счета" onClick={() => document.getElementById('accounts')?.scrollIntoView({ behavior: 'smooth' })}><CreditCard size={18} /><span>Мои счета</span><span className="nav-count">{accounts.length}</span></button>
          <a className="nav-item" href="/roadmap" aria-label="План проекта"><ClipboardList size={18} /><span>План проекта</span></a>
          <a className="nav-item" href="/history" aria-label="История операций"><History size={18} /><span>История операций</span></a>
          <button className="nav-item" type="button" aria-label="Переводы" onClick={() => document.getElementById('activity')?.scrollIntoView({ behavior: 'smooth' })}><ArrowLeftRight size={18} /><span>Переводы</span></button>
        </nav>
        <div className="sidebar-bottom">
          <div className="help-card"><div className="help-icon"><Sparkles size={16} /></div><strong>Тихий порядок<br />в финансах</strong><p>Деньги под контролем. Голова свободна.</p><a href="#accounts">К счетам <ArrowRight size={13} /></a></div>
          <button className="profile" type="button" onClick={() => {
            clearSession()
            window.location.assign('/auth?mode=login')
          }}>
            <div className="avatar">{(getSession()?.user?.name || 'С').trim().charAt(0).toUpperCase()}</div><span className="profile-copy"><b>{getSession()?.user?.name || 'Моё пространство'}</b><small>Завершить сеанс</small></span><Settings2 size={17} className="profile-settings" />
          </button>
        </div>
      </aside>

      <main className="main" id="home">
        <header className="topbar">
          <div className="breadcrumb"><span>Пространство</span><span className="crumb-slash">/</span><b>Обзор</b></div>
          <div className="top-actions">
            <span className={`connection ${apiReady ? 'online' : 'offline'}`}><i />{apiReady ? 'Синхронизировано' : 'Нет соединения'}</span>
            <button className="icon-button notification" aria-label="Уведомления" onClick={() => setToast('Уведомлений пока нет.')}><Bell size={18} /><span /></button>
            <div className="avatar avatar-small">{(getSession()?.user?.name || 'С').trim().charAt(0).toUpperCase()}</div>
          </div>
        </header>

        <div className="page-content">
          <section className="welcome-row">
            <div><div className="eyebrow"><span className="eyebrow-line" />{todayLabel}</div><h1>Деньги любят <em>ясность.</em></h1><p className="welcome-subtitle">Всё важное о ваших счетах — в одном спокойном месте.</p></div>
            <button className="button button-primary" onClick={() => openTransfer()} disabled={!apiReady || !canTransfer}><Plus size={17} /> Новый перевод</button>
          </section>

          {apiError && <div className="api-banner"><div className="api-banner-icon"><CircleHelp size={18} /></div><div><b>{accounts.length ? 'Не удалось обновить счета' : 'Не удалось загрузить счета'}</b><p>{apiError}{accounts.length ? ' Показанные остатки могут быть устаревшими; переводы временно отключены.' : ''} Проверьте адрес и доступность API <code>{API_BASE || 'на этом origin'}</code>.</p></div><button className="button button-quiet" onClick={loadAccounts}><RefreshCw size={15} /> Повторить</button></div>}

          <section className="overview-grid" aria-label="Сводка">
            <article className="balance-card">
              <div className="balance-top"><div className="balance-label">ОБЩИЙ БАЛАНС <span className="balance-sparkle"><Sparkles size={13} /></span></div><button className="balance-eye" aria-label={balanceHidden ? 'Показать баланс' : 'Скрыть баланс'} onClick={() => setBalanceHidden((value) => !value)}>{balanceHidden ? <Eye size={17} /> : <EyeOff size={17} />}</button></div>
              <div className={`balance-amount ${currencyTotals.length > 1 ? 'balance-amount-multi' : ''}`}>
                {loading ? <span className="skeleton skeleton-balance" /> : balanceHidden ? '••••••' : currencyTotals.length > 1
                  ? <span className="balance-currency-list">{currencyTotals.map((total) => <span className="balance-currency-total" key={total.currency}>{total.label}<small>{total.currency}</small></span>)}</span>
                  : <span className="balance-currency-total">{singleCurrencyTotal.label}<small>{singleCurrencyTotal.currency}</small></span>}
              </div>
              <div className="balance-meta"><span className="balance-chip"><span className="balance-chip-dot" />Мои счета</span><span className="balance-subtle">{accounts.length} {plural(accounts.length, 'счёт', 'счёта', 'счетов')}</span></div>
              <div className="balance-orb orb-one" /><div className="balance-orb orb-two" />
            </article>
            <article className="stat-card"><div className="stat-heading"><span className="stat-icon mint"><Wallet size={17} /></span><span className="stat-caption">Активные счета</span></div><div className="stat-number">{loading ? '—' : accounts.length.toString().padStart(2, '0')}</div><div className="stat-foot"><span className="stat-foot-dot" />{currencies.size === 0 ? 'Подключите данные счетов' : `${currencies.size} ${plural(currencies.size, 'валюта', 'валюты', 'валют')}`}</div></article>
            <article className="stat-card transfer-stat"><div className="stat-heading"><span className="stat-icon peach"><ArrowLeftRight size={17} /></span><span className="stat-caption">Переводы в этой вкладке</span></div><div className="stat-number">{transferCount.toString().padStart(2, '0')}</div><div className="stat-foot"><span className="stat-foot-dot peach-dot" />Подтверждены сервером</div></article>
          </section>

          <section className="section-block" id="accounts">
            <div className="section-heading"><div><div className="section-kicker">ВАШИ ДЕНЬГИ</div><h2>Мои счета <span className="title-count">{accounts.length}</span></h2></div><div className="section-tools"><button className="icon-button refresh-button" title="Обновить счета" aria-label="Обновить счета" onClick={loadAccounts}><RefreshCw size={16} className={loading ? 'spinning' : ''} /></button></div></div>
            {loading ? <div className="accounts-grid"><div className="account-skeleton" /><div className="account-skeleton" /><div className="account-skeleton" /></div> : accounts.length > 0 ? <div className="accounts-grid">{accounts.map((account, index) => <AccountCard key={account.id} account={account} index={index} hidden={balanceHidden} apiReady={apiReady} canTransfer={hasSameCurrencyDestination(account)} onOpen={() => window.location.assign(`/accounts/${encodeURIComponent(account.id)}`)} onTransfer={() => openTransfer(account)} />)}</div> : <div className="empty-state"><div className="empty-illustration"><Wallet size={24} /></div><h3>{apiError ? 'Счета появятся после подключения' : 'Здесь начнётся ваш порядок'}</h3><p>{apiError ? 'Когда сервер станет доступен, мы загрузим ваши счета автоматически.' : 'Подключите данные API, чтобы увидеть счета и текущие балансы.'}</p><button className="button button-quiet" onClick={loadAccounts}><RefreshCw size={15} /> Обновить данные</button></div>}
          </section>

          <section className="section-block activity-section" id="activity">
            <div className="section-heading"><div><div className="section-kicker">ДВИЖЕНИЕ ДЕНЕГ</div><h2>Недавние переводы <span className="title-count">{transfers.length}</span></h2></div><span className="local-note">Только квитанции, подтверждённые API в этой вкладке</span></div>
            {transfers.length ? <div className="activity-list">{transfers.map((item) => <TransferRow key={item.id} transfer={item} from={accountById(item.fromAccountId)} to={accountById(item.toAccountId)} />)}</div> : <div className="activity-empty"><span className="activity-empty-icon"><ArrowLeftRight size={18} /></span><span><b>Пока тихо</b><small>После перевода здесь появится серверная квитанция. Полная история будет в следующем разделе.</small></span><button className="text-button" disabled={!canTransfer || !apiReady} onClick={() => openTransfer()}>{canTransfer ? 'Сделать перевод' : 'Нужны два счёта одной валюты'} <ArrowRight size={14} /></button></div>}
          </section>
          <footer className="page-footer"><span>SelfBudget <span>·</span> ваши финансы в ясности</span><span className="footer-secure"><span />Данные приходят напрямую из API</span></footer>
        </div>
      </main>

      {modalOpen && <TransferDialog accounts={accounts} initialSourceId={transferSourceId} attemptRef={transferAttemptRef} apiReady={apiReady} onClose={() => setModalOpen(false)} onSuccess={handleTransferSuccess} />}
      {receiptOpen && receipt && <TransferReceiptDialog receipt={receipt} onClose={() => setReceiptOpen(false)} />}
      {toast && <div className="toast"><span><Check size={15} /></span>{toast}</div>}
    </div>
  )
}

function plural(value, one, few, many) {
  const mod10 = value % 10
  const mod100 = value % 100
  if (mod10 === 1 && mod100 !== 11) return one
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return few
  return many
}

function TransferRow({ transfer, from, to }) {
  const fromName = from?.name || transfer.fromAccountName || 'Счёт отправителя'
  const toName = to?.name || transfer.toAccountName || 'Счёт получателя'
  const currency = transfer.currencyCode || from?.currencyCode || to?.currencyCode || 'RUB'
  return <article className="activity-row"><span className="transfer-icon"><ArrowLeftRight size={17} /></span><span className="transfer-info"><b>{fromName} <span className="transfer-arrow">→</span> {toName}</b><small>{transfer.note || 'Перевод между своими счетами'} <span>·</span> {dateLabel(transfer.createdAt)}</small></span><span className="transfer-amount">−{money(transfer.amount, currency)}</span><span className="transfer-status" aria-label="Подтверждено сервером"><Check size={12} /></span></article>
}

export default App
