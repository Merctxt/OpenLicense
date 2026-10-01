import { useCallback, useState } from 'react'
import { EmptyState } from '../../../shared/components/EmptyState/EmptyState'
import { LoadingState } from '../../../shared/components/LoadingState/LoadingState'
import LicenseFormModal from '../components/LicenseFormModal/LicenseFormModal'
import { LicenseDetailsModal } from '../components/LicenseDetailsModal/LicenseDetailsModal'
import LicensesFilters from '../components/LicensesFilters/LicensesFilters'
import LicensesTable from '../components/LicensesTable/LicensesTable'
import LicensesPagination from '../components/LicensesPagination/LicensesPagination'
import useLicenses from './useLicenses'

export default function LicensesPage() {
  const {
    products, loading, displayLicenses, submitting,
    licenseModal, setLicenseModal,
    licSearch, setLicSearch,
    licStatusFilter, setLicStatusFilter,
    setLicPage,
    licPageSize, setLicPageSize,
    selectedProductId, setSelectedProductId,
    totalItems, totalPages, activePage, startIndex,
    handleCreateLicense,
    handleEditLicense,
    handleDeleteLicense,
  } = useLicenses()

  const handleLicenseModalClose = useCallback(() => setLicenseModal(null), [setLicenseModal])
  const [detailsLicense, setDetailsLicense] = useState(null)

  if (loading) {
    return <LoadingState full message="Loading licenses..." />
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Licenses</h1>
        <button className="btn btn-primary btn-sm" onClick={() => setLicenseModal({ mode: 'create', productId: selectedProductId !== 'all' ? selectedProductId : products[0]?.id })}>+ Add License</button>
      </div>

      <LicensesFilters
        licSearch={licSearch} setLicSearch={setLicSearch}
        licStatusFilter={licStatusFilter} setLicStatusFilter={setLicStatusFilter}
        selectedProductId={selectedProductId} setSelectedProductId={setSelectedProductId}
        products={products}
        setLicPage={setLicPage}
      />

      {displayLicenses.length === 0 ? (
        <div className="card">
          <EmptyState
            title="No licenses found"
            description={licSearch || licStatusFilter !== 'all' || selectedProductId !== 'all' ? 'Try adjusting your search or filters.' : 'Create your first license to get started.'}
            action={
              (licSearch || licStatusFilter !== 'all' || selectedProductId !== 'all') ? null : (
                <button className="btn btn-primary" onClick={() => setLicenseModal({ mode: 'create', productId: products[0]?.id })}>Create License</button>
              )
            }
          />
        </div>
      ) : (
        <div>
          <LicensesTable
            displayLicenses={displayLicenses}
            setLicenseModal={setLicenseModal}
            setDetailsLicense={setDetailsLicense}
            handleDeleteLicense={handleDeleteLicense}
          />
          <LicensesPagination
            totalPages={totalPages} setLicPage={setLicPage}
            licPageSize={licPageSize} setLicPageSize={setLicPageSize}
            activePage={activePage} startIndex={startIndex} totalItems={totalItems}
          />
        </div>
      )}

      {licenseModal && (
        <LicenseFormModal
          licenseModal={licenseModal}
          handleLicenseModalClose={handleLicenseModalClose}
          handleCreateLicense={handleCreateLicense}
          handleEditLicense={handleEditLicense}
          submitting={submitting}
        />
      )}

      {detailsLicense && (
        <LicenseDetailsModal
          license={detailsLicense}
          onClose={() => setDetailsLicense(null)}
        />
      )}
    </div>
  )
}
