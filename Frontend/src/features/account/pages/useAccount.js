import { useState } from 'react'
import { useAuth } from '../../../shared/context/AuthContext'
import { useAlert } from '../../../shared/context/AlertContext'
import { useConfirmation } from '../../../shared/context/ConfirmationContext'
import { updateAccount, deleteAccount, createApiKey, deleteApiKey, toggleApiKey, updateReportPreferences } from '../../../shared/api/endpoints'
import { useNavigate } from 'react-router-dom'
import { validatePassword } from '../../../shared/components/PasswordValidation/PasswordValidation'

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

  const { allPassed: pwAllPassed } = validatePassword(password)

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      await updateAccount({ name, email, password: password || undefined })
      await loadUser()
      setEditing(false)
      showSuccess('Profile updated')
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to update profile'
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
      showError(err.response?.data?.message || 'Failed to delete account')
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
      const msg = err.response?.data?.message || 'Failed to create API key'
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
      showSuccess('API key deleted')
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete API key')
    }
  }

  const handleToggleApiKey = async (id) => {
    const key = user.apiKeys?.find(k => k.id === id)
    const action = key?.isActive ? 'disable' : 'enable'
    if (action === 'disable') {
      const c = await confirm('Disable API key', 'Are you sure you want to disable this API key?')
      if (!c) return
    }
    setSubmitting(true)
    try {
      await toggleApiKey({ apiKeyId: id })
      await loadUser()
      showSuccess(`API key ${action === 'disable' ? 'disabled' : 'enabled'}`)
    } catch (err) {
      showError(err.response?.data?.message || `Failed to ${action} API key`)
    } finally {
      setSubmitting(false)
    }
  }

  const handleToggleReports = async (value) => {
    setSubmitting(true)
    try {
      await updateReportPreferences(value)
      await loadUser()
      showSuccess(value ? 'You\'ll now receive report emails.' : 'Report emails disabled.')
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update preferences')
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
