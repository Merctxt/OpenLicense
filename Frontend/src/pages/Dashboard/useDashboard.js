import { useState, useEffect, useCallback } from 'react'
import { getProducts, createProduct, updateProduct, deleteProduct, createLicense, updateLicense, deleteLicense, getLicenseActivations, deactivateLicense } from '../../api/endpoints'

export default function useDashboard() {
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
      const statusAtual = licenseModal.license.status
      const name = fd.get('name')
      const maxAct = fd.get('maxActivations')
      const statusRaw = fd.get('status')
      const expires = fd.get('expiresAt')
      if (name) payload.name = name
      if (maxAct) payload.maxActivations = parseInt(maxAct)
      if (statusRaw !== '' && statusRaw !== null) {
        const novoStatus = statusRaw === 'true'
        if (novoStatus === statusAtual) {
          payload.status = null
        } else {
          payload.status = novoStatus
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

  const handleViewActivations = async (licenseId) => {
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
      const res = await getLicenseActivations(licenseId)
      setActivationsData(prev => ({ ...prev, [licenseId]: res.data }))
      setSuccess('Activation removed successfully')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove activation')
    }
  }

  return {
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
  }
}
