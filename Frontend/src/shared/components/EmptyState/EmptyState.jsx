import { AlertTriangle } from 'lucide-react'

export function EmptyState({ icon, title, description, action }) {
  const defaultIcon = icon || <AlertTriangle size={32} className="text-body-secondary mb-3" />
  const animatedIcon = (
    <div data-animate="pulse">{defaultIcon}</div>
  )

  return (
    <div className="text-center py-5">
      {animatedIcon}
      {title && <h6 className="fw-semibold mb-1">{title}</h6>}
      {description && <p className="text-body-secondary small mb-3">{description}</p>}
      {action}
    </div>
  )
}
