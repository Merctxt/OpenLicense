import { Loader2 } from 'lucide-react'

export function LoadingState({ full = false, message = 'Loading...' }) {
  if (full) {
    return (
      <div className="d-flex justify-content-center align-items-center py-5">
        <div className="text-center">
          <Loader2 className="mb-2 animate-spin text-body-secondary" size={24} />
          <p className="text-body-secondary small mb-0">{message}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="text-center py-3">
      <Loader2 className="animate-spin text-body-secondary" size={18} />
      <p className="text-body-secondary small mb-0 mt-1">{message}</p>
    </div>
  )
}
