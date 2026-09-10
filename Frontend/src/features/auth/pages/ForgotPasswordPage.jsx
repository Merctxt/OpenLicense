import { Link } from 'react-router-dom'
import { KeyRound } from 'lucide-react'
import useForgotPassword from './useForgotPassword'
import Background from '../../../shared/components/Background/Background'
import { Alert } from '../../../shared/components/Alert'

export default function ForgotPassword() {
  const {
    email, setEmail,
    error, success,
    submitting,
    clearAlert,
    handleSubmit,
  } = useForgotPassword()

  return (
    <Background variant="auth">
      <div className="card shadow-sm" style={{ maxWidth: '400px', width: '100%' }}>
        <div className="card-body p-4">
          <div className="d-flex flex-column justify-content-center align-items-center mb-4">
            <h1 className="h4 fw-bold d-flex align-items-center gap-2"><KeyRound />Recover Password</h1>
            <p className="text-body-secondary mb-0">Enter your email to receive a recovery token</p>
          </div>
          {error && <Alert type="error" message={error} onDismiss={clearAlert} />}
          {success && <Alert type="success" message={success} onDismiss={clearAlert} />}
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
    </Background>
  )
}
