import { useState, useEffect } from 'react'
import Modal from '../../../../shared/components/Modal/Modal'
import { EmptyState } from '../../../../shared/components/EmptyState'
import { LoadingState } from '../../../../shared/components/LoadingState'
import { UserX, UserCheck } from 'lucide-react'
import { getLicenseActivations, deactivateLicense } from '../../../../shared/api/endpoints'
import { useAlert } from '../../../../shared/context/AlertContext'

export function LicenseDetailsModal({ license, onClose }) {
  const [activations, setActivations] = useState([])
  const [activationsLoading, setActivationsLoading] = useState(false)
  const [removeLoading, setRemoveLoading] = useState(null)
  const { showError, showSuccess } = useAlert()

  useEffect(() => {
    if (!license) return
    loadActivations()
  }, [license])

  const loadActivations = async () => {
    if (!license) return
    setActivationsLoading(true)
    try {
      const res = await getLicenseActivations(license.id)
      setActivations(res.data)
    } catch {
      showError('Failed to load activations')
    } finally {
      setActivationsLoading(false)
    }
  }

  const handleRemoveActivation = async (hardwareId) => {
    if (!license) return
    setRemoveLoading(hardwareId)
    try {
      await deactivateLicense({ licenseKey: license.licenseKey, hardwareId })
      await loadActivations()
      showSuccess('Activation removed')
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to remove activation')
    } finally {
      setRemoveLoading(null)
    }
  }

  return (
    <Modal
      title={`License Details: ${license?.name}`}
      onClose={onClose}
      size="lg"
    >
      <div className="mb-3">
        <div className="row g-2">
          <div className="col-sm-6">
            <label className="form-label small fw-semibold text-body-secondary text-uppercase mb-1">License Key</label>
            <code className="mono w-100 d-block">{license?.licenseKey}</code>
          </div>
          <div className="col-sm-6">
            <label className="form-label small fw-semibold text-body-secondary text-uppercase mb-1">Product</label>
            <div className="fw-semibold mt-1">{license?.productName}</div>
          </div>
          <div className="col-sm-4">
            <label className="form-label small fw-semibold text-body-secondary text-uppercase mb-1">Status</label>
            <div className="mt-1"><span className="badge bg-success-subtle text-success-emphasis border border-success-subtle">{license?.status ? 'Active' : 'Suspended'}</span></div>
          </div>
          <div className="col-sm-4">
            <label className="form-label small fw-semibold text-body-secondary text-uppercase mb-1">Max Activations</label>
            <div className="fw-semibold mt-1">{license?.maxActivations}</div>
          </div>
          <div className="col-sm-4">
            <label className="form-label small fw-semibold text-body-secondary text-uppercase mb-1">Expires</label>
            <div className="fw-semibold mt-1">{license?.expiresAt ? new Date(license.expiresAt).toLocaleDateString() : 'Never'}</div>
          </div>
        </div>
      </div>

      <hr style={{ borderColor: 'rgba(255,255,255,0.06)' }} />

      <h6 className="fw-semibold d-flex align-items-center gap-2 mb-3">
        <UserCheck size={16} className="text-body-secondary" />
        Activations ({activations.length} / {license?.maxActivations})
      </h6>

      {activationsLoading ? (
        <LoadingState message="Loading activations..." />
      ) : activations.length === 0 ? (
        <EmptyState title="No activations yet" description="Activations will appear here when users activate this license." />
      ) : (
        <div className="table-responsive custom-scrollbar" style={{ maxHeight: '350px', overflowY: 'auto' }}>
          <table className="table-shadcn mb-0">
            <thead className="sticky-top" style={{ background: 'var(--bs-body-bg)' }}>
              <tr>
                <th>Activated At</th>
                <th>Last Seen</th>
                <th>Status</th>
                <th>Hardware ID</th>
                <th className="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              {activations.map((act) => (
                <tr key={act.id}>
                  <td className="text-body-secondary">{new Date(act.activatedAt).toLocaleString()}</td>
                  <td className="text-body-secondary">{act.lastSeenAt ? new Date(act.lastSeenAt).toLocaleString() : '-'}</td>
                  <td>
                    <span className="badge bg-success-subtle text-success-emphasis border border-success-subtle">
                      {act.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td><code className="mono">{act.hardwareId}</code></td>
                  <td className="text-end">
                    <button
                      className="btn btn-sm btn-shadcn-destructive p-0"
                      style={{ minWidth: 'auto', padding: '0.35rem 0.5rem' }}
                      onClick={() => handleRemoveActivation(act.hardwareId)}
                      disabled={removeLoading === act.hardwareId}
                    >
                      <UserX size={14} />
                      {removeLoading === act.hardwareId ? ' Removing...' : ' Remove'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Modal>
  )
}
