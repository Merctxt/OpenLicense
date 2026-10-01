export default function AccountLimitsCard({ user }) {
  return (
    <div className="card mb-3">
      <div className="card-header">
        <span className="fw-semibold">Account Limits</span>
      </div>
      <div className="card-body">
        <div className="d-flex gap-4 flex-wrap">
          <div>
            <div className="text-body-secondary small text-uppercase fw-semibold">Product Limit</div>
            <div className="fs-5 fw-semibold">{user.productLimit}</div>
          </div>
          <div>
            <div className="text-body-secondary small text-uppercase fw-semibold">License Limit</div>
            <div className="fs-5 fw-semibold">{user.licenseLimit}</div>
          </div>
          <div>
            <div className="text-body-secondary small text-uppercase fw-semibold">API Key Limit</div>
            <div className="fs-5 fw-semibold">3</div>
          </div>
        </div>
      </div>
    </div>
  )
}
