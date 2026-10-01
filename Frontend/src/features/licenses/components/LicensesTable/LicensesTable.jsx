import { Fragment } from 'react'
import { Eye } from 'lucide-react'

export default function LicensesTable({ displayLicenses, setLicenseModal, setDetailsLicense, handleDeleteLicense }) {
  if (displayLicenses.length === 0) return null

  return (
    <div className="table-responsive">
      <table className="table table-sm align-middle mb-0 table-rounded">
        <thead>
          <tr>
            <th>Product</th>
            <th>Name</th>
            <th>Key</th>
            <th>Status</th>
            <th>Max Activations</th>
            <th>Expires</th>
            <th className="text-end">Actions</th>
          </tr>
        </thead>
        <tbody>
          {displayLicenses.map((lic) => (
            <Fragment key={lic.id}>
              <tr>
                <td>{lic.productName}</td>
                <td>{lic.name}</td>
                <td><code className="font-mono small bg-body-tertiary px-2 py-1 rounded border text-nowrap" style={{ whiteSpace: 'nowrap' }}>{lic.licenseKey}</code></td>
                <td>
                  {lic.status ? (
                    <span className="badge bg-success-subtle text-success-emphasis border border-success-subtle">Active</span>
                  ) : (
                    <span className="badge bg-danger-subtle text-danger-emphasis border border-danger-subtle">Suspended</span>
                  )}
                </td>
                <td>{lic.maxActivations}</td>
                <td>{lic.expiresAt ? new Date(lic.expiresAt).toLocaleDateString() : 'Never'}</td>
                <td>
                  <div className="d-flex gap-2 justify-content-end">
                    <button className="btn btn-link btn-sm text-decoration-none p-0" onClick={() => setDetailsLicense(lic)} title="Details">
                      <Eye size={16} />
                    </button>
                    <button className="btn btn-link btn-sm text-decoration-none p-0" onClick={() => setLicenseModal({ mode: 'edit', license: lic, productId: lic.productId })}>Edit</button>
                    <button className="btn btn-link btn-sm text-decoration-none text-danger p-0" onClick={() => handleDeleteLicense(lic.id)}>Delete</button>
                  </div>
                </td>
              </tr>
            </Fragment>
          ))}
        </tbody>
      </table>
    </div>
  )
}
