import { Fragment } from 'react'
import Modal from '../../components/Modal'
import useDashboard from './useDashboard'

export default function Dashboard() {
  const {
    products, loading,
    expandedId,
    productModal, setProductModal,
    licenseModal, setLicenseModal,
    error, success,
    activeActivationLicense,
    activationsData, activationsLoading, activationsError,
    licSearch, setLicSearch,
    licStatusFilter, setLicStatusFilter,
    licPage, setLicPage,
    licPageSize, setLicPageSize,
    handleCreateProduct,
    handleEditProduct,
    handleDeleteProduct,
    handleCreateLicense,
    handleEditLicense,
    handleDeleteLicense,
    toggleExpand,
    handleViewActivations,
    handleRemoveActivation,
  } = useDashboard()

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <div className="spinner-border text-secondary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Products</h1>
        <button className="btn btn-primary btn-sm" onClick={() => setProductModal({ mode: 'create' })}>+ New Product</button>
      </div>

      {error && <div className="alert alert-danger py-2">{error}</div>}
      {success && <div className="alert alert-success py-2">{success}</div>}

      {products.length === 0 ? (
        <div className="card text-center">
          <div className="card-body py-5">
            <p className="text-body-secondary mb-3">No products yet. Create your first product to start managing licenses.</p>
            <button className="btn btn-primary" onClick={() => setProductModal({ mode: 'create' })}>Create Product</button>
          </div>
        </div>
      ) : (
        <div className="d-flex flex-column gap-0">
          {products.map((product) => (
            <div className="card mb-3" key={product.id}>
              <div
                className="card-body d-flex justify-content-between align-items-center py-3"
                role="button"
                onClick={() => toggleExpand(product.id)}
                style={{ cursor: 'pointer' }}
              >
                <div className="d-flex align-items-center gap-3 flex-wrap">
                  <span className="fw-semibold">{product.name}</span>
                  {product.description && <span className="text-body-secondary small d-none d-sm-inline">{product.description}</span>}
                  <span className="badge bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle">{(product.licenses || []).length} licenses</span>
                </div>
                <div className="d-flex gap-1" onClick={(e) => e.stopPropagation()}>
                  <button className="btn btn-outline-secondary btn-sm" onClick={() => setProductModal({ mode: 'edit', product })}>Edit</button>
                  <button className="btn btn-outline-danger btn-sm" onClick={() => handleDeleteProduct(product.id)}>Delete</button>
                </div>
              </div>

              {expandedId === product.id && (() => {
                const licenses = product.licenses || [];
                const hasLicenses = licenses.length > 0;

                if (!hasLicenses) {
                  return (
                    <div className="border-top p-3">
                      <div className="d-flex justify-content-between align-items-center mb-2">
                        <h5 className="mb-0">Licenses</h5>
                        <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: product.id })}>+ Add License</button>
                      </div>
                      <p className="text-body-secondary small mb-0">No licenses for this product.</p>
                    </div>
                  );
                }

                const filtered = licenses.filter(lic => {
                  const term = licSearch.toLowerCase();
                  const nameMatch = lic.name ? lic.name.toLowerCase().includes(term) : false;
                  const keyMatch = lic.licenseKey ? lic.licenseKey.toLowerCase().includes(term) : false;

                  let statusMatch = true;
                  if (licStatusFilter === 'active') statusMatch = lic.status === true;
                  else if (licStatusFilter === 'suspended') statusMatch = lic.status === false;

                  return (!licSearch || nameMatch || keyMatch) && statusMatch;
                });

                const totalItems = filtered.length;
                const totalPages = Math.max(1, Math.ceil(totalItems / licPageSize));
                const activePage = Math.min(licPage, totalPages);
                const startIndex = (activePage - 1) * licPageSize;
                const displayLicenses = filtered.slice(startIndex, startIndex + licPageSize);

                return (
                  <div className="border-top p-3">
                    <div className="d-flex justify-content-between align-items-center mb-3">
                      <h5 className="mb-0">Licenses</h5>
                      <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: product.id })}>+ Add License</button>
                    </div>

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

                    {filtered.length === 0 ? (
                      <p className="text-body-secondary small mb-0">No licenses match the search filters.</p>
                    ) : (
                      <>
                        <div className="table-responsive">
                          <table className="table table-sm align-middle mb-0">
                            <thead>
                              <tr>
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
                                        <button className="btn btn-link btn-sm text-decoration-none p-0" onClick={() => setLicenseModal({ mode: 'edit', license: lic, productId: product.id })}>Edit</button>
                                        <button className="btn btn-link btn-sm text-decoration-none p-0" onClick={() => handleViewActivations(lic.id, lic.licenseKey)}>
                                          {activeActivationLicense === lic.id ? 'Hide' : 'Activations'}
                                        </button>
                                        <button className="btn btn-link btn-sm text-decoration-none text-danger p-0" onClick={() => handleDeleteLicense(lic.id)}>Delete</button>
                                      </div>
                                    </td>
                                  </tr>
                                  {activeActivationLicense === lic.id && (
                                    <tr>
                                      <td colSpan={6} className="p-0 border-bottom-0">
                                        <div className="p-3 bg-body-tertiary border rounded m-1">
                                          <div className="d-flex justify-content-between align-items-center mb-2">
                                            <h6 className="mb-0">Activations for {lic.name}</h6>
                                            <span className="badge bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle">{(activationsData[lic.id] || []).length} / {lic.maxActivations}</span>
                                          </div>
                                          {activationsLoading[lic.id] ? (
                                            <div className="text-center py-3 text-body-secondary">Loading activations...</div>
                                          ) : activationsError[lic.id] ? (
                                            <div className="alert alert-danger py-2 mb-0">{activationsError[lic.id]}</div>
                                          ) : !activationsData[lic.id] || activationsData[lic.id].length === 0 ? (
                                            <p className="text-body-secondary small mb-0">No activations yet.</p>
                                          ) : (
                                            <div className="table-responsive">
                                              <table className="table table-sm align-middle mb-0">
                                                <thead>
                                                  <tr>
                                                    <th>Activated At</th>
                                                    <th>Last Seen</th>
                                                    <th>Status</th>
                                                    <th className="text-end">Actions</th>
                                                  </tr>
                                                </thead>
                                                <tbody>
                                                  {activationsData[lic.id].map((act) => (
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
                                                      <td className="text-end">
                                                        <button className="btn btn-link btn-sm text-decoration-none text-danger p-0" onClick={() => handleRemoveActivation(lic.licenseKey, act.hardwareId, lic.id)}>Remove</button>
                                                      </td>
                                                    </tr>
                                                  ))}
                                                </tbody>
                                              </table>
                                            </div>
                                          )}
                                        </div>
                                      </td>
                                    </tr>
                                  )}
                                </Fragment>
                              ))}
                            </tbody>
                          </table>
                        </div>

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
                      </>
                    )}
                  </div>
                );
              })()}
            </div>
          ))}
        </div>
      )}

      {productModal && (
        <Modal
          title={productModal.mode === 'create' ? 'New Product' : 'Edit Product'}
          onClose={() => setProductModal(null)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setProductModal(null)}>Cancel</button>
              <button className="btn btn-primary" type="submit" form="product-form">
                {productModal.mode === 'create' ? 'Create' : 'Save'}
              </button>
            </>
          }
        >
          <form id="product-form" onSubmit={productModal.mode === 'create' ? handleCreateProduct : handleEditProduct}>
            <div className="mb-3">
              <label className="form-label">Name</label>
              <input className="form-control" name="name" defaultValue={productModal.product?.name || ''} required />
            </div>
            <div className="mb-3">
              <label className="form-label">Description</label>
              <textarea className="form-control" name="description" rows={3} defaultValue={productModal.product?.description || ''} />
            </div>
          </form>
        </Modal>
      )}

      {licenseModal && (
        <Modal
          title={licenseModal.mode === 'create' ? 'New License' : 'Edit License'}
          onClose={() => setLicenseModal(null)}
          footer={
            licenseModal.createdKey ? (
              <button className="btn btn-primary" onClick={() => setLicenseModal(null)}>Done</button>
            ) : (
              <>
                <button className="btn btn-secondary" onClick={() => setLicenseModal(null)}>Cancel</button>
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
