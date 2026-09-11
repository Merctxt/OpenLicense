import { AlertTriangle } from 'lucide-react'

export function EmptyState({ icon, title, description, action }) {
  return (
    <div className="text-center py-5">
      {icon || <AlertTriangle size={32} className="text-body-secondary mb-3" />}
      {title && <h6 className="fw-semibold mb-1">{title}</h6>}
      {description && <p className="text-body-secondary small mb-3">{description}</p>}
      {action}
    </div>
  )
}
