import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { verifyToken } from '../../../../shared/api/endpoints'
import { useAlert } from '../../../../shared/context/AlertContext'
import { handleError, MESSAGES } from '../../../../shared/utils/useApiError'

export default function useVerifyToken() {
  const [token, setToken] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()
  const location = useLocation()
  const { showError, showSuccess } = useAlert()

  const email = location.state?.email || ''

  const handleVerify = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await verifyToken({ email, token })
      showSuccess(MESSAGES.auth.passwordResetSuccess)
      setTimeout(() => {
        navigate('/reset-password', { state: { email, token } })
      }, 1500)
    } catch (err) {
      showError(handleError(err, 'recovery.invalidToken', MESSAGES.recovery.invalidToken))
    } finally {
      setSubmitting(false)
    }
  }

  return {
    email,
    token, setToken,
    submitting,
    handleVerify,
  }
}
