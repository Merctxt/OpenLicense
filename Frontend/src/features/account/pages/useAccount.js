import { useState, useEffect, useMemo } from 'react'
import { useAuth } from '../../../shared/context/AuthContext'
import { useAlert } from '../../../shared/context/AlertContext'
import { useConfirmation } from '../../../shared/context/ConfirmationContext'
import { updateAccount, deleteAccount, createApiKey, deleteApiKey, toggleApiKey, updateReportPreferences } from '../../../shared/api/endpoints'
import { useNavigate } from 'react-router-dom'
import { validatePassword } from '../../../shared/components/PasswordValidation/PasswordValidation'
import { handleError, MESSAGES } from '../../../shared/utils/useApiError'

export default function useAccount() {
  const { user, logout, loadUser } = useAuth()
  const navigate = useNavigate()
  const { showError, showSuccess, showInfo } = useAlert()
  const { confirm } = useConfirmation()
  const [apiKeyModal, setApiKeyModal] = useState(false)
  const [newKeyName, setNewKeyName] = useState('')
  const [createdKey, setCreatedKey] = useState('')
  const [editing, setEditing] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [name, setName] = useState(user?.name || '')
  const [email, setEmail] = useState(user?.email || '')
  const [password, setPassword] = useState('')

  useEffect(() => {
    if (user) {
      setName(user.name || '')
      setEmail(user.email || '')
    }
  }, [user])

  const pwAllPassed = useMemo(() => validatePassword(password).allPassed, [password])

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await updateAccount({ name, email, password: password || undefined })
      await loadUser()
      setEditing(false)
      showSuccess(MESSAGES.account.profileUpdated)
    } catch (err) {
      const msg = handleError(err, 'account.profileUpdated', MESSAGES.account.profileUpdated)
      if (msg.toLowerCase().includes('email change') || msg.toLowerCase().includes('maximum')) {
        showInfo(msg)
      } else {
        showError(msg)
      }
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteAccount = async () => {
    const c = await confirm('Delete account', 'Are you sure? This will permanently delete your account, all products, and licenses.')
    if (!c) return
    setSubmitting(true)
    try {
      await deleteAccount()
      logout()
      navigate('/login')
    } catch (err) {
      showError(handleError(err, 'account.deleteFailed', MESSAGES.account.deleteFailed))
    } finally {
      setSubmitting(false)
    }
  }

  const handleCreateApiKey = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      const res = await createApiKey({ name: newKeyName })
      setCreatedKey(res.data.apiKey)
      await loadUser()
    } catch (err) {
      const msg = handleError(err, 'account.apikeyCreateFailed', MESSAGES.account.apikeyCreateFailed)
      if (msg.toLowerCase().includes('limit reached')) {
        showInfo(msg)
      } else {
        showError(msg)
      }
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteApiKey = async (id) => {
    const c = await confirm('Delete API key', 'Delete this API key?')
    if (!c) return
    try {
      await deleteApiKey({ apiKeyId: id })
      await loadUser()
      showSuccess(MESSAGES.account.apiKeyDeleted)
    } catch (err) {
      showError(handleError(err, 'account.apiKeyDeleted', MESSAGES.account.apiKeyDeleted))
    }
  }

  const handleToggleApiKey = async (id) => {
    const key = user.apiKeys?.find(k => k.id === id)
    const action = key?.isActive ? 'disable' : 'enable'
    if (action === 'disable') {
      const c = await confirm('Disable API key', 'Are you sure you want to disable this API key?', { confirmLabel: 'Disable' })
      if (!c) return
    }
    setSubmitting(true)
    try {
      await toggleApiKey({ apiKeyId: id })
      await loadUser()
      showSuccess(`API key ${action === 'disable' ? 'disabled' : 'enabled'}`)
    } catch (err) {
      const actionLabel = action === 'disable' ? 'disable' : 'enable'
      showError(handleError(err, 'account.apikeyToggleFailed', MESSAGES.account.apikeyToggleFailed(actionLabel)))
    } finally {
      setSubmitting(false)
    }
  }

  const handleToggleReports = async (value) => {
    setSubmitting(true)
    try {
      await updateReportPreferences(value)
      await loadUser()
      showSuccess(value ? MESSAGES.account.reportsEnabled : MESSAGES.account.reportsDisabled)
    } catch (err) {
      showError(handleError(err, 'account.preferencesFailed', MESSAGES.account.preferencesFailed))
    } finally {
      setSubmitting(false)
    }
  }

  return {
    user,
    apiKeyModal, setApiKeyModal,
    newKeyName, setNewKeyName,
    createdKey, setCreatedKey,
    editing, setEditing,
    submitting,
    name, setName,
    email, setEmail,
    password, setPassword,
    pwAllPassed,
    handleUpdateProfile,
    handleDeleteAccount,
    handleCreateApiKey,
    handleDeleteApiKey,
    handleToggleApiKey,
    handleToggleReports,
  }
}
