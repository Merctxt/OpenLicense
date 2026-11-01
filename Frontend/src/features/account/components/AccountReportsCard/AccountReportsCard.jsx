import { Info } from 'lucide-react'

export default function AccountReportsCard({ user }) {
  return (
    <div className="card mb-3">
      <div className="card-header d-flex justify-content-between align-items-center">
        <span className="fw-semibold d-flex align-items-center gap-2">
          <Info size={16} />
          Reports
        </span>
      </div>
      <div className="card-body">
        <p className="text-body-secondary small mb-3">
          Generate a comprehensive PDF report of all your licenses at any time. 
          Access it directly from the Products page.
        </p>
      </div>
    </div>
  )
}
