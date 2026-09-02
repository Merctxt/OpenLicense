import { useState } from 'react'
import { useAuth } from '../../context/AuthContext'
import { updateAccount, deleteAccount, createApiKey, deleteApiKey } from '../../api/endpoints'
import { useNavigate } from 'react-router-dom'

const passwordRules = [
  { key: 'length', label: '8+ characters', test: pw => pw.length >= 8 },
  { key: 'upper', label: '1 uppercase letter', test: pw => /[A-Z]/.test(pw) },
  { key: 'lower', label: '1 lowercase letter', test: pw => /[a-z]/.test(pw) },
  { key: 'digit', label: '1 number', test: pw => /[0-9]/.test(pw) },
  { key: 'special', label: '1 special character', test: pw => /[^A-Za-z0-9]/.test(pw) },
]

export default function useAccount() {
  const { user, logout, loadUser } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [apiKeyModal, setApiKeyModal] = useState(false)
  const [newKeyName, setNewKeyName] = useState('')
  const [createdKey, setCreatedKey] = useState('')
  const [editing, setEditing] = useState(false)
  const [name, setName] = useState(user?.name || '')
  const [email, setEmail] = useState(user?.email || '')
  const [password, setPassword] = useState('')

  const passwordRulesState = passwordRules.map(rule => ({
    ...rule,
    passed: !password || rule.test(password),
  }))
  const pwAllPassed = password === '' || passwordRulesState.every(r => r.passed)

  const clearMsg = () => { setError(''); setSuccess('') }

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    clearMsg()
    try {
      await updateAccount({ name, email, password: password || undefined })
      await loadUser()
      setEditing(false)
      setSuccess('Profile updated')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update profile')
    }
  }

  const handleDeleteAccount = async () => {
    if (!confirm('Are you sure? This will permanently delete your account, all products, and licenses.')) return
    clearMsg()
    try {
      await deleteAccount()
      logout()
      navigate('/login')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete account')
    }
  }

  const handleCreateApiKey = async (e) => {
    e.preventDefault()
    clearMsg()
    try {
      const res = await createApiKey({ name: newKeyName })
      setCreatedKey(res.data.apiKey)
      await loadUser()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create API key')
    }
  }

  const handleDeleteApiKey = async (id) => {
    if (!confirm('Delete this API key?')) return
    clearMsg()
    try {
      await deleteApiKey({ apiKeyId: id })
      await loadUser()
      setSuccess('API key deleted')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete API key')
    }
  }

  return {
    user,
    error, success,
    apiKeyModal, setApiKeyModal,
    newKeyName, setNewKeyName,
    createdKey, setCreatedKey,
    editing, setEditing,
    name, setName,
    email, setEmail,
    password, setPassword,
    passwordRulesState,
    pwAllPassed,
    handleUpdateProfile,
    handleDeleteAccount,
    handleCreateApiKey,
    handleDeleteApiKey,
  }
}
