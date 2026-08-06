import { useState } from 'react'
import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [navOpen, setNavOpen] = useState(false)

  const handleLogout = () => {
    setNavOpen(false)
    logout()
    navigate('/login')
  }

  const closeNav = () => setNavOpen(false)

  return (
    <div className="d-flex flex-column flex-fill">
      <nav className="navbar navbar-expand-sm sticky-top" style={{ backgroundColor: 'var(--bs-body-bg)' }}>
        <div className="container" style={{ maxWidth: '1100px' }}>
          <Link to="/" className="navbar-brand fw-bold">OpenLicense</Link>
          {user && (
            <>
              <button
                className={`navbar-toggler ${navOpen ? '' : 'collapsed'}`}
                type="button"
                onClick={() => setNavOpen(prev => !prev)}
              >
                <span className="navbar-toggler-icon"></span>
              </button>
              <div className={`navbar-collapse ${navOpen ? 'show' : 'collapse'}`}>
                <ul className="navbar-nav ms-auto align-items-sm-center gap-1">
                  <li className="nav-item">
                    <Link to="/" className="nav-link" onClick={closeNav}>Products</Link>
                  </li>
                  <li className="nav-item">
                    <Link to="/account" className="nav-link" onClick={closeNav}>Account</Link>
                  </li>
                  <li className="nav-item">
                    <Link to="/docs" className="nav-link" onClick={closeNav}>Docs</Link>
                  </li>
                  <li className="nav-item ms-sm-2">
                    <button onClick={handleLogout} className="btn btn-outline-secondary btn-sm">Logout</button>
                  </li>
                </ul>
              </div>
            </>
          )}
        </div>
      </nav>
      <main className="container flex-fill py-4" style={{ maxWidth: '1100px' }}>
        <Outlet />
      </main>
    </div>
  )
}
