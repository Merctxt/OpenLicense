import { useState, useEffect, useCallback } from 'react'
import { getProducts, createProduct, updateProduct, deleteProduct } from '../../../shared/api/endpoints'
import { useAlert } from '../../../shared/context/AlertContext'

export default function useProducts() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [expandedId, setExpandedId] = useState(null)
  const [productModal, setProductModal] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const { showError, showSuccess, showInfo } = useAlert()

  const load = useCallback(async () => {
    try {
      const res = await getProducts()
      setProducts(res.data)
    } catch {
      showError('Failed to load products')
    } finally {
      setLoading(false)
    }
  }, [showError])

  useEffect(() => { load() }, [load])

  const handleCreateProduct = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    const fd = new FormData(e.target)
    try {
      await createProduct({ name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      showSuccess('Product created')
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to create product'
      if (msg.toLowerCase().includes('limit reached')) {
        showInfo(msg)
      } else {
        showError(msg)
      }
    } finally {
      setSubmitting(false)
    }
  }

  const handleEditProduct = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    const fd = new FormData(e.target)
    try {
      await updateProduct({ productId: productModal.product.id, name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      showSuccess('Product updated')
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update product')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteProduct = async (id) => {
    if (!confirm('Delete this product and all its licenses?')) return
    try {
      await deleteProduct({ productId: id })
      setExpandedId(null)
      await load()
      showSuccess('Product deleted')
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete product')
    }
  }

  const toggleExpand = (id) => {
    setExpandedId(expandedId === id ? null : id)
  }

  return {
    products, loading,
    expandedId, setExpandedId,
    productModal, setProductModal,
    submitting,
    handleCreateProduct,
    handleEditProduct,
    handleDeleteProduct,
    toggleExpand,
    load,
  }
}
