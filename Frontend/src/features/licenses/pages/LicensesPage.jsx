import { Fragment, useCallback } from 'react'
import Modal from '../../../shared/components/Modal/Modal'
import { EmptyState } from '../../../shared/components/EmptyState'
import { LoadingState } from '../../../shared/components/LoadingState'
import useLicenses from './useLicenses'
import { Alert } from '../../../shared/components/Alert'

export default function Licenses() {
  const {
    products, loading, displayLicenses, submitting,
    licenseModal, setLicenseModal,
    error, success,
    licSearch, setLicSearch,
    licStatusFilter, setLicStatusFilter,
    licPage, setLicPage,
    licPageSize, setLicPageSize,
    selectedProductId, setSelectedProductId,
    totalItems, totalPages, activePage, startIndex,
    clearAlert,
    handleCreateLicense,
    handleEditLicense,
    handleDeleteLicense,
  } = useLicenses()

  const handleLicenseModalClose = useCallback(() => setLicenseModal(null), [setLicenseModal])

  if (loading) {
    return <LoadingState full message="Loading licenses..." />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Licenses</h1>
        <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: selectedProductId !== 'all' ? selectedProductId : products[0]?.id })}>+ Add License</button>
      </div>

      {error && <Alert type="error" message={error} onDismiss={clearAlert} />}
      {success && <Alert type="success" message={success} onDismiss={clearAlert} />}

      <div className="d-flex flex-wrap gap-2 mb-3 bg-body-tertiary p-3 rounded border">
        <div className="flex-grow-1 position-relative">
          <input
            type="text"
            className="form-control form-control-sm"
            placeholder="Search by name or key..."
            value={licSearch}
            onChange={(e) => { setLicSearch(e.target.value); setLicPage(1); }}
          />
          {licSearch && (
            <button className="btn btn-sm position-absolute end-0 top-50 translate-middle-y border-0 bg-transparent text-body-secondary" onClick={() => { setLicSearch(''); setLicPage(1); }}>&times;</button>
          )}
        </div>
        <div className="d-flex align-items-center gap-2">
          <label className="form-label mb-0 small text-nowrap">Product:</label>
          <select
            className="form-select form-select-sm"
            style={{ width: '200px' }}
            value={selectedProductId}
            onChange={(e) => { setSelectedProductId(e.target.value); setLicPage(1); }}
          >
            <option value="all">All Products</option>
            {products.map(p => (
              <option key={p.id} value={p.id}>{p.name}</option>
            ))}
          </select>
        </div>
        <div className="d-flex align-items-center gap-2">
          <label className="form-label mb-0 small text-nowrap">Status:</label>
          <select
            className="form-select form-select-sm"
            style={{ width: '130px' }}
            value={licStatusFilter}
            onChange={(e) => { setLicStatusFilter(e.target.value); setLicPage(1); }}
          >
            <option value="all">All</option>
            <option value="active">Active</option>
            <option value="suspended">Suspended</option>
          </select>
        </div>
      </div>

      {displayLicenses.length === 0 ? (
        <div className="card">
          <EmptyState
            title="No licenses found"
            description={licSearch || licStatusFilter !== 'all' || selectedProductId !== 'all' ? 'Try adjusting your search or filters.' : 'Create your first license to get started.'}
            action={
              (licSearch || licStatusFilter !== 'all' || selectedProductId !== 'all') ? null : (
                <button className="btn btn-primary" onClick={() => setLicenseModal({ mode: 'create', productId: products[0]?.id })}>Create License</button>
              )
            }
          />
        </div>
      ) : (
        <div>
          <div className="table-responsive">
            <table className="table table-sm align-middle mb-0">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Name</th>
                  <th>Key</th>
                  <th>Status</th>
                  <th>Max Activations</th>
                  <th>Expires</th>
                  <th className="text-end">Actions</th>
                </tr>
              </thead>
              <tbody>
                {displayLicenses.map((lic) => (
                  <Fragment key={lic.id}>
                    <tr>
                      <td>{lic.productName}</td>
                      <td>{lic.name}</td>
                      <td><code className="font-mono small bg-body-tertiary px-2 py-1 rounded border text-break">{lic.licenseKey}</code></td>
                      <td>
                        {lic.status ? (
                          <span className="badge bg-success-subtle text-success-emphasis border border-success-subtle">Active</span>
                        ) : (
                          <span className="badge bg-danger-subtle text-danger-emphasis border border-danger-subtle">Suspended</span>
                        )}
                      </td>
                      <td>{lic.maxActivations}</td>
                      <td>{lic.expiresAt ? new Date(lic.expiresAt).toLocaleDateString() : 'Never'}</td>
                      <td>
                        <div className="d-flex gap-2 justify-content-end">
                          <button className="btn btn-link btn-sm text-decoration-none p-0" onClick={() => setLicenseModal({ mode: 'edit', license: lic, productId: lic.productId })}>Edit</button>
                          <button className="btn btn-link btn-sm text-decoration-none text-danger p-0" onClick={() => handleDeleteLicense(lic.id)}>Delete</button>
                        </div>
                      </td>
                    </tr>
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>

          {totalPages > 1 && (
            <div className="d-flex flex-wrap justify-content-between align-items-center mt-3 pt-2 border-top gap-2">
              <span className="text-body-secondary small">
                Showing {startIndex + 1} to {Math.min(startIndex + licPageSize, totalItems)} of {totalItems} licenses
              </span>

              <div className="d-flex gap-1">
                <button
                  className="btn btn-outline-secondary btn-sm"
                  disabled={activePage === 1}
                  onClick={() => setLicPage(prev => Math.max(1, prev - 1))}
                >
                  &lsaquo; Prev
                </button>
                {Array.from({ length: totalPages }, (_, i) => i + 1).map(pageNum => (
                  <button
                    key={pageNum}
                    className={`btn btn-sm ${activePage === pageNum ? 'btn-primary' : 'btn-outline-secondary'}`}
                    onClick={() => setLicPage(pageNum)}
                  >
                    {pageNum}
                  </button>
                ))}
                <button
                  className="btn btn-outline-secondary btn-sm"
                  disabled={activePage === totalPages}
                  onClick={() => setLicPage(prev => Math.min(totalPages, prev + 1))}
                >
                  Next &rsaquo;
                </button>
              </div>

              <select
                className="form-select form-select-sm"
                style={{ width: 'auto' }}
                value={licPageSize}
                onChange={(e) => { setLicPageSize(Number(e.target.value)); setLicPage(1); }}
              >
                <option value={5}>5 / page</option>
                <option value={10}>10 / page</option>
                <option value={20}>20 / page</option>
              </select>
            </div>
          )}
        </div>
      )}

      {licenseModal && (
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
      )}
    </div>
  )
}
