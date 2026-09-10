import api from './client'

export function register(data) {
  return api.post('/api/auth/register', data)
}

export function login(data) {
  return api.post('/api/auth/login', data)
}

export function logout() {
  return api.post('/api/auth/logout')
}

export function getMe() {
  return api.get('/api/auth/me')
}

export function updateAccount(data) {
  return api.put('/api/auth', data)
}

export function deleteAccount() {
  return api.delete('/api/auth')
}

export function forgotPassword(data) {
  return api.post('/api/auth/forgot-password', data)
}

export function verifyToken(data) {
  return api.post('/api/auth/reset-password/verify', data)
}

export function resetPassword(data) {
  return api.post('/api/auth/reset-password', data)
}

export function createApiKey(data) {
  return api.post('/api/auth/apikey', data)
}

export function deleteApiKey(data) {
  return api.delete('/api/auth/apikey', { data })
}
