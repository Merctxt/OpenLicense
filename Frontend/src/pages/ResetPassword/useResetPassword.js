import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { resetPassword } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'

const passwordRules = [
  { key: 'length', label: '8+ characters', test: pw => pw.length >= 8 },
  { key: 'upper', label: '1 uppercase letter', test: pw => /[A-Z]/.test(pw) },
  { key: 'lower', label: '1 lowercase letter', test: pw => /[a-z]/.test(pw) },
  { key: 'digit', label: '1 number', test: pw => /[0-9]/.test(pw) },
  { key: 'special', label: '1 special character', test: pw => /[^A-Za-z0-9]/.test(pw) },
]

export default function useResetPassword() {
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading } = useAuth()
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

  const passwordRulesState = passwordRules.map(rule => ({
    ...rule,
    passed: !password || rule.test(password),
  }))
  const allRulesPassed = passwordRulesState.every(r => r.passed)
  const passwordsMatch = confirmPassword && password === confirmPassword

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      await resetPassword({ email, token, password })
      navigate('/login', { state: { successMessage: 'Password reset successfully! Please sign in.' } })
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to reset password')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email,
    password, setPassword,
    confirmPassword, setConfirmPassword,
    error,
    submitting,
    passwordRulesState,
    allRulesPassed,
    passwordsMatch,
    user, loading,
    handleSubmit,
  }
}
