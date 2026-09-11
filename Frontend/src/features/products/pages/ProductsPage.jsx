import { useCallback } from 'react'
import Modal from '../../../shared/components/Modal/Modal'
import { EmptyState } from '../../../shared/components/EmptyState'
import { LoadingState } from '../../../shared/components/LoadingState'
import useProducts from './useProducts'

export default function Products() {
  const {
    products, loading,
    productModal, setProductModal,
    submitting,
    handleCreateProduct,
    handleEditProduct,
    handleDeleteProduct,
  } = useProducts()

  const handleProductModalClose = useCallback(() => setProductModal(null), [setProductModal])

  if (loading) {
    return <LoadingState full message="Loading products..." />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Products</h1>
        <button className="btn btn-primary btn-sm" onClick={() => setProductModal({ mode: 'create' })}>+ New Product</button>
      </div>

      {products.length === 0 ? (
        <div className="card">
          <EmptyState
            title="No products yet"
            description="Create your first product to start managing licenses."
            action={<button className="btn btn-primary" onClick={() => setProductModal({ mode: 'create' })}>Create Product</button>}
          />
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
          footerLoading={submitting}
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
