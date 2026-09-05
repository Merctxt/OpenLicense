import { Link } from 'react-router-dom'
import { Activity, Scale, Code2, Zap } from 'lucide-react'

export default function Footer() {
  const currentYear = new Date().getFullYear()
  const VITE_STATUS_URL = import.meta.env.VITE_STATUS_URL
  const VITE_SOURCE_URL = import.meta.env.VITE_SOURCE_URL
  const VITE_SCALER_URL = import.meta.env.VITE_API_URL + '/scalar/v1'

  return (
    <footer className="border-top py-3 mt-auto">
      <div className="container d-flex flex-column flex-sm-row justify-content-between align-items-center gap-2" style={{ maxWidth: '1100px' }}>
        <span className="text-body-secondary small">&copy; {currentYear} OpenLicense. All rights reserved.</span>
        <nav className="d-flex align-items-center gap-2 flex-wrap justify-content-center">
          <span className="text-body-secondary">&bull;</span>
          <a href={VITE_STATUS_URL} className="text-body-secondary small text-decoration-none d-flex align-items-center gap-1" target="_blank" rel="noopener noreferrer"><Activity width={14} height={14} />System Status</a>
          <span className="text-body-secondary">&bull;</span>
          <Link to="/terms" className="text-body-secondary small text-decoration-none d-flex align-items-center gap-1"><Scale width={14} height={14} />Terms of Use</Link>
          <span className="text-body-secondary">&bull;</span>
          <a href={VITE_SOURCE_URL} target="_blank" rel="noopener noreferrer" className="text-body-secondary small text-decoration-none d-flex align-items-center gap-1"><Code2 width={14} height={14} />Source Code and Docs</a>
          <span className="text-body-secondary">&bull;</span>
          <a href={VITE_SCALER_URL} target="_blank" rel="noopener noreferrer" className="text-body-secondary small text-decoration-none d-flex align-items-center gap-1"><Zap width={14} height={14} />API</a>
        </nav>
      </div>
    </footer>
  )
}
