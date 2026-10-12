import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { register } from '../../../../shared/api/endpoints'
import { useAuth } from '../../../../shared/context/AuthContext'
import { useAlert } from '../../../../shared/context/AlertContext'
import { validatePassword } from '../../../../shared/components/PasswordValidation/PasswordValidation'
import { handleError, MESSAGES } from '../../../../shared/utils/useApiError'

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
      navigate('/login', { state: { successMessage: MESSAGES.auth.accountCreated } })
    } catch (err) {
      showError(handleError(err, 'auth.registrationFailed', MESSAGES.auth.registrationFailed))
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
