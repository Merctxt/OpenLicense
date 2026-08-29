import { Link, Navigate } from 'react-router-dom'
import { LogIn, Layers } from 'lucide-react'
import useRegister from './useRegister'

export default function Register() {
  const {
    name, setName,
    email, setEmail,
    password, setPassword,
    error, submitting,
    user, loading,
    handleSubmit,
  } = useRegister()

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
            <p className="text-body-secondary mb-0">Create your account</p>
          </div>
          {error && <div className="alert alert-danger py-2">{error}</div>}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input type="text" className="form-control" value={name} onChange={(e) => setName(e.target.value)} required />
            </div>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input type="email" className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            <div className="mb-3">
              <label className="form-label">Password</label>
              <input type="password" className="form-control" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'Creating account...' : 'Create Account'}
            </button>
          </form>
          <div className="text-center mt-3">
            <span className="text-body-secondary small">Already have an account? <Link to="/login" className="text-decoration-none"><LogIn className="d-inline me-1" width={14} height={14} />Sign in</Link></span>
          </div>
        </div>
      </div>
    </div>
  )
}
