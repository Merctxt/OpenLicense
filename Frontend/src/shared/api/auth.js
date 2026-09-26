import api from './client'
import { API_VERSION } from './version'

const apiPrefix = `/api/${API_VERSION}`

export function register(data) {
  return api.post(`${apiPrefix}/auth/register`, data)
}

export function login(data) {
  return api.post(`${apiPrefix}/auth/login`, data)
}

export function logout() {
  return api.post(`${apiPrefix}/auth/logout`)
}

export function getMe() {
  return api.get(`${apiPrefix}/auth/me`)
}

export function updateAccount(data) {
  return api.put(`${apiPrefix}/auth`, data)
}

export function deleteAccount() {
  return api.delete(`${apiPrefix}/auth`)
}

export function forgotPassword(data) {
  return api.post(`${apiPrefix}/auth/forgot-password`, data)
}

export function verifyToken(data) {
  return api.post(`${apiPrefix}/auth/reset-password/verify`, data)
}

export function resetPassword(data) {
  return api.post(`${apiPrefix}/auth/reset-password`, data)
}

export function createApiKey(data) {
  return api.post(`${apiPrefix}/auth/apikey`, data)
}

export function deleteApiKey(data) {
  return api.delete(`${apiPrefix}/auth/apikey`, { data })
}

export function toggleApiKey(data) {
  return api.put(`${apiPrefix}/auth/apikey/toggle`, data)
}
