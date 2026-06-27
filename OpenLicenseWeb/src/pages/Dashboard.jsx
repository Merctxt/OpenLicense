import React, { useState, useEffect, useCallback } from 'react'
import { getProducts, createProduct, updateProduct, deleteProduct, createLicense, updateLicense, deleteLicense, getLicenseActivations, deactivateLicense } from '../api/endpoints'
import Modal from '../components/Modal'
import './Dashboard.css'

export default function Dashboard() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [expandedId, setExpandedId] = useState(null)
  const [productModal, setProductModal] = useState(null)
  const [licenseModal, setLicenseModal] = useState(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  // Activation panel state
  const [activeActivationLicense, setActiveActivationLicense] = useState(null)
  const [activationsData, setActivationsData] = useState({})
  const [activationsLoading, setActivationsLoading] = useState({})
  const [activationsError, setActivationsError] = useState({})

  // Licenses filtering & pagination state
  const [licSearch, setLicSearch] = useState('')
  const [licStatusFilter, setLicStatusFilter] = useState('all')
  const [licPage, setLicPage] = useState(1)
  const [licPageSize, setLicPageSize] = useState(5)

  const clearMsg = () => { setError(''); setSuccess('') }

  const load = useCallback(async () => {
    try {
      const res = await getProducts()
      setProducts(res.data)
    } catch {
      setError('Failed to load products')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])


  const handleCreateProduct = async (e) => {
    e.preventDefault()
    clearMsg()
    const fd = new FormData(e.target)
    try {
      await createProduct({ name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      setSuccess('Product created')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create product')
    }
  }

  const handleEditProduct = async (e) => {
    e.preventDefault()
    clearMsg()
    const fd = new FormData(e.target)
    try {
      await updateProduct({ productId: productModal.product.id, name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      setSuccess('Product updated')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update product')
    }
  }

  const handleDeleteProduct = async (id) => {
    if (!confirm('Delete this product and all its licenses?')) return
    clearMsg()
    try {
      await deleteProduct({ productId: id })
      setExpandedId(null)
      await load()
      setSuccess('Product deleted')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete product')
    }
  }

  const handleCreateLicense = async (e) => {
    e.preventDefault()
    clearMsg()
    const fd = new FormData(e.target)
    try {
      const payload = {
        productId: licenseModal.productId,
        name: fd.get('name'),
        maxActivations: parseInt(fd.get('maxActivations')) || 1,
      }
      const expires = fd.get('expiresAt')
      if (expires) payload.expiresAt = new Date(expires).toISOString()
      const res = await createLicense(payload)
      setLicenseModal({ ...licenseModal, createdKey: res.data.licenseKey })
      await load()
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create license')
    }
  }

  const handleEditLicense = async (e) => {
    e.preventDefault()
    clearMsg()
    const fd = new FormData(e.target)
    try {
      const payload = { licenseId: licenseModal.license.id }
      const statusAtual = licenseModal.license.status;
      const name = fd.get('name')
      const maxAct = fd.get('maxActivations')
      const statusRaw = fd.get('status')
      const expires = fd.get('expiresAt')
      if (name) payload.name = name
      if (maxAct) payload.maxActivations = parseInt(maxAct)
      if (statusRaw !== '' && statusRaw !== null) {
      const novoStatus = statusRaw === 'true'; 
      if (novoStatus === statusAtual) {
        payload.status = null;
      } else {
        payload.status = novoStatus;
      }
    } 
      if (expires) payload.expiresAt = new Date(expires).toISOString()
      await updateLicense(payload)
      setLicenseModal(null)
      await load()
      setSuccess('License updated')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update license')
    }
  }

  const handleDeleteLicense = async (id) => {
    if (!confirm('Delete this license?')) return
    clearMsg()
    try {
      await deleteLicense({ licenseId: id })
      setLicenseModal(null)
      await load()
      setSuccess('License deleted')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete license')
    }
  }

  const toggleExpand = (id) => {
    setExpandedId(expandedId === id ? null : id)
    setLicSearch('')
    setLicStatusFilter('all')
    setLicPage(1)
    setActiveActivationLicense(null)
  }

  const handleViewActivations = async (licenseId, licenseKey) => {
    if (activeActivationLicense === licenseId) {
      setActiveActivationLicense(null)
      return
    }
    clearMsg()
    setActiveActivationLicense(licenseId)
    setActivationsLoading(prev => ({ ...prev, [licenseId]: true }))
    setActivationsError(prev => ({ ...prev, [licenseId]: '' }))
    try {
      const res = await getLicenseActivations(licenseId)
      setActivationsData(prev => ({ ...prev, [licenseId]: res.data }))
    } catch (err) {
      setActivationsError(prev => ({ ...prev, [licenseId]: err.response?.data?.message || 'Failed to load activations' }))
    } finally {
      setActivationsLoading(prev => ({ ...prev, [licenseId]: false }))
    }
  }

  const handleRemoveActivation = async (licenseKey, hardwareId, licenseId) => {
    if (!confirm(`Remove activation for hardware "${hardwareId}"?`)) return
    clearMsg()
    try {
      await deactivateLicense({ licenseKey, hardwareId })
      // Refresh activations list
      const res = await getLicenseActivations(licenseId)
      setActivationsData(prev => ({ ...prev, [licenseId]: res.data }))
      setSuccess('Activation removed successfully')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove activation')
    }
  }

  if (loading) return <div style={{ textAlign: 'center', padding: '40px', color: 'var(--color-text-light)' }}>Loading...</div>

  return (
    <div className="dashboard">
      <div className="page-title-row">
        <h1>Products</h1>
        <button className="btn btn-primary btn-sm" onClick={() => setProductModal({ mode: 'create' })}>+ New Product</button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {products.length === 0 ? (
        <div className="panel">
          <div className="empty-state">
            <p>No products yet. Create your first product to start managing licenses.</p>
            <button className="btn btn-primary" onClick={() => setProductModal({ mode: 'create' })}>Create Product</button>
          </div>
        </div>
      ) : (
        <div className="product-list">
          {products.map((product) => (
            <div className="panel" key={product.id}>
              <div className="product-row" onClick={() => toggleExpand(product.id)}>
                <div className="product-info">
                  <span className="product-name">{product.name}</span>
                  {product.description && <span className="product-desc">{product.description}</span>}
                  <span className="badge badge-default">{(product.licenses || []).length} licenses</span>
                </div>
                <div className="product-actions" onClick={(e) => e.stopPropagation()}>
                  <button className="btn btn-sm btn-default" onClick={() => setProductModal({ mode: 'edit', product })}>Edit</button>
                  <button className="btn btn-sm btn-danger" onClick={() => handleDeleteProduct(product.id)}>Delete</button>
                  <span className="expand-icon">{expandedId === product.id ? '\u25BC' : '\u25B8'}</span>
                </div>
              </div>

              {expandedId === product.id && (() => {
                const licenses = product.licenses || [];
                const hasLicenses = licenses.length > 0;
                
                if (!hasLicenses) {
                  return (
                    <div className="product-licenses">
                      <div className="licenses-header">
                        <h4>Licenses</h4>
                        <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: product.id })}>+ Add License</button>
                      </div>
                      <p className="no-licenses">No licenses for this product.</p>
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
                  <div className="product-licenses">
                    <div className="licenses-header">
                      <h4>Licenses</h4>
                      <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: product.id })}>+ Add License</button>
                    </div>

                    {/* Filter Bar */}
                    <div className="license-filters">
                      <div className="filter-search-wrapper">
                        <input 
                          type="text" 
                          placeholder="Search by name or key..." 
                          value={licSearch} 
                          onChange={(e) => { setLicSearch(e.target.value); setLicPage(1); }} 
                          className="filter-search"
                        />
                        {licSearch && (
                          <button className="filter-clear-btn" onClick={() => { setLicSearch(''); setLicPage(1); }}>&times;</button>
                        )}
                      </div>
                      
                      <div className="filter-status-wrapper">
                        <label>Status:</label>
                        <select 
                          value={licStatusFilter} 
                          onChange={(e) => { setLicStatusFilter(e.target.value); setLicPage(1); }}
                          className="filter-status-select"
                        >
                          <option value="all">All</option>
                          <option value="active">Active</option>
                          <option value="suspended">Suspended</option>
                        </select>
                      </div>
                    </div>

                    {filtered.length === 0 ? (
                      <p className="no-licenses">No licenses match the search filters.</p>
                    ) : (
                      <>
                      <div className="table-responsive">
                        <table className="table">
                          <thead>
                            <tr>
                              <th>Name</th>
                              <th>Key</th>
                              <th>Status</th>
                              <th>Max Activations</th>
                              <th>Expires</th>
                              <th style={{ textAlign: 'right', paddingRight: '16px' }}>Actions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {displayLicenses.map((lic) => (
                              <React.Fragment key={lic.id}>
                              <tr>
                                <td>{lic.name}</td>
                                <td><code className="code-box">{lic.licenseKey}</code></td>
                                <td>
                                  {lic.status ? (
                                    <span className="badge badge-success">Active</span>
                                  ) : (
                                    <span className="badge badge-danger">Suspended</span>
                                  )}
                                </td>
                                <td>{lic.maxActivations}</td>
                                <td>{lic.expiresAt ? new Date(lic.expiresAt).toLocaleDateString() : 'Never'}</td>
                                <td>
                                  <div className="license-actions">
                                    <button className="btn-link" onClick={() => setLicenseModal({ mode: 'edit', license: lic, productId: product.id })}>Edit</button>
                                    <button className="btn-link" onClick={() => handleViewActivations(lic.id, lic.licenseKey)}>
                                      {activeActivationLicense === lic.id ? 'Hide' : 'Activations'}
                                    </button>
                                    <button className="btn-link btn-danger-link" onClick={() => handleDeleteLicense(lic.id)}>Delete</button>
                                  </div>
                                </td>
                              </tr>
                              {activeActivationLicense === lic.id && (
                                <tr className="activation-panel-row">
                                  <td colSpan={6} style={{ padding: 0 }}>
                                    <div className="activation-panel">
                                      <div className="activation-panel-header">
                                        <h5>Activations for {lic.name}</h5>
                                        <span className="badge badge-default">{activationsData[lic.id]?.length || 0} / {lic.maxActivations}</span>
                                      </div>
                                      {activationsLoading[lic.id] ? (
                                        <div style={{ textAlign: 'center', padding: '20px', color: 'var(--color-text-light)' }}>Loading activations...</div>
                                      ) : activationsError[lic.id] ? (
                                        <div className="alert alert-error" style={{ margin: 0 }}>{activationsError[lic.id]}</div>
                                      ) : !activationsData[lic.id] || activationsData[lic.id].length === 0 ? (
                                        <p className="no-activations">No activations yet.</p>
                                      ) : (
                                        <div className="table-responsive">
                                          <table className="table table-compact">
                                            <thead>
                                              <tr>
                                                <th>Activated At</th>
                                                <th>Last Seen</th>
                                                <th>Status</th>
                                                <th style={{ textAlign: 'right' }}>Actions</th>
                                              </tr>
                                            </thead>
                                            <tbody>
                                              {activationsData[lic.id].map((act) => (
                                                <tr key={act.id}>
                                                  <td>{new Date(act.activatedAt).toLocaleString()}</td>
                                                  <td>{act.lastSeenAt ? new Date(act.lastSeenAt).toLocaleString() : '-'}</td>
                                                  <td>
                                                    {act.isActive ? (
                                                      <span className="badge badge-success">Active</span>
                                                    ) : (
                                                      <span className="badge badge-danger">Inactive</span>
                                                    )}
                                                  </td>
                                                  <td>
                                                    <div className="license-actions">
                                                      <button className="btn-link btn-danger-link" onClick={() => handleRemoveActivation(lic.licenseKey, act.hardwareId, lic.id)}>Remove</button>
                                                    </div>
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
                              </React.Fragment>
                            ))}
                          </tbody>
                        </table>
                      </div>

                        {/* Pagination Footer */}
                        <div className="license-pagination">
                          <span className="pagination-info">
                            Showing {startIndex + 1} to {Math.min(startIndex + licPageSize, totalItems)} of {totalItems} licenses
                          </span>
                          
                          <div className="pagination-buttons">
                            <button 
                              className="btn btn-default btn-xs pagination-btn" 
                              disabled={activePage === 1}
                              onClick={() => setLicPage(prev => Math.max(1, prev - 1))}
                            >
                              &lsaquo; Previous
                            </button>
                            
                            {Array.from({ length: totalPages }, (_, i) => i + 1).map(pageNum => (
                              <button
                                key={pageNum}
                                className={`btn btn-xs pagination-btn ${activePage === pageNum ? 'btn-primary active' : 'btn-default'}`}
                                onClick={() => setLicPage(pageNum)}
                              >
                                {pageNum}
                              </button>
                            ))}
                            
                            <button 
                              className="btn btn-default btn-xs pagination-btn" 
                              disabled={activePage === totalPages}
                              onClick={() => setLicPage(prev => Math.min(totalPages, prev + 1))}
                            >
                              Next &rsaquo;
                            </button>
                          </div>

                          <div className="pagination-limit">
                            <select 
                              value={licPageSize} 
                              onChange={(e) => { setLicPageSize(Number(e.target.value)); setLicPage(1); }}
                              className="pagination-select"
                            >
                              <option value={5}>5 / page</option>
                              <option value={10}>10 / page</option>
                              <option value={20}>20 / page</option>
                            </select>
                          </div>
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
              <button className="btn btn-default" onClick={() => setProductModal(null)}>Cancel</button>
              <button className="btn btn-primary" type="submit" form="product-form">
                {productModal.mode === 'create' ? 'Create' : 'Save'}
              </button>
            </>
          }
        >
          <form id="product-form" onSubmit={productModal.mode === 'create' ? handleCreateProduct : handleEditProduct}>
            <div className="form-group">
              <label>Name</label>
              <input name="name" defaultValue={productModal.product?.name || ''} required />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea name="description" rows={3} defaultValue={productModal.product?.description || ''} />
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
                <button className="btn btn-default" onClick={() => setLicenseModal(null)}>Cancel</button>
                <button className="btn btn-primary" type="submit" form="license-form">
                  {licenseModal.mode === 'create' ? 'Create' : 'Save'}
                </button>
              </>
            )
          }
        >
          {licenseModal.createdKey ? (
            <div>
              <div className="alert alert-success">License created successfully!</div>
              <label>License Key</label>
              <div className="code-box" style={{ display: 'block', marginTop: 4, userSelect: 'all' }}>{licenseModal.createdKey}</div>
            </div>
          ) : (
            <form id="license-form" onSubmit={licenseModal.mode === 'create' ? handleCreateLicense : handleEditLicense}>
              <div className="form-group">
                <label>Name</label>
                <input name="name" defaultValue={licenseModal.license?.name || ''} required />
              </div>
              <div className="form-group">
                <label>Max Activations</label>
                <input type="number" name="maxActivations" min={1} defaultValue={licenseModal.license?.maxActivations || 1} required />
              </div>
              <div className="form-group">
                <label>Expires At</label>
                <input type="datetime-local" name="expiresAt" defaultValue={licenseModal.license?.expiresAt ? licenseModal.license.expiresAt.slice(0, 16) : ''} />
              </div>
              {licenseModal.mode === 'edit' && (
                <div className="form-group">
                  <label>Status</label>
                  <select name="status" defaultValue={licenseModal.license?.status !== undefined ? String(licenseModal.license.status) : ''}>
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
