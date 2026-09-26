import api from './client'

export function getLicenses(productId) {
  return api.get('/api/v1/licenses', { params: { productId } })
}

export function createLicense(data) {
  return api.post('/api/v1/licenses', data)
}

export function updateLicense(data) {
  return api.put('/api/v1/licenses', data)
}

export function deleteLicense(data) {
  return api.delete('/api/v1/licenses', { data })
}

export function getLicenseActivations(licenseId) {
  return api.get('/api/v1/licenses/activations', { params: { licenseId } })
}

export function deactivateLicense(data) {
  return api.post('/api/v1/licenses/deactivate-by-jwt', data)
}

export function toggleActivation(data) {
  return api.put('/api/v1/licenses/activations/toggle', data)
}
