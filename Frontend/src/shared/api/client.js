import axios from 'axios'
import { API_VERSION } from './version'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '',
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
})

api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      const url = err.config?.url || ''
      if (!url.includes(`/api/${API_VERSION}/auth/login`) && !url.includes(`/api/${API_VERSION}/auth/me`)) {
        window.location.href = '/login'
      }
      if (url.includes('/api/auth/login')) {
        return Promise.reject(err)
      }
    }
    return Promise.reject(err)
  }
)

export default api
