import { Loader2 } from 'lucide-react'

export function LoadingState({ full = false, message = 'Loading...' }) {
  if (full) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <div className="text-center">
          <Loader2 data-animate="spin" className="mb-2 text-body-secondary" size={24} />
          <p data-animate="fade-text" className="text-body-secondary small mb-0">{message}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="text-center py-3">
      <Loader2 data-animate="spin" className="text-body-secondary" size={18} />
      <p data-animate="fade-text" className="text-body-secondary small mb-0 mt-1">{message}</p>
    </div>
  )
}
