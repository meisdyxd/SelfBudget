import { ArrowRight, ArrowUpRight, CreditCard, Sparkles, Wallet } from 'lucide-react'
import { formatMoney } from '../../domain/money.js'
import './account-card.css'

const CARD_STYLES = ['lavender', 'sage', 'sand']

export default function AccountCard({ account, index = 0, hidden = false, apiReady = true, canTransfer = true, onOpen, onTransfer }) {
  const icon = index % 3 === 0
    ? <CreditCard size={18} />
    : index % 3 === 1
      ? <Wallet size={18} />
      : <Sparkles size={18} />
  const accountName = account.name || 'Без названия'

  return (
    <article className={`account-card ${CARD_STYLES[index % CARD_STYLES.length]}`}>
      <div className="account-card-top">
        <span className="account-icon" aria-hidden="true">{icon}</span>
        <span className="account-type">{account.type || 'Счёт'}</span>
        <button
          className="more-button account-open-button"
          type="button"
          aria-label={`Открыть счёт «${accountName}»`}
          title="Подробности счёта"
          onClick={onOpen}
        >
          <ArrowUpRight size={17} />
        </button>
      </div>
      <div className="account-name" title={accountName}>{accountName}</div>
      <div className="account-balance" title={hidden ? undefined : formatMoney(account.balance, account.currencyCode)}>
        {hidden ? '••••••' : formatMoney(account.balance, account.currencyCode)}
      </div>
      <div className="account-card-bottom">
        <span className="account-currency">{account.currencyCode || 'RUB'}</span>
        <button className="account-transfer" type="button" onClick={onTransfer} disabled={!apiReady || !canTransfer} title={!apiReady ? 'Нет связи с сервером счетов' : canTransfer ? 'Перевести с этого счёта' : 'Нужен другой счёт в той же валюте'}>
          Перевести <ArrowRight size={14} />
        </button>
      </div>
      <span className="account-decoration" aria-hidden="true" />
    </article>
  )
}
