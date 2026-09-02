import { useCallback } from 'react'
import Modal from '../../components/Modal'
import { useTheme } from '../../context/ThemeContext'
import useAccount from './useAccount'

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
    passwordRulesState,
    pwAllPassed,
    handleUpdateProfile,
    handleDeleteAccount,
    handleCreateApiKey,
    handleDeleteApiKey,
  } = useAccount()

  const handleModalClose = useCallback(() => setApiKeyModal(false), [setApiKeyModal])

  if (!user) return null

  return (
    <div>
      <h1 className="h4 mb-3">Account</h1>

      {error && <div className="alert alert-danger py-2">{error}</div>}
      {success && <div className="alert alert-success py-2">{success}</div>}

      <div className="card mb-3">
        <div className="card-header d-flex justify-content-between align-items-center">
          <span className="fw-semibold">Profile</span>
          {!editing && (
            <button className="btn btn-outline-secondary btn-sm" onClick={() => setEditing(true)}>Edit</button>
          )}
        </div>
        <div className="card-body">
          {editing ? (
            <form onSubmit={handleUpdateProfile}>
              <div className="mb-3">
                <label className="form-label">Name</label>
                <input className="form-control" value={name} onChange={(e) => setName(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">Email</label>
                <input className="form-control" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
              </div>
              <div className="mb-3">
                <label className="form-label">New Password <small className="text-body-secondary">(leave blank to keep current)</small></label>
                <input className="form-control" type="password" value={password} onChange={(e) => setPassword(e.target.value)} minLength={8} />
                {password && (
                  <div className="mt-2">
                    <p className="small text-body-secondary mb-1">Password must contain:</p>
                    <ul className="list-unstyled small" style={{ fontSize: '0.8rem' }}>
                      {passwordRulesState.map(rule => (
                        <li key={rule.key} className={rule.passed ? 'text-success' : 'text-danger'}>
                          {rule.passed ? '✓' : '✗'} {rule.label}
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary" disabled={!pwAllPassed}>Save</button>
                <button type="button" className="btn btn-secondary" onClick={() => { setEditing(false); setName(user.name); setEmail(user.email); setPassword('') }}>Cancel</button>
              </div>
            </form>
          ) : (
            <div>
              <div className="row mb-2">
                <div className="col-sm-3 fw-semibold text-body-secondary small">Name</div>
                <div className="col-sm-9">{user.name}</div>
              </div>
              <div className="row mb-2">
                <div className="col-sm-3 fw-semibold text-body-secondary small">Email</div>
                <div className="col-sm-9">{user.email}</div>
              </div>
              <div className="row">
                <div className="col-sm-3 fw-semibold text-body-secondary small">Member since</div>
                <div className="col-sm-9">{new Date(user.createdAt).toLocaleDateString()}</div>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-header d-flex justify-content-between align-items-center">
          <span className="fw-semibold">API Keys</span>
          <button className="btn btn-primary btn-sm" onClick={() => { setApiKeyModal(true); setNewKeyName(''); setCreatedKey('') }}>+ New Key</button>
        </div>
        <div className="card-body">
          <p className="text-body-secondary small mb-3">Use API keys to authenticate requests from your application. Maximum 3 keys per account.</p>
          {(!user.apiKeys || user.apiKeys.length === 0) ? (
            <p className="text-body-secondary small mb-0">No API keys yet.</p>
          ) : (
            <div className="table-responsive">
              <table className="table table-sm align-middle mb-0">
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
                          <span className="badge bg-success-subtle text-success-emphasis border border-success-subtle">Active</span>
                        ) : (
                          <span className="badge bg-danger-subtle text-danger-emphasis border border-danger-subtle">Inactive</span>
                        )}
                      </td>
                      <td className="text-end">
                        <button className="btn btn-link btn-sm text-decoration-none text-danger p-0" onClick={() => handleDeleteApiKey(key.id)}>Delete</button>
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
          onClose={handleModalClose}
          footer={
            createdKey ? (
              <button className="btn btn-primary" onClick={handleModalClose}>Done</button>
            ) : (
              <>
                <button className="btn btn-secondary" onClick={handleModalClose}>Cancel</button>
                <button className="btn btn-primary" type="submit" form="apikey-form">Create</button>
              </>
            )
          }
        >
          {createdKey ? (
            <div>
              <div className="alert alert-success py-2">API key created! Copy it now - it won't be shown again.</div>
              <label className="form-label">API Key</label>
              <div className="font-mono small bg-body-tertiary border rounded p-2 user-select-all text-break">{createdKey}</div>
            </div>
          ) : (
            <form id="apikey-form" onSubmit={handleCreateApiKey}>
              <div className="mb-3">
                <label className="form-label">Name</label>
                <input className="form-control" value={newKeyName} onChange={(e) => setNewKeyName(e.target.value)} required placeholder="e.g. Production App" />
              </div>
            </form>
          )}
        </Modal>
      )}

      <div className="card mb-3">
        <div className="card-header">
          <span className="fw-semibold">Account Limits</span>
        </div>
        <div className="card-body">
          <div className="d-flex gap-4 flex-wrap">
            <div>
              <div className="text-body-secondary small text-uppercase fw-semibold">Product Limit</div>
              <div className="fs-5 fw-semibold">{user.productLimit}</div>
            </div>
            <div>
              <div className="text-body-secondary small text-uppercase fw-semibold">License Limit</div>
              <div className="fs-5 fw-semibold">{user.licenseLimit}</div>
            </div>
            <div>
              <div className="text-body-secondary small text-uppercase fw-semibold">API Key Limit</div>
              <div className="fs-5 fw-semibold">3</div>
            </div>
          </div>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-header">
          <span className="fw-semibold">Theme</span>
        </div>
        <div className="card-body">
          <p className="text-body-secondary small mb-3">Choose your preferred appearance for the dashboard.</p>
          <div className="d-flex flex-column gap-2">
            <label className="d-flex align-items-center gap-3 p-3 border rounded" style={{ cursor: 'pointer' }}>
              <input type="radio" name="theme" value="system" checked={theme === 'system'} onChange={() => setTheme('system')} className="form-check-input m-0" />
              <div>
                <div className="fw-semibold">System</div>
                <div className="text-body-secondary small">Follows your browser or device setting</div>
              </div>
            </label>
            <label className="d-flex align-items-center gap-3 p-3 border rounded" style={{ cursor: 'pointer' }}>
              <input type="radio" name="theme" value="light" checked={theme === 'light'} onChange={() => setTheme('light')} className="form-check-input m-0" />
              <div>
                <div className="fw-semibold">Light</div>
                <div className="text-body-secondary small">Light background with dark text</div>
              </div>
            </label>
            <label className="d-flex align-items-center gap-3 p-3 border rounded" style={{ cursor: 'pointer' }}>
              <input type="radio" name="theme" value="dark" checked={theme === 'dark'} onChange={() => setTheme('dark')} className="form-check-input m-0" />
              <div>
                <div className="fw-semibold">Dark</div>
                <div className="text-body-secondary small">Dark background with light text</div>
              </div>
            </label>
          </div>
        </div>
      </div>

      <div className="border border-danger rounded p-4">
        <h3 className="h5 text-danger">Danger Zone</h3>
        <p className="text-body-secondary small mb-3">Once you delete your account, there is no going back.</p>
        <button className="btn btn-danger" onClick={handleDeleteAccount}>Delete Account</button>
      </div>
    </div>
  )
}
