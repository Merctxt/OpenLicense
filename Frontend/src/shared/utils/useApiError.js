import { MESSAGES } from './messages.js'

export { MESSAGES }

function getMessage(key, fallback) {
  const msg = key instanceof Function ? key() : MESSAGES[key]
  if (msg === null) return fallback
  return msg || fallback
}

function normalizeErrorMessage(err) {
  if (err?.response?.data?.message) return err.response.data.message
  if (err?.response?.data?.error?.join) return err.response.data.error.join(' ')
  return null
}

export function handleError(err, key, fallback) {
  const apiMsg = normalizeErrorMessage(err)
  return apiMsg || getMessage(key, fallback)
}
