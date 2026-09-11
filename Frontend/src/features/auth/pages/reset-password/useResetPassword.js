import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { resetPassword } from '../../../../shared/api/endpoints'
import { useAuth } from '../../../../shared/context/AuthContext'
import { useAlert } from '../../../../shared/context/AlertContext'
import { validatePassword } from '../../../../shared/components/PasswordValidation/PasswordValidation'

export default function useResetPassword() {
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading } = useAuth()
  const { showError } = useAlert()
  const navigate = useNavigate()
  const location = useLocation()

  const email = location.state?.email || ''
  const token = location.state?.token || ''
  const searchParams = new URLSearchParams(location.search)
  const urlToken = searchParams.get('token')
  const urlEmail = searchParams.get('email')

  useEffect(() => {
    if (urlToken && urlEmail) {
      navigate('/reset-password', { state: { email: urlEmail, token: urlToken } })
    }
  }, [navigate, urlToken, urlEmail])

  useEffect(() => {
    if (!email || !token) {
      navigate('/forgot-password')
    }
  }, [email, token, navigate])

  const { allPassed: allRulesPassed } = validatePassword(password)
  const passwordsMatch = confirmPassword && password === confirmPassword

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await resetPassword({ email, token, password })
      navigate('/login', { state: { successMessage: 'Password reset successfully! Please sign in.' } })
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to reset password')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email,
    password, setPassword,
    confirmPassword, setConfirmPassword,
    submitting,
    allRulesPassed,
    passwordsMatch,
    user, loading,
    handleSubmit,
  }
}
