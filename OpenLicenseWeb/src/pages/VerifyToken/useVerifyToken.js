import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { verifyToken } from '../../api/endpoints'

export default function useVerifyToken() {
  const [token, setToken] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()
  const location = useLocation()

  const email = location.state?.email || ''

  const handleVerify = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setSubmitting(true)
    try {
      await verifyToken({ email, token })
      setSuccess('Token verified! Redirecting...')
      setTimeout(() => {
        navigate('/reset-password', { state: { email, token } })
      }, 1500)
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid token')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email,
    token, setToken,
    error, success,
    submitting,
    handleVerify,
  }
}
