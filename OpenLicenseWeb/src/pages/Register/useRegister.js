import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { register } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'

const passwordRules = [
  { key: 'length', label: '8+ characters', test: pw => pw.length >= 8 },
  { key: 'upper', label: '1 uppercase letter', test: pw => /[A-Z]/.test(pw) },
  { key: 'lower', label: '1 lowercase letter', test: pw => /[a-z]/.test(pw) },
  { key: 'digit', label: '1 number', test: pw => /[0-9]/.test(pw) },
  { key: 'special', label: '1 special character', test: pw => /[^A-Za-z0-9]/.test(pw) },
]

export default function useRegister() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading } = useAuth()
  const navigate = useNavigate()

  const passwordRulesState = passwordRules.map(rule => ({
    ...rule,
    passed: !password || rule.test(password),
  }))
  const allRulesPassed = passwordRulesState.every(r => r.passed)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      await register({ name, email, password })
      navigate('/login', { state: { successMessage: 'Account created successfully! Please sign in.' } })
    } catch (err) {
      setError(err.response?.data?.message || err.response?.data?.error?.join(' ') || 'Registration failed')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    name, setName,
    email, setEmail,
    password, setPassword,
    error, submitting,
    user, loading,
    passwordRulesState,
    allRulesPassed,
    handleSubmit,
  }
}
