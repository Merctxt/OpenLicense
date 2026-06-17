import { useState } from 'react'
import { useAuth } from '../context/AuthContext'
import { updateAccount, deleteAccount, createApiKey, deleteApiKey } from '../api/endpoints'
import { useNavigate } from 'react-router-dom'
import Modal from '../components/Modal'
import './Account.css'

export default function Account() {
  const { user, setUser, logout, loadUser } = useAuth()
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

  const clearMsg = () => { setError(''); setSuccess('') }

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    clearMsg()
    try {
      await updateAccount({ userId: user.id, name, email, password: password || undefined })
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

  if (!user) return null

  return (
    <div className="account-page">
      <h1>Account</h1>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="panel">
        <div className="panel-heading">
          <span>Profile</span>
          {!editing && (
            <button className="btn btn-sm btn-default" onClick={() => setEditing(true)}>Edit</button>
          )}
        </div>
        <div className="panel-body">
          {editing ? (
            <form onSubmit={handleUpdateProfile}>
              <div className="form-group">
                <label>Name</label>
                <input value={name} onChange={(e) => setName(e.target.value)} required />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
              </div>
              <div className="form-group">
                <label>New Password <small>(leave blank to keep current)</small></label>
                <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} minLength={8} />
              </div>
              <div className="form-inline">
                <button type="submit" className="btn btn-primary">Save</button>
                <button type="button" className="btn btn-default" onClick={() => { setEditing(false); setName(user.name); setEmail(user.email); setPassword('') }}>Cancel</button>
              </div>
            </form>
          ) : (
            <div className="profile-view">
              <div className="profile-row">
                <span className="profile-label">Name</span>
                <span>{user.name}</span>
              </div>
              <div className="profile-row">
                <span className="profile-label">Email</span>
                <span>{user.email}</span>
              </div>
              <div className="profile-row">
                <span className="profile-label">Member since</span>
                <span>{new Date(user.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="panel">
        <div className="panel-heading">
          <span>API Keys</span>
          <button className="btn btn-sm btn-primary" onClick={() => { setApiKeyModal(true); setNewKeyName(''); setCreatedKey('') }}>+ New Key</button>
        </div>
        <div className="panel-body">
          <p className="api-key-hint">Use API keys to authenticate requests from your application. Maximum 3 keys per account.</p>
          {(!user.apiKeys || user.apiKeys.length === 0) ? (
            <p className="no-api-keys">No API keys yet.</p>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Created</th>
                  <th>Last Used</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {user.apiKeys.map((key) => (
                  <tr key={key.id}>
                    <td>{key.name}</td>
                    <td>{new Date(key.createdAt).toLocaleDateString()}</td>
                    <td>{key.lastUsedAt ? new Date(key.lastUsedAt).toLocaleDateString() : 'Never'}</td>
                    <td>
                      {key.isActive ? (
                        <span className="badge badge-success">Active</span>
                      ) : (
                        <span className="badge badge-danger">Inactive</span>
                      )}
                    </td>
                    <td>
                      <button className="btn-link btn-danger-link btn-sm" onClick={() => handleDeleteApiKey(key.id)}>Delete</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {apiKeyModal && (
        <Modal
          title="Create API Key"
          onClose={() => setApiKeyModal(false)}
          footer={
            createdKey ? (
              <button className="btn btn-primary" onClick={() => setApiKeyModal(false)}>Done</button>
            ) : (
              <>
                <button className="btn btn-default" onClick={() => setApiKeyModal(false)}>Cancel</button>
                <button className="btn btn-primary" type="submit" form="apikey-form">Create</button>
              </>
            )
          }
        >
          {createdKey ? (
            <div>
              <div className="alert alert-success">API key created! Copy it now - it won't be shown again.</div>
              <label>API Key</label>
              <div className="code-box" style={{ display: 'block', marginTop: 4, userSelect: 'all' }}>{createdKey}</div>
            </div>
          ) : (
            <form id="apikey-form" onSubmit={handleCreateApiKey}>
              <div className="form-group">
                <label>Name</label>
                <input value={newKeyName} onChange={(e) => setNewKeyName(e.target.value)} required placeholder="e.g. Production App" />
              </div>
            </form>
          )}
        </Modal>
      )}

      <div className="panel">
        <div className="panel-heading">
          <span>Account Limits</span>
        </div>
        <div className="panel-body">
          <div className="limits-grid">
            <div className="limit-item">
              <span className="limit-label">Product Limit</span>
              <span className="limit-value">{user.productLimit}</span>
            </div>
            <div className="limit-item">
              <span className="limit-label">License Limit</span>
              <span className="limit-value">{user.licenseLimit}</span>
            </div>
            <div className="limit-item">
              <span className="limit-label">API Key Limit</span>
              <span className="limit-value">3</span>
            </div>
          </div>
        </div>
      </div>

      <div className="danger-zone">
        <h3>Danger Zone</h3>
        <p>Once you delete your account, there is no going back.</p>
        <button className="btn btn-danger" onClick={handleDeleteAccount}>Delete Account</button>
      </div>
    </div>
  )
}
