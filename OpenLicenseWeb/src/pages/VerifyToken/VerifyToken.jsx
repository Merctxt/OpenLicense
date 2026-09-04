import { useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { CheckCircle, AlertTriangle } from 'lucide-react'
import useVerifyToken from './useVerifyToken'

export default function VerifyToken() {
  const {
    email,
    token, setToken,
    error, success,
    submitting,
    handleVerify,
  } = useVerifyToken()
  const navigate = useNavigate()
  const location = useLocation()

  useEffect(() => {
    if (!location.state?.email) {
      navigate('/forgot-password')
    }
  }, [location.state?.email, navigate])

  return (
    <div className="d-flex align-items-center justify-content-center min-vh-100 bg-body-tertiary">
      <div className="card shadow-sm" style={{ maxWidth: '400px', width: '100%' }}>
        <div className="card-body p-4">
          <div className="d-flex flex-column justify-content-center align-items-center mb-4">
            <h1 className="h4 fw-bold d-flex align-items-center gap-2"><CheckCircle />Verify Token</h1>
            <p className="text-body-secondary mb-0">Enter the recovery token sent to your email</p>
          </div>
          <div className="mb-3">
            <label className="form-label">Email</label>
            <input type="email" className="form-control" value={email} readOnly />
          </div>
          {error && <div className="alert alert-danger py-2">{error}</div>}
          {success && <div className="alert alert-success py-2">{success}</div>}
          <form onSubmit={handleVerify}>
            <div className="mb-3">
              <label className="form-label">Recovery Token</label>
              <input type="text" className="form-control text-center" style={{ letterSpacing: '5px', fontSize: '1.2rem' }} value={token} onChange={(e) => setToken(e.target.value)} required placeholder="Enter token" />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'Verifying...' : 'Verify Token'}
            </button>
          </form>
          <div className="text-center mt-3">
            <span className="text-body-secondary small">
              <a href="/forgot-password" className="text-decoration-none">Resend Token</a>
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}
