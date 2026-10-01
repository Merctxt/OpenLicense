import Modal from '../../../../shared/components/Modal/Modal'

export default function AccountApiKeyModal({ isOpen, createdKey, newKeyName, setNewKeyName, submitting, handleCreateApiKey, handleModalClose }) {
  if (!isOpen) return null

  return (
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
  )
}
