import api from './client'

export function getProducts() {
  return api.get('/api/v1/products/all')
}

export function createProduct(data) {
  return api.post('/api/v1/products/create', data)
}

export function updateProduct(data) {
  return api.put('/api/v1/products/update', data)
}

export function deleteProduct(data) {
  return api.delete('/api/v1/products', { data })
}
