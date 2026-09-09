import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'

const titleMap = {
  '/': 'OpenLicense - Dashboard',
  '/login': 'OpenLicense - Login',
  '/register': 'OpenLicense - Register',
  '/forgot-password': 'OpenLicense - Forgot Password',
  '/verify-token': 'OpenLicense - Verify Token',
  '/reset-password': 'OpenLicense - Reset Password',
  '/account': 'OpenLicense - Account',
  '/metrics': 'OpenLicense - Metrics',
  '/terms': 'OpenLicense - Terms',
}

export default function UseTitle() {
  const location = useLocation()

  useEffect(() => {
    const path = location.pathname
    const title = titleMap[path] || 'OpenLicense'
    document.title = title
  }, [location])
}
