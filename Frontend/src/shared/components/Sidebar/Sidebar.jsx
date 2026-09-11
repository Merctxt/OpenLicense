import { useEffect, useRef, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import {
  Activity,
  Code2,
  LogOut,
  Menu,
  Moon,
  Package,
  Star,
  Sun,
  User,
  X,
  Zap,
} from 'lucide-react'
import { logout } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'
import { useTheme } from '../../context/ThemeContext'
import Background from '../Background/Background'

const sourceUrl = import.meta.env.VITE_SOURCE_URL

const sidebarLinks = [
  { to: '/products', label: 'Products', icon: Package },
  { to: '/licenses', label: 'Licenses', icon: Star },
  { href: import.meta.env.VITE_STATUS_URL, label: 'Status', icon: Activity, external: true },
  { href: sourceUrl, label: 'Docs', icon: Code2, external: true },
  { href: `${import.meta.env.VITE_API_URL}/scalar/v1`, label: 'API', icon: Zap, external: true },
]

function MenuItem({ link, isActive, onNavigate }) {
  const Icon = link.icon

  const content = (
    <>
      <Icon className="me-2" size={16} />
      <span>{link.label}</span>
    </>
  )

  const className = `nav-link ${isActive ? 'active link-light' : 'link-body-emphasis'}`

  if (link.external) {
    return (
      <a
        key={link.label}
        href={link.href}
        target="_blank"
        rel="noopener noreferrer"
        className={className}
        aria-current={isActive ? 'page' : undefined}
      >
        {content}
      </a>
    )
  }

  return (
    <Link
      key={link.to}
      to={link.to}
      onClick={onNavigate}
      className={className}
      aria-current={isActive ? 'page' : undefined}
    >
      {content}
    </Link>
  )
}

function UserMenu({ user, onLogout }) {
  const { theme, setTheme } = useTheme()
  const [open, setOpen] = useState(false)
  const buttonRef = useRef(null)
  const menuRef = useRef(null)

  useEffect(() => {
    if (!open) return

    const handlePointerDown = (event) => {
      const clickedOutsideButton = buttonRef.current && !buttonRef.current.contains(event.target)
      const clickedOutsideMenu = menuRef.current && !menuRef.current.contains(event.target)

      if (clickedOutsideButton && clickedOutsideMenu) {
        setOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)
    return () => document.removeEventListener('mousedown', handlePointerDown)
  }, [open])

  return (
    <div className="dropdown" style={{ position: 'relative' }}>
      <button
        ref={buttonRef}
        type="button"
        className="dropdown-toggle link-body-emphasis d-flex align-items-center text-decoration-none w-100 border-0 bg-transparent px-0 text-start"
        data-bs-toggle="dropdown"
        aria-expanded={open}
        onClick={() => setOpen((previous) => !previous)}
        style={{
          fontWeight: 600,
          padding: '0.5rem 0',
          outline: 'none',
        }}
      >
        <div className="d-flex align-items-center justify-content-center rounded-circle bg-body-tertiary me-2" style={{ width: '32px', height: '32px' }}>
          <User size={18} />
        </div>
        <strong>{user.name || user.email || 'User'}</strong>
      </button>

      {open && (
        <div
          ref={menuRef}
          className="dropdown-menu show shadow"
          style={{
            left: '0',
            bottom: '100%',
            marginBottom: '0.5rem',
            minWidth: '220px',
            zIndex: 1050,
          }}
        >
          <div className="dropdown-item-text text-body-secondary small" style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {user.email}
          </div>
          <div className="dropdown-item" style={{ cursor: 'default' }}>
            <span className="me-2">Theme</span>
            <div className="d-flex gap-1 ms-4">
              <button
                className={`btn btn-sm ${theme === 'light' ? 'btn-primary' : 'btn-outline-secondary'}`}
                style={{ padding: '0.15rem 0.5rem', fontSize: '0.7rem' }}
                onClick={() => { setTheme('light'); setOpen(false) }}
              >
                <Sun size={12} className="me-1" />
                Light
              </button>
              <button
                className={`btn btn-sm ${theme === 'dark' ? 'btn-primary' : 'btn-outline-secondary'}`}
                style={{ padding: '0.15rem 0.5rem', fontSize: '0.7rem' }}
                onClick={() => { setTheme('dark'); setOpen(false) }}
              >
                <Moon size={12} className="me-1" />
                Dark
              </button>
              <button
                className={`btn btn-sm ${theme === 'system' ? 'btn-primary' : 'btn-outline-secondary'}`}
                style={{ padding: '0.15rem 0.5rem', fontSize: '0.7rem' }}
                onClick={() => { setTheme('system'); setOpen(false) }}
              >
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="me-1"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>
                Auto
              </button>
            </div>
          </div>
          <Link className="dropdown-item" to="/account" onClick={() => setOpen(false)}>
            Account
          </Link>
          <div className="dropdown-divider" />
          <button
            type="button"
            className="dropdown-item text-danger"
            onClick={async () => {
              setOpen(false)
              try {
                await logout()
              } catch {
                // ignore
              }
              onLogout()
            }}
          >
            Sign out
          </button>
        </div>
      )}
    </div>
  )
}

export default function Sidebar({ children }) {
  const { user, logout: authLogout } = useAuth()
  const location = useLocation()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [isMobile, setIsMobile] = useState(() => window.innerWidth < 768)

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768)
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  useEffect(() => {
    setMobileOpen(false)
  }, [location.pathname])

  const asideStyle = {
    position: isMobile ? 'fixed' : 'sticky',
    top: 0,
    left: 0,
    height: '100vh',
    width: isMobile ? '85%' : '20%',
    maxWidth: isMobile ? '300px' : '260px',
    minWidth: isMobile ? '220px' : '220px',
    zIndex: 1040,
    transform: isMobile && !mobileOpen ? 'translateX(-100%)' : 'none',
    transition: 'transform 0.2s ease',
    borderRight: '1px solid var(--bs-border-color)',
    backgroundColor: 'var(--bs-body-bg)',
    flexShrink: 0,
  }

  return (
    <Background>
      <div className="d-flex" style={{ minHeight: '100vh' }}>
        <aside className="d-flex flex-column" style={asideStyle}>
          <div className="d-flex align-items-center justify-content-between px-3 pt-3 pb-2">
            <Link to="/" className="link-body-emphasis d-flex align-items-center me-md-auto text-decoration-none">
              <img src="/favicon.svg" alt="OpenLicense logo" style={{ width: '24px', height: '24px', marginRight: '0.75rem' }} />
              <span className="fs-4">OpenLicense</span>
            </Link>

            {isMobile && (
              <button type="button" className="btn-close" aria-label="Close" onClick={() => setMobileOpen(false)} />
            )}
          </div>

          <div className="d-flex flex-column justify-content-between pt-0 px-3 flex-grow-1">
            <div>
              <hr className="mt-0" />
              <ul className="nav nav-pills flex-column mb-auto">
                {user &&
                  sidebarLinks.map((link) => (
                    <li key={link.label || link.to} className="nav-item">
                      <MenuItem
                        link={link}
                        isActive={link.external ? false : location.pathname === link.to}
                        onNavigate={() => setMobileOpen(false)}
                      />
                    </li>
                  ))}
              </ul>
            </div>

            <div className="pb-3">
              <hr />
              {user && <UserMenu user={user} onLogout={() => { authLogout(); window.location.href = '/login' }} />}
            </div>
          </div>
        </aside>

        {isMobile && mobileOpen && (
          <div className="position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-50" style={{ zIndex: 1035 }} onClick={() => setMobileOpen(false)} />
        )}

        <div className="flex-grow-1 d-flex flex-column" style={{ minWidth: 0 }}>
          {isMobile && (
            <div className="d-flex align-items-center justify-content-between p-3 border-bottom bg-body" style={{ height: '56px', minHeight: '56px' }}>
              <Link to="/" className="text-decoration-none text-body fw-bold d-flex align-items-center gap-2">
                <img src="/favicon.svg" alt="OpenLicense" style={{ height: '18px' }} />
                OpenLicense
              </Link>
              <button type="button" className="btn btn-link text-body p-0 border-0" onClick={() => setMobileOpen((previous) => !previous)} aria-label={mobileOpen ? 'Close menu' : 'Open menu'}>
                {mobileOpen ? <X size={22} /> : <Menu size={22} />}
              </button>
            </div>
          )}

          <main className="flex-grow-1 px-3 px-md-4 py-4" style={{ width: '100%', margin: '0 auto', maxWidth: '1100px' }}>
            {children}
          </main>
        </div>
      </div>
    </Background>
  )
}
