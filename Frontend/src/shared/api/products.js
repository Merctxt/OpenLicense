import api from './client'
import { API_VERSION } from './version'

const apiPrefix = `/api/${API_VERSION}`

export function getProducts() {
  return api.get(`${apiPrefix}/products/all`)
}

export function createProduct(data) {
  return api.post(`${apiPrefix}/products/create`, data)
}

export function updateProduct(data) {
  return api.put(`${apiPrefix}/products/update`, data)
}

export function deleteProduct(data) {
  return api.delete(`${apiPrefix}/products`, { data })
}
