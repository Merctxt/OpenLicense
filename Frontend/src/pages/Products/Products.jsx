import { useCallback } from 'react'
import Modal from '../../components/Modal'
import useProducts from './useProducts'
import { Alert } from '../../components/Alert'

export default function Products() {
  const {
    products, loading,
    productModal, setProductModal,
    error, success, info,
    clearAlert,
    handleCreateProduct,
    handleEditProduct,
    handleDeleteProduct,
  } = useProducts()

  const handleProductModalClose = useCallback(() => setProductModal(null), [setProductModal])

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

      {error && <Alert type="error" message={error} onDismiss={clearAlert} />}
      {info && <Alert type="info" message={info} onDismiss={clearAlert} />}
      {success && <Alert type="success" message={success} onDismiss={clearAlert} />}

      {products.length === 0 ? (
        <div className="card text-center">
          <div className="card-body py-5">
            <p className="text-body-secondary mb-3">No products yet. Create your first product to start managing licenses.</p>
            <button className="btn btn-primary" onClick={() => setProductModal({ mode: 'create' })}>Create Product</button>
          </div>
        </div>
      ) : (
        <div className="d-flex flex-column gap-3">
          {products.map((product) => (
            <div className="card" key={product.id}>
              <div className="card-body d-flex justify-content-between align-items-center py-3">
                <div className="d-flex align-items-center gap-3 flex-wrap">
                  <span className="fw-semibold">{product.name}</span>
                  {product.description && <span className="text-body-secondary small d-none d-sm-inline">{product.description}</span>}
                  <span className="badge bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle">{(product.licenses || []).length} {product.licenses && product.licenses.length === 1 ? 'license' : 'licenses'}</span>
                </div>
                <div className="d-flex gap-1">
                  <button className="btn btn-outline-secondary btn-sm" onClick={() => setProductModal({ mode: 'edit', product })}>Edit</button>
                  <button className="btn btn-outline-danger btn-sm" onClick={() => handleDeleteProduct(product.id)}>Delete</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {productModal && (
        <Modal
          title={productModal.mode === 'create' ? 'New Product' : 'Edit Product'}
          onClose={handleProductModalClose}
          footer={
            <>
              <button className="btn btn-secondary" onClick={handleProductModalClose}>Cancel</button>
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
    </div>
  )
}
