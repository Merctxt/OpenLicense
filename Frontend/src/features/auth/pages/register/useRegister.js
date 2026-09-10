import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { register } from '../../../../shared/api/endpoints'
import { useAuth } from '../../../../shared/context/AuthContext'
import { validatePassword } from '../../../../shared/components/PasswordValidation/PasswordValidation'

export default function useRegister() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading } = useAuth()
  const navigate = useNavigate()

  const { allPassed: allRulesPassed } = validatePassword(password)

  const clearAlert = () => setError('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    clearAlert()
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
    allRulesPassed,
    clearAlert,
    handleSubmit,
  }
}
