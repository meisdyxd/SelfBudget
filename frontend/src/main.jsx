import React from 'react'
import { createRoot } from 'react-dom/client'
import App from './ui/App.jsx'
import AuthScreen from './ui/AuthScreen.jsx'
import AccountDetails from './ui/accounts/AccountDetails.jsx'
import HistoryPage from './ui/history/HistoryPage.jsx'
import ProjectBoard from './ui/ProjectBoard.jsx'
import { getSession, monitorSessionExpiration } from './ui/session.js'
import './ui/styles.css'
import './ui/readability.css'

const route = window.location.pathname
const accountRoute = route.match(/^\/accounts\/([^/]+)\/?$/)
const historyRoute = route.match(/^\/history\/([^/]+)\/?$/)
const isPublicRoute = route === '/auth' || route === '/roadmap'
const hasSession = Boolean(getSession())
if (!isPublicRoute && !hasSession) window.location.replace('/auth?mode=login')
if (hasSession) monitorSessionExpiration()

const page = route === '/auth' || (!isPublicRoute && !hasSession)
  ? <AuthScreen />
  : route === '/roadmap'
    ? <ProjectBoard />
    : accountRoute
      ? <AccountDetails accountId={decodeURIComponent(accountRoute[1])} />
      : route === '/history'
        ? <HistoryPage />
        : historyRoute
          ? <HistoryPage transactionId={decodeURIComponent(historyRoute[1])} />
          : <App />

createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    {page}
  </React.StrictMode>,
)
