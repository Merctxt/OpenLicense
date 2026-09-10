import { useEffect, useRef, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import {
  Activity,
  ChevronLeft,
  ChevronRight,
  Code2,
  Layers,
  LogOut,
  Menu,
  Package,
  Scale,
  Sparkles,
  Star,
  User,
  X,
  Zap,
} from 'lucide-react'
import { logout as apiLogout } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'
import Background from '../Background/Background'

const sidebarLinks = [
  { to: '/products', label: 'Products', icon: Package },
  { to: '/licenses', label: 'Licenses', icon: Star },
  { to: '/activations', label: 'Activations', icon: Activity },
  { to: '/account', label: 'Account', icon: User },
  { to: '/metrics', label: 'Metrics', icon: Sparkles },
  { to: '/terms', label: 'Terms', icon: Scale },
  { href: import.meta.env.VITE_STATUS_URL, label: 'Status', icon: Activity, external: true },
  { href: import.meta.env.VITE_SOURCE_URL, label: 'Docs', icon: Code2, external: true },
  { href: `${import.meta.env.VITE_API_URL}/scalar/v1`, label: 'API', icon: Zap, external: true },
]

function SidebarItem({ link, collapsed, active, onNavigate }) {
  const Icon = link.icon
  const content = (
    <>
      <Icon size={18} className="flex-shrink-0" />
      {!collapsed && <span className="text-truncate">{link.label}</span>}
    </>
  )

  if (link.external) {
    return (
      <a
        key={link.label}
        href={link.href}
        target="_blank"
        rel="noopener noreferrer"
        className={`d-flex align-items-center gap-2 px-3 py-2 rounded-2 text-decoration-none mb-1 ${
          active ? 'text-bg-primary' : 'text-body-secondary'
        }`}
        style={{ fontSize: '0.875rem' }}
        title={collapsed ? link.label : undefined}
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
      className={`d-flex align-items-center gap-2 px-3 py-2 rounded-2 text-decoration-none mb-1 ${
        active ? 'text-bg-primary' : 'text-body-secondary'
      }`}
      style={{ fontSize: '0.875rem' }}
      title={collapsed ? link.label : undefined}
    >
      {content}
    </Link>
  )
}

function UserMenu({ user, collapsed, onLogout }) {
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
    <div style={{ position: 'relative' }}>
      <button
        ref={buttonRef}
        type="button"
        onClick={() => setOpen((previous) => !previous)}
        aria-expanded={open}
        className="d-flex align-items-center gap-2 w-100 text-decoration-none text-body p-3 border-top"
        style={{
          fontSize: '0.875rem',
          textAlign: 'left',
          backgroundColor: 'transparent',
          border: 'none',
          cursor: 'pointer',
        }}
      >
        <User size={18} className="flex-shrink-0" />
        {!collapsed && (
          <span className="text-truncate d-inline-block" style={{ maxWidth: '140px' }}>
            {user.name || user.email || 'Account'}
          </span>
        )}
        {!collapsed && (
          <ChevronRight
            size={14}
            style={{
              transition: 'transform 0.2s ease',
              transform: open ? 'rotate(90deg)' : 'none',
              marginLeft: 'auto',
            }}
          />
        )}
      </button>

      {open && (
        <div
          ref={menuRef}
          className="position-absolute shadow-sm"
          style={{
            left: 'calc(100% + 10px)',
            bottom: '8px',
            zIndex: 1050,
            minWidth: '220px',
            backgroundColor: 'var(--bs-body-bg)',
            border: '1px solid var(--bs-border-color)',
            borderRadius: '0.75rem',
            overflow: 'hidden',
          }}
        >
          <div className="px-3 py-2 text-body-secondary small" style={{ whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {user.email}
          </div>

          <Link to="/account" className="d-flex align-items-center gap-2 px-3 py-2 text-decoration-none text-body" onClick={() => setOpen(false)}>
            <User size={16} /> Account
          </Link>

          <hr className="dropdown-divider my-1" />

          <button
            type="button"
            className="d-flex align-items-center gap-2 px-3 py-2 text-danger border-0 bg-transparent w-100 text-start"
            onClick={async () => {
              setOpen(false)
              try {
                await apiLogout()
              } catch {
                // ignore
              }
              onLogout()
            }}
          >
            <LogOut size={16} /> Logout
          </button>
        </div>
      )}
    </div>
  )
}

export default function Sidebar({ children }) {
  const { user, logout } = useAuth()
  const location = useLocation()
  const [collapsed, setCollapsed] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const [isMobile, setIsMobile] = useState(() => window.innerWidth < 768)

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth < 768)
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  useEffect(() => {
    setMobileOpen(false)
    setCollapsed(false)
  }, [location.pathname])

  const sidebarWidth = collapsed ? 64 : 240

  return (
    <Background>
      <div className="d-flex" style={{ minHeight: '100vh' }}>
        <aside
          className="d-flex flex-column"
          style={{
            position: isMobile ? 'fixed' : 'sticky',
            top: 0,
            left: 0,
            height: '100vh',
            zIndex: 1040,
            transition: 'transform 0.2s ease, width 0.2s ease',
            width: sidebarWidth,
            transform: isMobile && !mobileOpen ? `translateX(-${sidebarWidth}px)` : 'none',
            overflow: 'visible',
            backgroundColor: 'var(--bs-body-bg)',
            borderRight: isMobile ? '1px solid var(--bs-border-color)' : 'none',
            flexShrink: 0,
          }}
        >
          <div className="d-flex align-items-center justify-content-between p-3 border-bottom" style={{ height: '56px', minHeight: '56px' }}>
            <Link to="/" className="text-decoration-none text-body fw-bold d-flex align-items-center gap-2" style={{ whiteSpace: 'nowrap', overflow: 'hidden' }}>
              <Layers size={20} />
              {!collapsed && <span>OpenLicense</span>}
            </Link>

            {isMobile ? (
              <button type="button" className="btn btn-link text-body p-0 border-0" onClick={() => setMobileOpen(false)} aria-label="Close menu">
                <X size={20} />
              </button>
            ) : (
              <button type="button" className="btn btn-link text-body p-0 border-0" onClick={() => setCollapsed((previous) => !previous)} title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'} aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}>
                {collapsed ? <ChevronLeft size={20} /> : <ChevronRight size={20} />}
              </button>
            )}
          </div>

          <nav className="flex-grow-1 p-2 overflow-auto">
            {user &&
              sidebarLinks.map((link) => (
                <SidebarItem
                  key={link.label || link.to}
                  link={link}
                  collapsed={collapsed}
                  active={link.external ? false : location.pathname === link.to}
                  onNavigate={() => setMobileOpen(false)}
                />
              ))}
          </nav>

          {user && <UserMenu user={user} collapsed={collapsed} onLogout={() => { logout(); window.location.href = '/login' }} />}
        </aside>

        {isMobile && mobileOpen && <div className="position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-50" style={{ zIndex: 1035 }} onClick={() => setMobileOpen(false)} />}

        <div className="flex-grow-1 d-flex flex-column" style={{ minWidth: 0 }}>
          {isMobile && (
            <div className="d-flex align-items-center justify-content-between p-3 border-bottom" style={{ height: '56px', minHeight: '56px', backgroundColor: 'var(--bs-body-bg)' }}>
              <Link to="/" className="text-decoration-none text-body fw-bold d-flex align-items-center gap-2">
                <Layers size={18} /> OpenLicense
              </Link>
              <button type="button" className="btn btn-link text-body p-0 border-0" onClick={() => setMobileOpen((previous) => !previous)} aria-label={mobileOpen ? 'Close menu' : 'Open menu'}>
                {mobileOpen ? <X size={22} /> : <Menu size={22} />}
              </button>
            </div>
          )}

          {!isMobile && (
            <>
              {!collapsed && (
                <button type="button" className="btn btn-link text-body d-none d-md-block position-fixed" style={{ left: `${sidebarWidth}px`, top: '64px', zIndex: 1030, transform: 'translateX(-100%)', padding: '0.25rem' }} onClick={() => setCollapsed(true)} title="Collapse sidebar" aria-label="Collapse sidebar">
                  <ChevronRight size={20} />
                </button>
              )}

              {collapsed && (
                <button type="button" className="btn btn-link text-body d-none d-md-block position-fixed" style={{ left: `${sidebarWidth}px`, top: '64px', zIndex: 1030, transform: 'translateX(-100%)', padding: '0.25rem' }} onClick={() => setCollapsed(false)} title="Expand sidebar" aria-label="Expand sidebar">
                  <ChevronLeft size={20} />
                </button>
              )}
            </>
          )}

          <main className="flex-grow-1 py-4" style={{ maxWidth: '1100px', width: '100%', margin: '0 auto', padding: '1rem' }}>
            {children}
          </main>
        </div>
      </div>
    </Background>
  )
}
