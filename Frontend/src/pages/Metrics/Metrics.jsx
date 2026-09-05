import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { Sparkles, Box, FileText, CircleCheck, CircleX } from 'lucide-react'
import useMetrics from './useMetrics'

export default function Metrics() {
  const {
    summary, licensesActive, licensesSuspended, licensesExpiring7,
    licensesExpiring30, licensesPerpetual, usage,
    productActivationData, activationsLoading, loading: dataLoading,
  } = useMetrics()

  if (dataLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <div className="spinner-border text-secondary" role="status">
          <span className="visually-hidden">Loading metrics...</span>
        </div>
      </div>
    )
  }

  return (
    <div>
      <h1 className="h4 mb-3 d-flex align-items-center gap-2">
        <Sparkles width={20} height={20} />Metrics
      </h1>

      {/* Summary cards */}
      <div className="row g-3 mb-4">
        <div className="col-sm-6 col-xl-3">
          <div className="card h-100 border-0 shadow-sm">
            <div className="card-body">
              <div className="d-flex align-items-center gap-2 mb-2">
                <Box width={16} height={16} className="text-primary" />
                <span className="text-body-secondary text-uppercase fw-semibold small mb-0">Products</span>
              </div>
              <div className="fs-3 fw-bold">{summary.totalProducts}</div>
            </div>
          </div>
        </div>

        <div className="col-sm-6 col-xl-3">
          <div className="card h-100 border-0 shadow-sm">
            <div className="card-body">
              <div className="d-flex align-items-center gap-2 mb-2">
                <FileText width={16} height={16} className="text-primary" />
                <span className="text-body-secondary text-uppercase fw-semibold small mb-0">Licenses</span>
              </div>
              <div className="fs-3 fw-bold">{summary.totalLicenses}</div>
            </div>
          </div>
        </div>

        <div className="col-sm-6 col-xl-3">
          <div className="card h-100 border-0 shadow-sm">
            <div className="card-body">
              <div className="d-flex align-items-center gap-2 mb-2">
                <CircleCheck width={16} height={16} className="text-success" />
                <span className="text-body-secondary text-uppercase fw-semibold small mb-0">Active</span>
              </div>
              <div className="fs-3 fw-bold text-success">{licensesActive}</div>
            </div>
          </div>
        </div>

        <div className="col-sm-6 col-xl-3">
          <div className="card h-100 border-0 shadow-sm">
            <div className="card-body">
              <div className="d-flex align-items-center gap-2 mb-2">
                <CircleX width={16} height={16} className="text-danger" />
                <span className="text-body-secondary text-uppercase fw-semibold small mb-0">Suspended</span>
              </div>
              <div className="fs-3 fw-bold text-danger">{licensesSuspended}</div>
            </div>
          </div>
        </div>
      </div>

      {/* Account usage */}
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-header">Account Usage</div>
        <div className="card-body">
          <div className="row g-3">
            <div className="col-md-6">
              <label className="text-body-secondary small fw-semibold mb-1">Products ({summary.totalProducts} / {usage.productLimit})</label>
              <div className="progress" style={{ height: 8 }}>
                <div
                  className="progress-bar bg-primary"
                  style={{ width: `${usage.productPct}%` }}
                />
              </div>
            </div>
            <div className="col-md-6">
              <label className="text-body-secondary small fw-semibold mb-1">Licenses ({summary.totalLicenses} / {usage.licenseLimit})</label>
              <div className="progress" style={{ height: 8 }}>
                <div
                  className="progress-bar bg-primary"
                  style={{ width: `${usage.licensePct}%` }}
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Activation stats */}
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-header d-flex justify-content-between align-items-center">
          <span>Activations</span>
          {activationsLoading && (
            <span className="small text-body-secondary">Fetching activation data...</span>
          )}
        </div>
        <div className="card-body">
          <div className="row g-3 mb-3">
            <div className="col-sm-4">
              <div className="text-body-secondary small fw-semibold mb-1">Total</div>
              <div className="fs-5 fw-bold">{summary.activations.total}</div>
            </div>
            <div className="col-sm-4">
              <div className="text-body-secondary small fw-semibold mb-1">Active</div>
              <div className="fs-5 fw-bold text-success">{summary.activations.active}</div>
            </div>
            <div className="col-sm-4">
              <div className="text-body-secondary small fw-semibold mb-1">Inactive</div>
              <div className="fs-5 fw-bold text-muted">{summary.activations.inactive}</div>
            </div>
          </div>

          {productActivationData.length > 0 && (
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={productActivationData} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} />
                <Tooltip />
                <Bar dataKey="active" fill="#198754" name="Active" radius={[4, 4, 0, 0]} />
                <Bar dataKey="inactive" fill="#6c757d" name="Inactive" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>

      {/* Expiration stats */}
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-header">Expiration Stats</div>
        <div className="card-body">
          <div className="row g-3">
            <div className="col-md-3 col-sm-6 text-center">
              <div className="text-body-secondary small fw-semibold mb-1">Perpetual (no expiry)</div>
              <div className="fs-5 fw-bold">{licensesPerpetual}</div>
            </div>
            <div className="col-md-3 col-sm-6 text-center">
              <div className="text-body-secondary small fw-semibold mb-1">Expiring in 7 days</div>
              <div className="fs-5 fw-bold text-danger">{licensesExpiring7}</div>
            </div>
            <div className="col-md-3 col-sm-6 text-center">
              <div className="text-body-secondary small fw-semibold mb-1">Expiring in 30 days</div>
              <div className="fs-5 fw-bold text-warning">{licensesExpiring30}</div>
            </div>
            <div className="col-md-3 col-sm-6 text-center">
              <div className="text-body-secondary small fw-semibold mb-1">API Keys</div>
              <div className="fs-5 fw-bold">{usage.apiKeyCount} / 3</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
