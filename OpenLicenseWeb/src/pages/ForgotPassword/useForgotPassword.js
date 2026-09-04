import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { forgotPassword } from '../../api/endpoints'

export default function useForgotPassword() {
  const [email, setEmail] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setSubmitting(true)
    try {
      await forgotPassword({ email })
      navigate('/verify-token', { state: { email } })
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to send recovery email')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email, setEmail,
    error, success,
    submitting,
    handleSubmit,
  }
}
