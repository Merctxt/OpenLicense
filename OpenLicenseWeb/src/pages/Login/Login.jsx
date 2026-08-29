import { Link, Navigate } from 'react-router-dom'
import { UserPlus, Layers } from 'lucide-react'
import useLogin from './useLogin'

export default function Login() {
  const {
    email, setEmail,
    password, setPassword,
    error, success,
    submitting,
    user, loading,
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
    <div className="d-flex align-items-center justify-content-center min-vh-100 bg-body-tertiary">
      <div className="card shadow-sm" style={{ maxWidth: '400px', width: '100%' }}>
        <div className="card-body p-4">
          <div className="d-flex flex-column justify-content-center align-items-center mb-4">
            <h1 className="h4 fw-bold d-flex align-items-center gap-2"><Layers />OpenLicense</h1>
            <p className="text-body-secondary mb-0">Sign in to your account</p>
          </div>
          {success && <div className="alert alert-success py-2">{success}</div>}
          {error && <div className="alert alert-danger py-2">{error}</div>}
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
          </form>
          <div className="text-center mt-3">
            <span className="text-body-secondary small">Don't have an account? <Link to="/register" className="text-decoration-none"><UserPlus className="d-inline me-1" width={14} height={14} />Create one</Link></span>
          </div>
        </div>
      </div>
    </div>
  )
}
