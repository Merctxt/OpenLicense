import api from './client'

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
