export default function LicensesFilters({ licSearch, setLicSearch, licStatusFilter, setLicStatusFilter, selectedProductId, setSelectedProductId, products, setLicPage }) {
  return (
    <div className="d-flex flex-wrap gap-2 mb-3 bg-body-tertiary p-3 rounded border">
      <div className="flex-grow-1 position-relative">
        <input
          type="text"
          className="form-control form-control-sm"
          placeholder="Search by name or key..."
          value={licSearch}
          onChange={(e) => { setLicSearch(e.target.value); setLicPage(1); }}
        />
        {licSearch && (
          <button className="btn btn-sm position-absolute end-0 top-50 translate-middle-y border-0 bg-transparent text-body-secondary" onClick={() => { setLicSearch(''); setLicPage(1); }}>&times;</button>
        )}
      </div>
      <div className="d-flex align-items-center gap-2">
        <label className="form-label mb-0 small text-nowrap">Product:</label>
        <select
          className="form-select form-select-sm"
          style={{ width: '200px' }}
          value={selectedProductId}
          onChange={(e) => { setSelectedProductId(e.target.value); setLicPage(1); }}
        >
          <option value="all">All Products</option>
          {products.map(p => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>
      <div className="d-flex align-items-center gap-2">
        <label className="form-label mb-0 small text-nowrap">Status:</label>
        <select
          className="form-select form-select-sm"
          style={{ width: '130px' }}
          value={licStatusFilter}
          onChange={(e) => { setLicStatusFilter(e.target.value); setLicPage(1); }}
        >
          <option value="all">All</option>
          <option value="active">Active</option>
          <option value="suspended">Suspended</option>
        </select>
      </div>
    </div>
  )
}
