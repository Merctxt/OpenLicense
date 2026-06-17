import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="app-layout">
      <header className="app-header">
        <div className="header-inner">
          <Link to="/" className="header-brand">OpenLicense</Link>
          {user && (
            <nav className="header-nav">
              <Link to="/">Products</Link>
              <Link to="/account">Account</Link>
              <Link to="/docs">Docs</Link>
              <button onClick={handleLogout} className="btn btn-sm btn-default">Logout</button>
            </nav>
          )}
        </div>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
    </div>
  )
}
