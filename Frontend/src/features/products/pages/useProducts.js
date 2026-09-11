import { useState, useEffect, useCallback } from 'react'
import { getProducts, createProduct, updateProduct, deleteProduct } from '../../../shared/api/endpoints'

export default function useProducts() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [expandedId, setExpandedId] = useState(null)
  const [productModal, setProductModal] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [info, setInfo] = useState('')

  const clearAlert = () => {
    setError('')
    setSuccess('')
    setInfo('')
  }

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
    clearAlert()
    setSubmitting(true)
    const fd = new FormData(e.target)
    try {
      await createProduct({ name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      setSuccess('Product created')
    } catch (err) {
      const msg = err.response?.data?.message || 'Failed to create product'
      if (msg.toLowerCase().includes('limit reached')) {
        setInfo(msg)
      } else {
        setError(msg)
      }
    } finally {
      setSubmitting(false)
    }
  }

  const handleEditProduct = async (e) => {
    e.preventDefault()
    clearAlert()
    setSubmitting(true)
    const fd = new FormData(e.target)
    try {
      await updateProduct({ productId: productModal.product.id, name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      setSuccess('Product updated')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update product')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteProduct = async (id) => {
    if (!confirm('Delete this product and all its licenses?')) return
    clearAlert()
    try {
      await deleteProduct({ productId: id })
      setExpandedId(null)
      await load()
      setSuccess('Product deleted')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete product')
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
    error, success, info,
    clearAlert,
    handleCreateProduct,
    handleEditProduct,
    handleDeleteProduct,
    toggleExpand,
    load,
  }
}
