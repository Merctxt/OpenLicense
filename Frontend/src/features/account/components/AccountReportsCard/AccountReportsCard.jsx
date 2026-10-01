import { Mail } from 'lucide-react'

export default function AccountReportsCard({ user, submitting, handleToggleReports }) {
  return (
    <div className="card mb-3">
      <div className="card-header d-flex justify-content-between align-items-center">
        <span className="fw-semibold d-flex align-items-center gap-2">
          <Mail size={16} />
          Reports
        </span>
      </div>
      <div className="card-body">
        <p className="text-body-secondary small mb-3">Receive periodic email reports with license status summaries and important updates.</p>
        <div className="d-flex align-items-center justify-content-between">
          <div>
            <div className="fw-semibold small">Email Reports</div>
            <div className="text-body-secondary small">
              {user.reportsOptIn === true ? 'You will receive report emails.' : user.reportsOptIn === false ? 'You will not receive report emails.' : 'Select your preference below.'}
            </div>
          </div>
          <div className="d-flex gap-2">
            <button
              className={`btn btn-sm ${user.reportsOptIn === true ? 'btn-success' : 'btn-outline-secondary'}`}
              onClick={() => { handleToggleReports(true) }}
              disabled={submitting}
            >
              Yes
            </button>
            <button
              className={`btn btn-sm ${user.reportsOptIn === false ? 'btn-danger' : 'btn-outline-secondary'}`}
              onClick={() => { handleToggleReports(false) }}
              disabled={submitting}
            >
              No
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
