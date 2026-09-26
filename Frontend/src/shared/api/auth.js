import api from './client'

export function register(data) {
  return api.post('/api/v1/auth/register', data)
}

export function login(data) {
  return api.post('/api/v1/auth/login', data)
}

export function logout() {
  return api.post('/api/v1/auth/logout')
}

export function getMe() {
  return api.get('/api/v1/auth/me')
}

export function updateAccount(data) {
  return api.put('/api/v1/auth', data)
}

export function deleteAccount() {
  return api.delete('/api/v1/auth')
}

export function forgotPassword(data) {
  return api.post('/api/v1/auth/forgot-password', data)
}

export function verifyToken(data) {
  return api.post('/api/v1/auth/reset-password/verify', data)
}

export function resetPassword(data) {
  return api.post('/api/v1/auth/reset-password', data)
}

export function createApiKey(data) {
  return api.post('/api/v1/auth/apikey', data)
}

export function deleteApiKey(data) {
  return api.delete('/api/v1/auth/apikey', { data })
}

export function toggleApiKey(data) {
  return api.put('/api/v1/auth/apikey/toggle', data)
}
