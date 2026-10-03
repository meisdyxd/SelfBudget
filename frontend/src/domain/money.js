function decimalToMinorUnits(amount, fractionDigits) {
  if (amount === null || amount === undefined) return null
  const raw = String(amount).trim()
  if (raw.length > 128) return null
  const match = raw.match(/^([+-]?)(\d+)(?:\.(\d*))?(?:e([+-]?\d+))?$/i)
  if (!match) return null

  const [, sign, integer, fraction = '', exponentText = '0'] = match
  const exponent = Number(exponentText)
  if (!Number.isSafeInteger(exponent) || Math.abs(exponent) > 30) return null

  const digits = `${integer}${fraction}`
  const retainedLength = integer.length + exponent + fractionDigits
  let minorDigits
  if (retainedLength <= 0) {
    if (/[1-9]/.test(digits)) return null
    minorDigits = '0'
  } else if (retainedLength < digits.length) {
    if (/[1-9]/.test(digits.slice(retainedLength))) return null
    minorDigits = digits.slice(0, retainedLength) || '0'
  } else {
    minorDigits = digits + '0'.repeat(retainedLength - digits.length)
  }

  const value = BigInt(minorDigits || '0')
  return sign === '-' ? -value : value
}

export function currencyFractionDigits(currency) {
  try {
    return new Intl.NumberFormat('ru-RU', { style: 'currency', currency }).resolvedOptions().maximumFractionDigits
  } catch {
    return null
  }
}

export function parsePositiveMoneyAmount(amount, currency = 'RUB') {
  const normalizedCurrency = currency || 'RUB'
  const fractionDigits = currencyFractionDigits(normalizedCurrency)
  if (fractionDigits === null) return null

  const raw = String(amount ?? '').trim().replace(',', '.')
  if (!raw || raw.length > 128) return null
  const match = raw.match(/^(\d+)(?:\.(\d*))?$/)
  if (!match) return null

  const [, wholeText, fractionText = ''] = match
  if (fractionText.length > fractionDigits) return null

  const whole = BigInt(wholeText)
  const factor = 10n ** BigInt(fractionDigits)
  const fraction = fractionText.padEnd(fractionDigits, '0')
  const minorUnits = whole * factor + BigInt(fraction || '0')
  if (minorUnits <= 0n) return null

  const normalizedWhole = whole.toString()
  return {
    decimal: fractionDigits === 0 ? normalizedWhole : `${normalizedWhole}.${fraction}`,
    minorUnits,
    fractionDigits,
  }
}

function formatMinorUnits(minorUnits, currency, fractionDigits) {
  const factor = 10n ** BigInt(fractionDigits)
  const absolute = minorUnits < 0n ? -minorUnits : minorUnits
  const whole = absolute / factor
  const fraction = (absolute % factor).toString().padStart(fractionDigits, '0')
  const formattedWhole = minorUnits < 0n ? (whole === 0n ? -0 : -whole) : whole
  const formatter = new Intl.NumberFormat('ru-RU', {
    style: 'currency', currency, minimumFractionDigits: fractionDigits, maximumFractionDigits: fractionDigits,
  })

  return formatter.formatToParts(formattedWhole)
    .map((part) => part.type === 'fraction' ? fraction : part.value)
    .join('')
}

export function formatMoney(amount, currency = 'RUB') {
  const normalizedCurrency = currency || 'RUB'
  const digits = currencyFractionDigits(normalizedCurrency)
  if (digits === null) return '—'
  const minorUnits = decimalToMinorUnits(amount, digits)
  return minorUnits === null ? '—' : formatMinorUnits(minorUnits, normalizedCurrency, digits)
}

export function getCurrencyTotals(accounts) {
  const totals = new Map()
  for (const account of accounts) {
    const currency = account.currencyCode || 'RUB'
    let total = totals.get(currency)
    if (!total) {
      const fractionDigits = currencyFractionDigits(currency)
      total = { currency, fractionDigits, minorUnits: 0n, valid: fractionDigits !== null }
      totals.set(currency, total)
    }

    const minorUnits = total.valid ? decimalToMinorUnits(account.balance, total.fractionDigits) : null
    if (minorUnits === null) total.valid = false
    else if (total.valid) total.minorUnits += minorUnits
  }

  return [...totals.values()].map((total) => ({
    currency: total.currency,
    label: total.valid ? formatMinorUnits(total.minorUnits, total.currency, total.fractionDigits) : '—',
  }))
}
