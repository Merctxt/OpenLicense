import { Link } from 'react-router-dom'
import './Footer.css'

export default function Footer() {
  const currentYear = new Date().getFullYear()
  const VITE_STATUS_URL = import.meta.env.VITE_STATUS_URL
  const VITE_SOURCE_URL = import.meta.env.VITE_SOURCE_URL

  return (
    <footer className="app-footer-global">
      <div className="footer-inner">
        <span className="footer-copy">&copy; {currentYear} OpenLicense. All rights reserved.</span>
        <nav className="footer-links">
          <Link to="/docs" className="footer-link">API Docs</Link>
          <span className="footer-separator">&bull;</span>
          <a href={VITE_STATUS_URL} className="footer-link" target="_blank" rel="noopener noreferrer">System Status</a>
          <span className="footer-separator">&bull;</span>
          <Link to="/terms" className="footer-link">Terms of Use</Link>
          <span className="footer-separator">&bull;</span>
          <a href={VITE_SOURCE_URL} target="_blank" rel="noopener noreferrer" className="footer-link">Source Code</a>
        </nav>
      </div>
    </footer>
  )
}
