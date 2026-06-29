import Modal from '../../components/Modal'
import { useTheme } from '../../context/ThemeContext'
import useAccount from './useAccount'
import './Account.css'

export default function Account() {
  const { theme, setTheme } = useTheme()

  const {
    user,
    error, success,
    apiKeyModal, setApiKeyModal,
    newKeyName, setNewKeyName,
    createdKey, setCreatedKey,
    editing, setEditing,
    name, setName,
    email, setEmail,
    password, setPassword,
    handleUpdateProfile,
    handleDeleteAccount,
    handleCreateApiKey,
    handleDeleteApiKey,
  } = useAccount()

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
            <div className="table-responsive">
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
            </div>
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

      <div className="panel">
        <div className="panel-heading">
          <span>Theme</span>
        </div>
        <div className="panel-body">
          <p className="api-key-hint">Choose your preferred appearance for the dashboard.</p>
          <div className="theme-options">
            <label className="theme-option">
              <input type="radio" name="theme" value="system" checked={theme === 'system'} onChange={() => setTheme('system')} />
              <span className="theme-option-label">System</span>
              <span className="theme-option-desc">Follows your browser or device setting</span>
            </label>
            <label className="theme-option">
              <input type="radio" name="theme" value="light" checked={theme === 'light'} onChange={() => setTheme('light')} />
              <span className="theme-option-label">Light</span>
              <span className="theme-option-desc">Light background with dark text</span>
            </label>
            <label className="theme-option">
              <input type="radio" name="theme" value="dark" checked={theme === 'dark'} onChange={() => setTheme('dark')} />
              <span className="theme-option-label">Dark</span>
              <span className="theme-option-desc">Dark background with light text</span>
            </label>
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
