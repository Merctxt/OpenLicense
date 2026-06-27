import { Link, Navigate } from 'react-router-dom'
import useRegister from './useRegister'
import '../Auth.css'

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
          <p>Create your account</p>
        </div>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Name</label>
            <input type="text" value={name} onChange={(e) => setName(e.target.value)} required />
          </div>
          <div className="form-group">
            <label>Email</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
          </div>
          <button type="submit" className="btn btn-primary auth-btn" disabled={submitting}>
            {submitting ? 'Creating account...' : 'Create Account'}
          </button>
        </form>
        <div className="auth-footer">
          Already have an account? <Link to="/login">Sign in</Link>
        </div>
      </div>
    </div>
  )
}
