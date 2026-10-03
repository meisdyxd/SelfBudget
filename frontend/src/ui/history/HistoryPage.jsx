import { useCallback, useEffect, useRef, useState } from 'react'
import { ArrowLeft, ArrowRight, ArrowUpRight, CalendarDays, CircleHelp, History, RefreshCw, RotateCcw, Search, Wallet } from 'lucide-react'
import { apiRequest } from '../../api/client.js'
import { getTransaction, getTransactionCategories, getTransactions } from '../../api/transactions.js'
import { formatMoney } from '../../domain/money.js'
import './history.css'

const EMPTY_FILTERS = { accountId: '', kind: '', categoryId: '', fromDate: '', toDate: '' }
const PAGE_SIZE = 20

function requestFilters(filters) {
  return {
    accountId: filters.accountId || undefined,
    kind: filters.kind || undefined,
    categoryId: filters.categoryId || undefined,
    fromInclusive: filters.fromDate ? new Date(`${filters.fromDate}T00:00:00`).toISOString() : undefined,
    toExclusive: filters.toDate ? nextLocalDayStart(filters.toDate) : undefined,
  }
}

function nextLocalDayStart(value) {
  const date = new Date(`${value}T00:00:00`)
  date.setDate(date.getDate() + 1)
  return date.toISOString()
}

function listItems(result) {
  return Array.isArray(result) ? result : result?.items
}

function operationLabel(kind) {
  if (kind === 'income') return 'Поступление'
  if (kind === 'expense') return 'Списание'
  if (kind === 'transfer') return 'Перевод'
  return 'Операция'
}

function operationSign(kind) {
  if (kind === 'income') return '+'
  if (kind === 'expense') return '−'
  return ''
}

function formatOperationAmount(amount, kind, currency) {
  const formatted = formatMoney(amount, currency)
  return formatted === '—' ? formatted : `${operationSign(kind)}${formatted}`
}

function formatDateTime(value) {
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? 'Дата не указана'
    : new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(date)
}

function formatDay(value) {
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? 'Другие даты'
    : new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' }).format(date)
}

function HistoryHeader() {
  return (
    <header className="history-header">
      <a className="history-brand" href="/" aria-label="SelfBudget — обзор">
        <span><Wallet size={19} /></span> SelfBudget
      </a>
      <a className="history-back" href="/"><ArrowLeft size={17} /> К обзору</a>
    </header>
  )
}

function PageShell({ children }) {
  return <div className="history-page"><HistoryHeader />{children}</div>
}

export default function HistoryPage({ transactionId }) {
  return transactionId ? <OperationDetails transactionId={transactionId} /> : <HistoryList />
}

function HistoryList() {
  const [draftFilters, setDraftFilters] = useState(EMPTY_FILTERS)
  const [activeFilters, setActiveFilters] = useState(EMPTY_FILTERS)
  const [accounts, setAccounts] = useState([])
  const [categories, setCategories] = useState([])
  const [filterOptionsError, setFilterOptionsError] = useState('')
  const [filterValidationError, setFilterValidationError] = useState('')
  const [items, setItems] = useState([])
  const [nextCursor, setNextCursor] = useState(null)
  const [hasMore, setHasMore] = useState(false)
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [error, setError] = useState('')
  const [loadMoreError, setLoadMoreError] = useState('')
  const requestVersion = useRef(0)
  const loadingMoreLock = useRef(false)

  const loadOptions = useCallback(async () => {
    const [accountsResult, categoriesResult] = await Promise.allSettled([
      apiRequest('/api/Accounts'),
      getTransactionCategories(),
    ])
    const errors = []
    if (accountsResult.status === 'fulfilled') {
      setAccounts(Array.isArray(accountsResult.value) ? accountsResult.value : [])
    } else {
      errors.push('счета')
    }
    if (categoriesResult.status === 'fulfilled') {
      setCategories(listItems(categoriesResult.value) || [])
    } else {
      errors.push('категории')
    }
    setFilterOptionsError(errors.length ? `Не удалось загрузить фильтры: ${errors.join(', ')}.` : '')
  }, [])

  const loadFirstPage = useCallback(async (filters) => {
    const version = ++requestVersion.current
    setActiveFilters(filters)
    setItems([])
    setNextCursor(null)
    setHasMore(false)
    setError('')
    setLoadMoreError('')
    setLoading(true)
    setLoadingMore(false)
    loadingMoreLock.current = false
    try {
      const result = await getTransactions(requestFilters(filters), null, PAGE_SIZE)
      if (version !== requestVersion.current) return
      const pageItems = listItems(result)
      if (!Array.isArray(pageItems)) throw new Error('Сервер вернул историю в неожиданном формате.')
      setItems(pageItems)
      setNextCursor(result?.nextCursor || null)
      setHasMore(Boolean(result?.hasMore && result?.nextCursor))
    } catch (loadError) {
      if (version === requestVersion.current) setError(loadError.message || 'Не удалось загрузить историю операций.')
    } finally {
      if (version === requestVersion.current) setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadOptions()
    void loadFirstPage(EMPTY_FILTERS)
  }, [loadFirstPage, loadOptions])

  async function loadMore() {
    if (!hasMore || !nextCursor || loadingMoreLock.current) return
    const version = requestVersion.current
    loadingMoreLock.current = true
    setLoadingMore(true)
    setLoadMoreError('')
    try {
      const result = await getTransactions(requestFilters(activeFilters), nextCursor, PAGE_SIZE)
      if (version !== requestVersion.current) return
      const pageItems = listItems(result)
      if (!Array.isArray(pageItems)) throw new Error('Сервер вернул историю в неожиданном формате.')
      setItems((current) => {
        const existingIds = new Set(current.map((item) => item.id))
        return [...current, ...pageItems.filter((item) => !existingIds.has(item.id))]
      })
      setNextCursor(result?.nextCursor || null)
      setHasMore(Boolean(result?.hasMore && result?.nextCursor))
    } catch (loadError) {
      if (version === requestVersion.current) setLoadMoreError(loadError.message || 'Не удалось загрузить следующую страницу.')
    } finally {
      loadingMoreLock.current = false
      if (version === requestVersion.current) setLoadingMore(false)
    }
  }

  function applyFilters(event) {
    event.preventDefault()
    setFilterValidationError('')
    if (draftFilters.fromDate && draftFilters.toDate && draftFilters.fromDate > draftFilters.toDate) {
      setFilterValidationError('Дата начала периода должна быть раньше даты окончания.')
      return
    }
    void loadFirstPage({ ...draftFilters })
  }

  function resetFilters() {
    setDraftFilters(EMPTY_FILTERS)
    setFilterValidationError('')
    void loadFirstPage(EMPTY_FILTERS)
  }

  const groups = []
  for (const item of items) {
    const title = formatDay(item.createdAt)
    const lastGroup = groups[groups.length - 1]
    if (lastGroup?.title === title) lastGroup.items.push(item)
    else groups.push({ title, items: [item] })
  }

  return (
    <PageShell>
      <main className="history-content">
        <div className="history-eyebrow"><History size={15} /> МОИ ОПЕРАЦИИ</div>
        <h1>История операций</h1>
        <p className="history-intro">Переводы, поступления и списания по вашим счетам.</p>

        <form className="history-filters" onSubmit={applyFilters}>
          <div className="history-filters-heading">
            <div><Search size={17} /><b>Фильтры</b></div>
            <button className="history-reset" type="button" onClick={resetFilters}><RotateCcw size={14} /> Сбросить</button>
          </div>
          <div className="history-filter-grid">
            <label>Счёт
              <select value={draftFilters.accountId} onChange={(event) => setDraftFilters((current) => ({ ...current, accountId: event.target.value }))}>
                <option value="">Все счета</option>
                {accounts.map((account) => <option key={account.id} value={account.id}>{account.name || 'Без названия'}</option>)}
              </select>
            </label>
            <label>Вид операции
              <select value={draftFilters.kind} onChange={(event) => setDraftFilters((current) => ({ ...current, kind: event.target.value }))}>
                <option value="">Все виды</option>
                <option value="transfer">Перевод</option>
                <option value="income">Поступление</option>
                <option value="expense">Списание</option>
              </select>
            </label>
            <label>Категория
              <select value={draftFilters.categoryId} onChange={(event) => setDraftFilters((current) => ({ ...current, categoryId: event.target.value }))}>
                <option value="">Все категории</option>
                {categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}
              </select>
            </label>
            <label>С даты
              <span className="history-date-control"><CalendarDays size={15} /><input type="date" value={draftFilters.fromDate} onChange={(event) => setDraftFilters((current) => ({ ...current, fromDate: event.target.value }))} /></span>
            </label>
            <label>По дату включительно
              <span className="history-date-control"><CalendarDays size={15} /><input type="date" value={draftFilters.toDate} onChange={(event) => setDraftFilters((current) => ({ ...current, toDate: event.target.value }))} /></span>
            </label>
          </div>
          {filterOptionsError && <p className="history-filter-options-error" role="status">{filterOptionsError} <button type="button" onClick={loadOptions}>Повторить</button></p>}
          {filterValidationError && <p className="history-inline-error" role="alert">{filterValidationError}</p>}
          <button className="history-apply" type="submit" disabled={loading}><Search size={16} /> Показать операции</button>
        </form>

        <section className="history-results" aria-label="Список операций" aria-busy={loading || loadingMore}>
          <div className="history-results-heading"><h2>Все операции</h2><span>{loading ? 'Загружаем…' : `${items.length} в списке`}</span></div>
          {error && <div className="history-error" role="alert"><CircleHelp size={19} /><div><b>Не удалось загрузить операции</b><p>{error}</p></div><button type="button" onClick={() => loadFirstPage(activeFilters)}><RefreshCw size={15} /> Повторить</button></div>}
          {loading ? <div className="history-skeleton-list" aria-label="Загружаем историю">{[0, 1, 2].map((item) => <div className="history-skeleton-row" key={item}><i /><span /><b /></div>)}</div> : !error && items.length === 0 ? (
            <div className="history-empty"><span><History size={21} /></span><b>Операций пока нет</b><p>Когда появятся переводы, поступления или списания, они отобразятся здесь.</p>{Object.values(activeFilters).some(Boolean) && <button type="button" onClick={resetFilters}>Сбросить фильтры</button>}</div>
          ) : !error && groups.map((group) => (
            <div className="history-day-group" key={group.title}>
              <h3>{group.title}</h3>
              <div className="history-operation-list">{group.items.map((item) => <OperationRow operation={item} key={item.id} />)}</div>
            </div>
          ))}
          {hasMore && !loading && !error && <button className="history-load-more" type="button" onClick={loadMore} disabled={loadingMore}>{loadingMore ? <><RefreshCw className="history-spin" size={16} /> Загружаем…</> : <>Загрузить ещё <ArrowRight size={16} /></>}</button>}
          {loadMoreError && !loading && <div className="history-more-error" role="alert"><span>{loadMoreError}</span><button type="button" onClick={loadMore}>Повторить загрузку</button></div>}
        </section>
      </main>
    </PageShell>
  )
}

function OperationRow({ operation }) {
  const currency = operation.currencyCode || 'RUB'
  const amountClass = operation.kind === 'income' ? 'income' : operation.kind === 'expense' ? 'expense' : 'transfer'
  const amountLabel = formatOperationAmount(operation.amount, operation.kind, currency)
  const route = [operation.fromAccountName, operation.toAccountName].filter(Boolean).join(' → ')
  const categoryName = operation.categoryName || operation.category?.name
  const subtitle = [route || operation.accountName, categoryName, operation.note, formatDateTime(operation.createdAt)]
    .filter(Boolean).join(' · ')

  return (
    <a className="history-operation-row" href={`/history/${encodeURIComponent(operation.id)}`} aria-label={`Открыть: ${operationLabel(operation.kind)}, ${amountLabel}, ${subtitle}`}>
      <span className={`history-operation-icon ${amountClass}`}><ArrowUpRight size={18} /></span>
      <span className="history-operation-copy"><b>{categoryName || operationLabel(operation.kind)}</b><small>{subtitle}</small></span>
      <span className={`history-operation-amount ${amountClass}`}>{amountLabel}</span>
      <ArrowRight className="history-operation-open" size={16} />
    </a>
  )
}

function OperationDetails({ transactionId }) {
  const [operation, setOperation] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const loadOperation = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await getTransaction(transactionId)
      if (!result || typeof result !== 'object') throw new Error('Сервер вернул пустой ответ.')
      setOperation(result)
    } catch (loadError) {
      setOperation(null)
      setError(loadError)
    } finally {
      setLoading(false)
    }
  }, [transactionId])

  useEffect(() => { void loadOperation() }, [loadOperation])

  const currency = operation?.currencyCode || 'RUB'
  const amountClass = operation?.kind === 'income' ? 'income' : operation?.kind === 'expense' ? 'expense' : 'transfer'
  const amountLabel = operation ? formatOperationAmount(operation.amount, operation.kind, currency) : ''

  return (
    <PageShell>
      <main className="history-content history-detail-content">
        <a className="history-detail-back" href="/history"><ArrowLeft size={17} /> К истории операций</a>
        <div className="history-eyebrow"><History size={15} /> ДЕТАЛИ ОПЕРАЦИИ</div>
        <h1>{operation ? operationLabel(operation.kind) : 'Детали операции'}</h1>
        {loading ? <div className="history-detail-card history-detail-loading" aria-busy="true"><i /><span /><span /><span /><span /></div>
          : error ? <section className="history-detail-card history-detail-error" role="alert">
            <CircleHelp size={23} />
            <h2>{error.status === 404 ? 'Операция недоступна' : 'Не удалось загрузить операцию'}</h2>
            <p>{error.status === 404 ? 'Операция не найдена или у вас нет к ней доступа.' : error.message || 'Проверьте соединение и попробуйте ещё раз.'}</p>
            <div className="history-detail-actions">{error.status !== 404 && <button type="button" onClick={loadOperation}><RefreshCw size={15} /> Повторить</button>}<a href="/history">К истории операций</a></div>
          </section>
            : <section className="history-detail-card">
              <div className={`history-detail-kind ${amountClass}`}>{operationLabel(operation.kind)}</div>
              <div className={`history-detail-amount ${amountClass}`}>{amountLabel}</div>
              <p className="history-detail-date">{formatDateTime(operation.createdAt)}</p>
              <dl className="history-detail-fields">
                <div><dt>Счёт отправителя</dt><dd>{operation.fromAccountName || '—'}</dd></div>
                <div><dt>Счёт получателя</dt><dd>{operation.toAccountName || '—'}</dd></div>
                <div><dt>Категория</dt><dd>{operation.categoryName || operation.category?.name || 'Без категории'}</dd></div>
                <div><dt>Валюта</dt><dd>{currency}</dd></div>
                <div><dt>Номер операции</dt><dd>{operation.id}</dd></div>
                {operation.note && <div><dt>Комментарий</dt><dd>{operation.note}</dd></div>}
              </dl>
            </section>}
      </main>
    </PageShell>
  )
}
