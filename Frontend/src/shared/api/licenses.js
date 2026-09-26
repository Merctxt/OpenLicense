import api from './client'
import { API_VERSION } from './version'

const apiPrefix = `/api/${API_VERSION}`

export function getLicenses(productId) {
  return api.get(`${apiPrefix}/licenses`, { params: { productId } })
}

export function createLicense(data) {
  return api.post(`${apiPrefix}/licenses`, data)
}

export function updateLicense(data) {
  return api.put(`${apiPrefix}/licenses`, data)
}

export function deleteLicense(data) {
  return api.delete(`${apiPrefix}/licenses`, { data })
}

export function getLicenseActivations(licenseId) {
  return api.get(`${apiPrefix}/licenses/activations`, { params: { licenseId } })
}

export function deactivateLicense(data) {
  return api.post(`${apiPrefix}/licenses/deactivate-by-jwt`, data)
}

export function toggleActivation(data) {
  return api.put(`${apiPrefix}/licenses/activations/toggle`, data)
}
