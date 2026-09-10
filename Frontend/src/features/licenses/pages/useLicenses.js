import { useState, useEffect, useCallback, useMemo } from 'react'
import { getProducts, createLicense, updateLicense, deleteLicense } from '../../../shared/api/endpoints'

export default function useLicenses() {
  const [products, setProducts] = useState([])
  const [loading, setLoading] = useState(true)
  const [licenseModal, setLicenseModal] = useState(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [licSearch, setLicSearch] = useState('')
  const [licStatusFilter, setLicStatusFilter] = useState('all')
  const [licPage, setLicPage] = useState(1)
  const [licPageSize, setLicPageSize] = useState(5)
  const [selectedProductId, setSelectedProductId] = useState('all')

  const clearAlert = () => {
    setError('')
    setSuccess('')
  }

  const load = useCallback(async () => {
    try {
      const res = await getProducts()
      setProducts(res.data)
    } catch {
      setError('Failed to load licenses')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const allLicenses = useMemo(() => {
    const licenses = []
    for (const product of products) {
      for (const license of (product.licenses || [])) {
        licenses.push({ ...license, productId: product.id, productName: product.name })
      }
    }
    return licenses
  }, [products])

  const filteredLicenses = useMemo(() => {
    let result = allLicenses

    if (selectedProductId !== 'all') {
      result = result.filter(l => l.productId === selectedProductId)
    }

    const term = licSearch.toLowerCase()
    if (term) {
      result = result.filter(lic => {
        const nameMatch = lic.name?.toLowerCase().includes(term)
        const keyMatch = lic.licenseKey?.toLowerCase().includes(term)
        return nameMatch || keyMatch
      })
    }

    if (licStatusFilter === 'active') {
      result = result.filter(l => l.status === true)
    } else if (licStatusFilter === 'suspended') {
      result = result.filter(l => l.status === false)
    }

    return result
  }, [allLicenses, selectedProductId, licSearch, licStatusFilter])

  const totalItems = filteredLicenses.length
  const totalPages = Math.max(1, Math.ceil(totalItems / licPageSize))
  const activePage = Math.min(licPage, totalPages)
  const startIndex = (activePage - 1) * licPageSize
  const displayLicenses = filteredLicenses.slice(startIndex, startIndex + licPageSize)

  const handleCreateLicense = async (e) => {
    e.preventDefault()
    clearAlert()
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
    clearAlert()
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
    clearAlert()
    try {
      await deleteLicense({ licenseId: id })
      setLicenseModal(null)
      await load()
      setSuccess('License deleted')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete license')
    }
  }

  return {
    products, loading,
    allLicenses, filteredLicenses, displayLicenses,
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
    load,
  }
}
