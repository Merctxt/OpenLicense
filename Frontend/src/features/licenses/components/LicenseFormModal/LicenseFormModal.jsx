import Modal from '../../../../shared/components/Modal/Modal'

export default function LicenseFormModal({ licenseModal, handleLicenseModalClose, handleCreateLicense, handleEditLicense, submitting }) {
  return (
    <Modal
      title={licenseModal.mode === 'create' ? 'New License' : 'Edit License'}
      onClose={handleLicenseModalClose}
      footerLoading={submitting}
      footer={
        licenseModal.createdKey ? (
          <button className="btn btn-primary" onClick={handleLicenseModalClose}>Done</button>
        ) : (
          <>
            <button className="btn btn-secondary" onClick={handleLicenseModalClose}>Cancel</button>
            <button className="btn btn-primary" type="submit" form="license-form">
              {licenseModal.mode === 'create' ? 'Create' : 'Save'}
            </button>
          </>
        )
      }
    >
      {licenseModal.createdKey ? (
        <div>
          <div className="alert alert-success py-2">License created successfully!</div>
          <label className="form-label">License Key</label>
          <div className="font-mono small bg-body-tertiary border rounded p-2 user-select-all text-break">{licenseModal.createdKey}</div>
        </div>
      ) : (
        <form id="license-form" onSubmit={licenseModal.mode === 'create' ? handleCreateLicense : handleEditLicense}>
          <div className="mb-3">
            <label className="form-label">Name</label>
            <input className="form-control" name="name" defaultValue={licenseModal.license?.name || ''} required />
          </div>
          <div className="mb-3">
            <label className="form-label">Max Activations</label>
            <input className="form-control" type="number" name="maxActivations" min={1} defaultValue={licenseModal.license?.maxActivations || 1} required />
          </div>
          <div className="mb-3">
            <label className="form-label">Expires At</label>
            <input className="form-control" type="datetime-local" name="expiresAt" defaultValue={licenseModal.license?.expiresAt ? licenseModal.license.expiresAt.slice(0, 16) : ''} />
          </div>
          {licenseModal.mode === 'edit' && (
            <div className="mb-3">
              <label className="form-label">Status</label>
              <select className="form-select" name="status" defaultValue={licenseModal.license?.status !== undefined ? String(licenseModal.license.status) : ''}>
                <option value="true">Active</option>
                <option value="false">Suspended</option>
              </select>
            </div>
          )}
        </form>
      )}
    </Modal>
  )
}
