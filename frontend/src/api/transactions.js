import { apiRequest } from './client.js'

function makeQuery(filters = {}, cursor = null, limit = 20) {
  const params = new URLSearchParams()
  if (filters.accountId) params.set('accountId', filters.accountId)
  if (filters.kind) params.set('kind', filters.kind)
  if (filters.categoryId) params.set('categoryId', filters.categoryId)
  if (filters.fromInclusive) params.set('fromInclusive', filters.fromInclusive)
  if (filters.toExclusive) params.set('toExclusive', filters.toExclusive)
  if (cursor) params.set('cursor', cursor)
  params.set('limit', String(limit))
  return params.toString()
}

export function getTransactions(filters, cursor, limit = 20) {
  const query = makeQuery(filters, cursor, limit)
  return apiRequest(`/api/Transactions?${query}`)
}

export function getTransaction(transactionId) {
  return apiRequest(`/api/Transactions/${encodeURIComponent(transactionId)}`)
}

export function getTransactionCategories() {
  return apiRequest('/api/Transactions/categories')
}
