import axios from 'axios'

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
      if (!url.includes('/api/auth/login') && !url.includes('/api/auth/me')) {
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
