import { Link, Navigate } from 'react-router-dom'
import { UserPlus } from 'lucide-react'
import useLogin from './useLogin'
import Background from '../../../../shared/components/Background/Background'
import { Alert } from '../../../../shared/components/Alert'

const registrationEnabled = import.meta.env.VITE_REGISTRATION_ENABLED !== 'false'

export default function Login() {
  const {
    email, setEmail,
    password, setPassword,
    error, success,
    submitting,
    user, loading,
    clearAlert,
    handleSubmit,
  } = useLogin()

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="spinner-border text-secondary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    )
  }

  if (user) {
    return <Navigate to="/" replace />
  }

  return (
    <Background variant="auth">
      <div className="card shadow-sm" style={{ maxWidth: '400px', width: '100%' }}>
        <div className="card-body p-4">
          <div className="d-flex flex-column justify-content-center align-items-center mb-4">
            <h1 className="h4 fw-bold d-flex align-items-center gap-2"><img src="/favicon.svg" alt="" style={{ height: '24px' }} />OpenLicense</h1>
            <p className="text-body-secondary mb-0">Sign in to your account</p>
          </div>
          {success && <Alert type="success" message={success} onDismiss={clearAlert} />}
          {error && <Alert type="error" message={error} onDismiss={clearAlert} />}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input type="email" className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            <div className="mb-3">
              <label className="form-label">Password</label>
              <input type="password" className="form-control" value={password} onChange={(e) => setPassword(e.target.value)} required />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'Signing in...' : 'Sign In'}
            </button>
            <div className="text-center mt-3">
              <a href="/forgot-password" className="text-decoration-none small text-body-secondary">Forgot your password?</a>
            </div>
          </form>
          {registrationEnabled && <div className="text-center mt-3">
            <span className="text-body-secondary small">Don't have an account? <Link to="/register" className="text-decoration-none"><UserPlus className="d-inline me-1" width={14} height={14} />Create one</Link></span>
          </div>}
        </div>
      </div>
    </Background>
  )
}
