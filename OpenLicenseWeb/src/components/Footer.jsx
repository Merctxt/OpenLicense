import { Link } from 'react-router-dom'
import './Footer.css'

export default function Footer() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="app-footer-global">
      <div className="footer-inner">
        <span className="footer-copy">&copy; {currentYear} OpenLicense. All rights reserved.</span>
        <nav className="footer-links">
          <Link to="/docs" className="footer-link">API Docs</Link>
          <span className="footer-separator">•</span>
          <Link to="/status" className="footer-link">System Status</Link>
          <span className="footer-separator">•</span>
          <Link to="/terms" className="footer-link">Terms of Use</Link>
        </nav>
      </div>
    </footer>
  )
}
