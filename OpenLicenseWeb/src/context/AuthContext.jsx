import { createContext, useContext, useState, useEffect, useCallback } from 'react'
import { getMe } from '../api/endpoints'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  const loadUser = useCallback(async () => {
    const token = localStorage.getItem('ol_token')
    if (!token) {
      setUser(null)
      setLoading(false)
      return
    }
    try {
      const res = await getMe()
      setUser(res.data)
    } catch {
      localStorage.removeItem('ol_token')
      setUser(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadUser()
  }, [loadUser])

  const saveToken = (token) => {
    localStorage.setItem('ol_token', token)
  }

  const logout = () => {
    localStorage.removeItem('ol_token')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, setUser, loading, logout, saveToken, loadUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
