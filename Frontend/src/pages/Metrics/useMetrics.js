import { useState, useEffect, useRef } from 'react'
import { getProducts, getLicenseActivations } from '../../api/endpoints'
import { useAuth } from '../../context/AuthContext'

function isExpiringInDays(expiresAt, days) {
  if (!expiresAt) return false
  const now = new Date()
  const exp = new Date(expiresAt)
  const diffMs = exp.getTime() - now.getTime()
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24))
  return diffDays >= 0 && diffDays <= days
}

function isExpiringSoon(expiresAt) {
  if (!expiresAt) return false
  const now = new Date()
  const exp = new Date(expiresAt)
  const diffMs = exp.getTime() - now.getTime()
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24))
  return diffDays >= 0 && diffDays <= 30
}

export default function useMetrics() {
  const { user } = useAuth()
  const [products, setProducts] = useState([])
  const [activationsLoading, setActivationsLoading] = useState(false)
  const [productActivationData, setProductActivationData] = useState([])
  const [hasLoaded, setHasLoaded] = useState(false)

  // Load products and licenses
  useEffect(() => {
    async function load() {
      try {
        const res = await getProducts()
        setProducts(res.data || [])
      } catch {
        setProducts([])
      } finally {
        setHasLoaded(true)
      }
    }
    load()
  }, [])

  // Calculate summary metrics from products/licences data
  const totalLicenses = products.reduce((acc, p) => acc + (p.licenses?.length || 0), 0)
  const licensesActive = products.reduce((acc, p) =>
    acc + (p.licenses?.filter(l => l.status === true).length || 0), 0)
  const licensesSuspended = products.reduce((acc, p) =>
    acc + (p.licenses?.filter(l => l.status === false).length || 0), 0)

  const now = new Date()
  let exp7 = 0
  let exp30 = 0
  let perpetual = 0

  products.forEach(product => {
    ;(product.licenses || []).forEach(lic => {
      if (!lic.expiresAt) {
        perpetual++
      } else if (isExpiringInDays(lic.expiresAt, 7)) {
        exp7++
      } else if (isExpiringSoon(lic.expiresAt)) {
        exp30++
      }
    })
  })

  // Aggregate activation data per product
  useEffect(() => {
    async function loadActivations() {
      setActivationsLoading(true)

      const allLicenses = []
      products.forEach(product => {
        ;(product.licenses || []).forEach(lic => {
          allLicenses.push({ productId: product.id, productName: product.name, licenseId: lic.id })
        })
      })

      if (allLicenses.length === 0) {
        setProductActivationData([])
        setActivationsLoading(false)
        return
      }

      // Fetch activations for each license (fan-out)
      const results = await Promise.allSettled(
        allLicenses.map(async item => {
          try {
            const res = await getLicenseActivations(item.licenseId)
            const data = res.data || []
            const active = data.filter(a => a.isActive).length
            return { ...item, total: data.length, active, inactive: data.length - active }
          } catch {
            return { ...item, total: 0, active: 0, inactive: 0 }
          }
        })
      )

      const aggregated = {}
      let totals = { total: 0, active: 0, inactive: 0 }

      results.forEach(result => {
        if (result.status === 'fulfilled' && result.value) {
          const { productId, productName, active, inactive } = result.value
          totals.total += active + inactive
          totals.active += active
          totals.inactive += inactive

          if (!aggregated[productId]) {
            aggregated[productId] = { name: productName, active: 0, inactive: 0 }
          }
          aggregated[productId].active += active
          aggregated[productId].inactive += inactive
        }
      })

      setProductActivationData(Object.values(aggregated))
      setActivationsLoading(false)
    }

    loadActivations()
  }, [products])

  const summary = {
    totalProducts: products.length,
    totalLicenses,
    activations: {
      total: productActivationData.reduce((s, d) => s + d.active + d.inactive, 0),
      active: productActivationData.reduce((s, d) => s + d.active, 0),
      inactive: productActivationData.reduce((s, d) => s + d.inactive, 0),
    },
  }

  const usage = {
    productLimit: user?.productLimit ?? 0,
    licenseLimit: user?.licenseLimit ?? 0,
    apiKeyCount: (user?.apiKeys?.length ?? 0),
    productPct: Math.round(((products.length) / (user?.productLimit || 1)) * 100),
    licensePct: Math.round((totalLicenses / (user?.licenseLimit || 1)) * 100),
  }

  return {
    summary,
    licensesActive,
    licensesSuspended,
    licensesExpiring7: exp7,
    licensesExpiring30: exp30,
    licensesPerpetual: perpetual,
    usage,
    productActivationData,
    activationsLoading,
    loading: !hasLoaded,
  }
}
