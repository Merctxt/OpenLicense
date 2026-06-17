import api from './client'

export function register(data) {
  return api.post('/api/auth/register', data)
}

export function login(data) {
  return api.post('/api/auth/login', data)
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

export function createApiKey(data) {
  return api.post('/api/auth/apikey', data)
}

export function deleteApiKey(data) {
  return api.delete('/api/auth/apikey', { data })
}

export function getProducts() {
  return api.get('/api/products/all')
}

export function createProduct(data) {
  return api.post('/api/products/create', data)
}

export function updateProduct(data) {
  return api.put('/api/products/update', data)
}

export function deleteProduct(data) {
  return api.delete('/api/products', { data })
}

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
