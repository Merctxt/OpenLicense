import { useState, useEffect, useCallback } from 'react'
import { getProducts, createProduct, updateProduct, deleteProduct, createLicense, updateLicense, deleteLicense } from '../api/endpoints'
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
      const name = fd.get('name')
      const maxAct = fd.get('maxActivations')
      const status = fd.get('status')
      const expires = fd.get('expiresAt')
      if (name) payload.name = name
      if (maxAct) payload.maxActivations = parseInt(maxAct)
      if (status !== '') payload.status = status === 'true'
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

              {expandedId === product.id && (
                <div className="product-licenses">
                  <div className="licenses-header">
                    <h4>Licenses</h4>
                    <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: product.id })}>+ Add License</button>
                  </div>

                  {(!product.licenses || product.licenses.length === 0) ? (
                    <p className="no-licenses">No licenses for this product.</p>
                  ) : (
                    <table className="table">
                      <thead>
                        <tr>
                          <th>Name</th>
                          <th>Key</th>
                          <th>Status</th>
                          <th>Max Activations</th>
                          <th>Expires</th>
                          <th></th>
                        </tr>
                      </thead>
                      <tbody>
                        {product.licenses.map((lic) => (
                          <tr key={lic.id}>
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
                            <td className="license-actions">
                              <button className="btn-link btn-sm" onClick={() => setLicenseModal({ mode: 'edit', license: lic, productId: product.id })}>Edit</button>
                              <button className="btn-link btn-danger-link btn-sm" onClick={() => handleDeleteLicense(lic.id)}>Delete</button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  )}
                </div>
              )}
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
              <label>License Key <small>(save this - it won't be shown again)</small></label>
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
