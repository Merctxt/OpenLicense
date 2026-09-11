import { Alert } from '../../../shared/components/Alert'
import { EmptyState } from '../../../shared/components/EmptyState'
import { LoadingState } from '../../../shared/components/LoadingState'
import useActivations from './useActivations'

export default function Activations() {
  const {
    filteredLicenses, allLicenses, loading, submitting,
    licenseSearch, setLicenseSearch,
    selectedLicenseId, selectedLicense,
    activationsData, activationsLoading,
    error, success,
    clearAlert,
    handleRemoveActivation,
    handleSelectLicense,
  } = useActivations()

  const hasLicenses = allLicenses.length > 0
  const showSearch = licenseSearch.trim() !== ''

  if (loading) {
    return <LoadingState full message="Loading activations..." />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Activations</h1>
      </div>

      {error && <Alert type="error" message={error} onDismiss={clearAlert} />}
      {success && <Alert type="success" message={success} onDismiss={clearAlert} />}

      {!hasLicenses ? (
        <div className="card">
          <EmptyState title="No licenses available" description="Create licenses from the Licenses page first." />
        </div>
      ) : (
        <div className="mb-3 bg-body-tertiary p-3 rounded border">
          <div className="d-flex flex-wrap gap-2 align-items-center">
            <label className="form-label mb-0 small text-nowrap">License:</label>
            <div className="flex-grow-1 position-relative">
              <input
                type="text"
                className="form-control form-control-sm"
                placeholder="Search by name, key, or product..."
                value={licenseSearch}
                onChange={(e) => setLicenseSearch(e.target.value)}
              />
              {licenseSearch && (
                <button className="btn btn-sm position-absolute end-0 top-50 translate-middle-y border-0 bg-transparent text-body-secondary" onClick={() => setLicenseSearch('')}>
                  &times;
                </button>
              )}
            </div>
            <select
              className="form-select form-select-sm"
              style={{ minWidth: '300px' }}
              value={selectedLicenseId}
              onChange={(e) => handleSelectLicense(e.target.value)}
            >
              {(showSearch ? filteredLicenses : allLicenses).map(l => (
                <option key={l.id} value={l.id}>
                  {l.name} ({l.licenseKey.slice(0, 12)}...) - {l.productName}
                </option>
              ))}
            </select>
          </div>
        </div>
      )}

      {selectedLicense && (
        <div>
          <div className="d-flex justify-content-between align-items-center mb-2">
            <h6 className="mb-0">Activations for {selectedLicense.name}</h6>
            <span className="badge bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle">{activationsData.length} / {selectedLicense.maxActivations}</span>
          </div>

          {activationsLoading ? (
            <div className="text-center py-3 text-body-secondary">Loading activations...</div>
          ) : !activationsData || activationsData.length === 0 ? (
            <div className="card">
              <EmptyState title="No activations yet" description="Activations will appear here when users activate this license." />
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table table-sm align-middle mb-0">
                <thead>
                  <tr>
                    <th>Activated At</th>
                    <th>Last Seen</th>
                    <th>Status</th>
                    <th>Hardware ID</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {activationsData.map((act) => (
                    <tr key={act.id}>
                      <td>{new Date(act.activatedAt).toLocaleString()}</td>
                      <td>{act.lastSeenAt ? new Date(act.lastSeenAt).toLocaleString() : '-'}</td>
                      <td>
                        {act.isActive ? (
                          <span className="badge bg-success-subtle text-success-emphasis border border-success-subtle">Active</span>
                        ) : (
                          <span className="badge bg-danger-subtle text-danger-emphasis border border-danger-subtle">Inactive</span>
                        )}
                      </td>
                      <td><code className="font-mono small bg-body-tertiary px-2 py-1 rounded border">{act.hardwareId}</code></td>
                      <td className="text-end">
                        <button className="btn btn-link btn-sm text-decoration-none text-danger p-0" onClick={() => handleRemoveActivation(selectedLicense.licenseKey, act.hardwareId)}>Remove</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
