export function inputDate(value: string): string {
  return value.split('T')[0] ?? value
}

export function todayInputDate(): string {
  const now = new Date()
  const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60_000)

  return localDate.toISOString().slice(0, 10)
}

export function displayDate(value: string): string {
  const date = inputDate(value)
  const [year, month, day] = date.split('-')

  if (!year || !month || !day) {
    return value
  }

  return `${month}/${day}/${year}`
}

export function displayAmount(value: number): string {
  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}
