import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { login } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'

export default function useLogin() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { user, loading, loadUser } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  useEffect(() => {
    if (location.state?.successMessage) {
      setSuccess(location.state.successMessage)
      navigate(location.pathname, { replace: true, state: {} })
    }
  }, [location, navigate])

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setSubmitting(true)
    try {
      const res = await login({ email, password })
      await loadUser()
      navigate('/')
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid credentials')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email, setEmail,
    password, setPassword,
    error, success,
    submitting,
    user, loading,
    handleSubmit,
  }
}
