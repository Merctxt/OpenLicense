import { EmptyState } from '../../../../shared/components/EmptyState/EmptyState'

export default function AccountApiKeysCard({ user, submitting, setShowApiKeyModal, handleDeleteApiKey, handleToggleApiKey }) {
  return (
    <>
      <div className="card mb-3">
        <div className="card-header d-flex justify-content-between align-items-center">
          <span className="fw-semibold">API Keys</span>
          <button className="btn btn-primary btn-sm" onClick={() => setShowApiKeyModal()}>+ New Key</button>
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
                        <div className="d-flex gap-2 justify-content-end">
                          <button
                            className={`btn btn-link btn-sm text-decoration-none p-0 ${key.isActive ? 'text-body-secondary' : 'text-success'}`}
                            onClick={() => handleToggleApiKey(key.id)}
                            disabled={submitting}
                          >
                            {key.isActive ? 'Disable' : 'Enable'}
                          </button>
                          <button
                            className="btn btn-link btn-sm text-decoration-none text-danger p-0"
                            onClick={() => handleDeleteApiKey(key.id)}
                            disabled={submitting}
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </>
  )
}
