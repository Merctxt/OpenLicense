import { useState, useEffect, useRef } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { login } from '../../../../shared/api/endpoints'
import { useAuth } from '../../../../shared/context/AuthContext'
import { useAlert } from '../../../../shared/context/AlertContext'
import { handleError, MESSAGES } from '../../../../shared/utils/useApiError'

export default function useLogin() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading, loadUser } = useAuth()
  const { showSuccess, showError } = useAlert()
  const navigate = useNavigate()
  const location = useLocation()
  const shownRef = useRef(false)

  useEffect(() => {
    if (location.state?.successMessage && !shownRef.current) {
      const pathname = location.pathname
      showSuccess(location.state.successMessage)
      navigate(pathname, { replace: true, state: {} })
      shownRef.current = true
    }
  }, [location.state?.successMessage, navigate, showSuccess, location.pathname])

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await login({ email, password })
      await loadUser()
      navigate('/')
    } catch (err) {
      showError(handleError(err, 'auth.invalidCredentials', MESSAGES.auth.invalidCredentials))
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email, setEmail,
    password, setPassword,
    submitting,
    user, loading,
    handleSubmit,
  }
}
