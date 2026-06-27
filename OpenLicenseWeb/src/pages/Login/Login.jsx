import { Link, Navigate } from 'react-router-dom'
import useLogin from './useLogin'
import '../Auth.css'

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
    return <div style={{ textAlign: 'center', padding: '40px', color: 'var(--color-text-light)' }}>Loading...</div>
  }

  if (user) {
    return <Navigate to="/" replace />
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-header">
          <h1>OpenLicense</h1>
          <p>Sign in to your account</p>
        </div>
        {success && <div className="alert alert-success">{success}</div>}
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </div>
          <button type="submit" className="btn btn-primary auth-btn" disabled={submitting}>
            {submitting ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
        <div className="auth-footer">
          Don't have an account? <Link to="/register">Create one</Link>
        </div>
      </div>
    </div>
  )
}
