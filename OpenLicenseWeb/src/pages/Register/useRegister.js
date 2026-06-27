import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { register } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'

export default function useRegister() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (password.length < 8) {
      setError('Password must be at least 8 characters')
      return
    }
    setSubmitting(true)
    try {
      await register({ name, email, password })
      navigate('/login', { state: { successMessage: 'Account created successfully! Please sign in.' } })
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed')
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
    handleSubmit,
  }
}
