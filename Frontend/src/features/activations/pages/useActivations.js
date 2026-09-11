import { useState, useEffect, useCallback, useMemo } from 'react'
import { getProducts, getLicenseActivations, deactivateLicense } from '../../../shared/api/endpoints'

export default function useActivations() {
  const [products, setProducts] = useState([])
  const [allLicenses, setAllLicenses] = useState([])
  const [loading, setLoading] = useState(true)
  const [selectedLicenseId, setSelectedLicenseId] = useState('')
  const [activationsData, setActivationsData] = useState([])
  const [activationsLoading, setActivationsLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [licenseSearch, setLicenseSearch] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const clearAlert = () => {
    setError('')
    setSuccess('')
  }

  const load = useCallback(async () => {
    try {
      const res = await getProducts()
      const prods = res.data
      setProducts(prods)
      const licenses = []
      for (const product of prods) {
        for (const license of (product.licenses || [])) {
          licenses.push({ ...license, productId: product.id, productName: product.name })
        }
      }
      setAllLicenses(licenses)
      if (licenses.length > 0 && !selectedLicenseId) {
        setSelectedLicenseId(licenses[0].id)
      }
    } catch {
      setError('Failed to load activations')
    } finally {
      setLoading(false)
    }
  }, [selectedLicenseId])

  useEffect(() => { load() }, [load])

  const selectedLicense = useMemo(() => {
    return allLicenses.find(l => l.id === selectedLicenseId) || null
  }, [allLicenses, selectedLicenseId])

  const filteredLicenses = useMemo(() => {
    if (!licenseSearch.trim()) return allLicenses
    const term = licenseSearch.toLowerCase()
    return allLicenses.filter(l => {
      const nameMatch = l.name?.toLowerCase().includes(term)
      const keyMatch = l.licenseKey?.toLowerCase().includes(term)
      const productMatch = l.productName?.toLowerCase().includes(term)
      return nameMatch || keyMatch || productMatch
    })
  }, [allLicenses, licenseSearch])

  const handleSelectLicense = useCallback((licenseId) => {
    setSelectedLicenseId(licenseId)
  }, [])

  const loadActivations = useCallback(async (licenseId) => {
    if (!licenseId) {
      setActivationsData([])
      return
    }
    clearAlert()
    setActivationsLoading(true)
    try {
      const res = await getLicenseActivations(licenseId)
      setActivationsData(res.data)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load activations')
    } finally {
      setActivationsLoading(false)
    }
  }, [])

  useEffect(() => { loadActivations(selectedLicenseId) }, [selectedLicenseId, loadActivations])

  const handleRemoveActivation = async (licenseKey, hardwareId) => {
    if (!confirm(`Remove activation for hardware "${hardwareId}"?`)) return
    clearAlert()
    setSubmitting(true)
    try {
      await deactivateLicense({ licenseKey, hardwareId })
      if (selectedLicenseId) {
        await loadActivations(selectedLicenseId)
      }
      setSuccess('Activation removed successfully')
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove activation')
    } finally {
      setSubmitting(false)
    }
  }

  return {
    products, allLicenses, filteredLicenses, loading, submitting,
    licenseSearch, setLicenseSearch,
    selectedLicenseId, setSelectedLicenseId,
    selectedLicense,
    activationsData, activationsLoading,
    error, success,
    clearAlert,
    handleRemoveActivation,
    handleSelectLicense,
  }
}
