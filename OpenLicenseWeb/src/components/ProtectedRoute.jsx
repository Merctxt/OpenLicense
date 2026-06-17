import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function ProtectedRoute({ children }) {
  const { user, loading } = useAuth()

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '40px', color: 'var(--color-text-light)' }}>Loading...</div>
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  return children
}
