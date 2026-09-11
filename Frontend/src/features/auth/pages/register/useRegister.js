import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { register } from '../../../../shared/api/endpoints'
import { useAuth } from '../../../../shared/context/AuthContext'
import { useAlert } from '../../../../shared/context/AlertContext'
import { validatePassword } from '../../../../shared/components/PasswordValidation/PasswordValidation'

export default function useRegister() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading } = useAuth()
  const { showError } = useAlert()
  const navigate = useNavigate()

  const { allPassed: allRulesPassed } = validatePassword(password)

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await register({ name, email, password })
      navigate('/login', { state: { successMessage: 'Account created successfully! Please sign in.' } })
    } catch (err) {
      showError(err.response?.data?.message || err.response?.data?.error?.join(' ') || 'Registration failed')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    name, setName,
    email, setEmail,
    password, setPassword,
    submitting,
    user, loading,
    allRulesPassed,
    handleSubmit,
  }
}
