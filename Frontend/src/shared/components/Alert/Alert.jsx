import { useEffect } from 'react'
import { Info, CheckCircle, AlertTriangle } from 'lucide-react'

export const alertConfig = {
  success: { bg: 'alert-success', icon: CheckCircle },
  error: { bg: 'alert-danger', icon: AlertTriangle },
  info: { bg: 'alert-info', icon: Info },
}

export default function Alert({ type = 'success', message, onDismiss }) {
  useEffect(() => {
    const timer = setTimeout(() => onDismiss?.(), 5000)
    return () => clearTimeout(timer)
  }, [onDismiss])

  const { bg, icon: Icon } = alertConfig[type] || alertConfig.success

  return (
    <div className={`alert ${bg} alert-dismissible fade show d-flex align-items-center`}>
      <Icon size={14} className="flex-shrink-0 me-2" />
      <span className="flex-grow-1">{message}</span>
      <button type="button" className="btn-close" onClick={onDismiss} aria-label="Close" style={{ marginRight: '-2px' }} />
    </div>
  )
}

export function AlertToast({ id, type = 'success', message, onDismiss }) {
  const { bg, icon: Icon } = alertConfig[type] || alertConfig.success

  return (
    <div
      className={`alert ${bg} alert-dismissible fade show d-flex align-items-center shadow-sm`}
      role="alert"
      style={{ maxWidth: '360px', width: '100%', pointerEvents: 'auto' }}
    >
      <Icon size={14} className="flex-shrink-0 me-2" />
      <span className="flex-grow-1">{message}</span>
      <button
        type="button"
        className="btn-close"
        onClick={() => onDismiss?.(id)}
        aria-label="Close"
        style={{ marginRight: '-2px', pointerEvents: 'auto' }}
      />
    </div>
  )
}
