import { useCallback } from 'react'
import Modal from '../../../shared/components/Modal/Modal'
import { EmptyState } from '../../../shared/components/EmptyState'
import { LoadingState } from '../../../shared/components/LoadingState'
import useAccount from './useAccount'
import PasswordValidation from '../../../shared/components/PasswordValidation/PasswordValidation'
import { Alert } from '../../../shared/components/Alert'

export default function Account() {
  const {
    user,
    error, success, info,
    apiKeyModal, setApiKeyModal,
    newKeyName, setNewKeyName,
    createdKey, setCreatedKey,
    editing, setEditing,
    submitting,
    name, setName,
    email, setEmail,
    password, setPassword,
    pwAllPassed,
    clearAlert,
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

      {error && <Alert type="error" message={error} onDismiss={clearAlert} />}
      {info && <Alert type="info" message={info} onDismiss={clearAlert} />}
      {success && <Alert type="success" message={success} onDismiss={clearAlert} />}

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
                <PasswordValidation password={password} showLabel={false} />
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
            <EmptyState title="No API keys yet" />
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
          footerLoading={submitting}
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

      <div className="border border-danger rounded p-4">
        <h3 className="h5 text-danger">Danger Zone</h3>
        <p className="text-body-secondary small mb-3">Once you delete your account, there is no going back.</p>
        <button className="btn btn-danger" onClick={handleDeleteAccount}>Delete Account</button>
      </div>
    </div>
  )
}
