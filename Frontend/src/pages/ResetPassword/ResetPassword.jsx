import { Navigate } from 'react-router-dom'
import { Lock } from 'lucide-react'
import useResetPassword from './useResetPassword'
import PasswordValidation from '../../components/PasswordValidation'

export default function ResetPassword() {
  const {
    email,
    password, setPassword,
    confirmPassword, setConfirmPassword,
    error,
    submitting,
    allRulesPassed,
    passwordsMatch,
    user, loading,
    handleSubmit,
  } = useResetPassword()

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
            <h1 className="h4 fw-bold d-flex align-items-center gap-2"><Lock />Reset Password</h1>
            <p className="text-body-secondary mb-0">Enter your new password</p>
          </div>
          {error && <div className="alert alert-danger py-2">{error}</div>}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Email</label>
              <input type="email" className="form-control" value={email} readOnly />
            </div>
            <div className="mb-3">
              <label className="form-label">New Password</label>
              <input type="password" className="form-control" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
              <PasswordValidation password={password} />
            </div>
            <div className="mb-3">
              <label className="form-label">Confirm Password</label>
              <input type="password" className="form-control" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required minLength={8} />
              {confirmPassword && (
                <p className={`small mt-1 ${passwordsMatch ? 'text-success' : 'text-danger'}`}>
                  {passwordsMatch ? '✓ Passwords match' : '✗ Passwords do not match'}
                </p>
              )}
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={submitting || !allRulesPassed || !passwordsMatch}>
              {submitting ? 'Resetting...' : !allRulesPassed ? 'Complete password requirements' : !passwordsMatch ? 'Passwords must match' : 'Reset Password'}
            </button>
          </form>
          <div className="text-center mt-3">
            <span className="text-body-secondary small">
              <a href="/login" className="text-decoration-none">Back to Sign In</a>
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}
