import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { forgotPassword } from '../../../../shared/api/endpoints'
import { useAlert } from '../../../../shared/context/AlertContext'

export default function useForgotPassword() {
  const [email, setEmail] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()
  const { showError } = useAlert()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await forgotPassword({ email })
      navigate('/verify-token', { state: { email } })
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to send recovery email')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email, setEmail,
    submitting,
    handleSubmit,
  }
}
