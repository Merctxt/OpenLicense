import { useState, useCallback } from 'react'
import { getProducts, createProduct, updateProduct, deleteProduct, generateReportPdf } from '../../../shared/api/endpoints'
import { useAlert } from '../../../shared/context/AlertContext'
import { useConfirmation } from '../../../shared/context/ConfirmationContext'
import { handleError, MESSAGES } from '../../../shared/utils/useApiError'
import { useMountEffect } from '../../../shared/hooks/useMountEffect'

export default function useProducts() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [productModal, setProductModal] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const [reportLoading, setReportLoading] = useState(false)
  const { showError, showSuccess, showInfo } = useAlert()
  const { confirm } = useConfirmation()

  const load = useCallback(async () => {
    try {
      const res = await getProducts()
      setProducts(res.data)
    } catch {
      showError(handleError(null, 'products.loadFailed', MESSAGES.products.loadFailed))
    } finally {
      setLoading(false)
    }
  }, [showError])

  useMountEffect(load)

  const handleCreateProduct = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    const fd = new FormData(e.target)
    try {
      await createProduct({ name: fd.get('name'), description: fd.get('description') || undefined })
      setProductModal(null)
      await load()
      showSuccess(MESSAGES.products.created)
    } catch (err) {
      const msg = handleError(err, 'products.createFailed', MESSAGES.products.createFailed)
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
      showSuccess(MESSAGES.products.updated)
    } catch (err) {
      showError(handleError(err, 'products.updateFailed', MESSAGES.products.updateFailed))
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteProduct = async (id) => {
    const c = await confirm('Delete product', 'Delete this product and all its licenses?')
    if (!c) return
    try {
      await deleteProduct({ productId: id })
      await load()
      showSuccess(MESSAGES.products.deleted)
    } catch (err) {
      showError(handleError(err, 'products.deleteFailed', MESSAGES.products.deleteFailed))
    }
  }

  const handleGenerateReport = async () => {
    try {
      setReportLoading(true)
      const response = await generateReportPdf()
      const blob = new Blob([response.data], { type: 'application/pdf' })
      const url = URL.createObjectURL(blob)
      window.open(url, '_blank')
      URL.revokeObjectURL(url)
      showSuccess(MESSAGES.account.reportGenerated)
    } catch (err) {
      showError(handleError(err, 'account.reportGenerated', MESSAGES.account.reportGenerated))
    } finally {
      setReportLoading(false)
    }
  }

  return {
    products, loading,
    productModal, setProductModal,
    submitting,
    handleCreateProduct,
    handleEditProduct,
    handleDeleteProduct,
    load,
    reportLoading,
    handleGenerateReport,
  }
}
