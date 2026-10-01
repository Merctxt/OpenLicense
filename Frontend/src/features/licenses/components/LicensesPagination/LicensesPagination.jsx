export default function LicensesPagination({ totalPages, setLicPage, licPageSize, setLicPageSize, activePage, startIndex, totalItems }) {
  if (totalPages <= 1) return null

  return (
    <div className="d-flex flex-wrap justify-content-between align-items-center mt-3 pt-2 border-top gap-2">
      <span className="text-body-secondary small">
        Showing {startIndex + 1} to {Math.min(startIndex + licPageSize, totalItems)} of {totalItems} licenses
      </span>

      <div className="d-flex gap-1">
        <button
          className="btn btn-outline-secondary btn-sm"
          disabled={activePage === 1}
          onClick={() => setLicPage(prev => Math.max(1, prev - 1))}
        >
          &lsaquo; Prev
        </button>
        {Array.from({ length: totalPages }, (_, i) => i + 1).map(pageNum => (
          <button
            key={pageNum}
            className={`btn btn-sm ${activePage === pageNum ? 'btn-primary' : 'btn-outline-secondary'}`}
            onClick={() => setLicPage(pageNum)}
          >
            {pageNum}
          </button>
        ))}
        <button
          className="btn btn-outline-secondary btn-sm"
          disabled={activePage === totalPages}
          onClick={() => setLicPage(prev => Math.min(totalPages, prev + 1))}
        >
          Next &rsaquo;
        </button>
      </div>

      <select
        className="form-select form-select-sm"
        style={{ width: 'auto' }}
        value={licPageSize}
        onChange={(e) => { setLicPageSize(Number(e.target.value)); setLicPage(1); }}
      >
        <option value={5}>5 / page</option>
        <option value={10}>10 / page</option>
        <option value={20}>20 / page</option>
      </select>
    </div>
  )
}
