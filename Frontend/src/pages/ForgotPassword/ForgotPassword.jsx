import { Link } from 'react-router-dom'
import { KeyRound } from 'lucide-react'
import useForgotPassword from './useForgotPassword'

export default function ForgotPassword() {
  const {
    email, setEmail,
    error, success,
    submitting,
    handleSubmit,
  } = useForgotPassword()

  return (
    <div className="d-flex align-items-center justify-content-center min-vh-100 bg-body-tertiary">
      <div className="card shadow-sm" style={{ maxWidth: '400px', width: '100%' }}>
        <div className="card-body p-4">
          <div className="d-flex flex-column justify-content-center align-items-center mb-4">
            <h1 className="h4 fw-bold d-flex align-items-center gap-2"><KeyRound />Recover Password</h1>
            <p className="text-body-secondary mb-0">Enter your email to receive a recovery token</p>
          </div>
          {error && <div className="alert alert-danger py-2">{error}</div>}
          {success && <div className="alert alert-success py-2">{success}</div>}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input type="email" className="form-control" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting ? 'Sending...' : 'Send Recovery Token'}
            </button>
          </form>
          <div className="text-center mt-3">
            <span className="text-body-secondary small">
              <Link to="/login" className="text-decoration-none">Back to Sign In</Link>
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}
