import api from './client'

export function getLicenses(productId) {
  return api.get('/api/licenses', { params: { productId } })
}

export function createLicense(data) {
  return api.post('/api/licenses', data)
}

export function updateLicense(data) {
  return api.put('/api/licenses', data)
}

export function deleteLicense(data) {
  return api.delete('/api/licenses', { data })
}

export function getLicenseActivations(licenseId) {
  return api.get('/api/licenses/activations', { params: { licenseId } })
}

export function deactivateLicense(data) {
  return api.post('/api/licenses/deactivate-by-jwt', data)
}
