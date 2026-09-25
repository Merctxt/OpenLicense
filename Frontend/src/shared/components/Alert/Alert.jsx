import { useEffect, useRef } from 'react'
import { Info, CheckCircle, AlertTriangle } from 'lucide-react'
import { motion, AnimatePresence } from 'framer-motion'

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
    <motion.div
      layout
      initial={{ opacity: 0, x: 300, scale: 0.9 }}
      animate={{ opacity: 1, x: 0, scale: 1 }}
      exit={{ opacity: 0, x: 300, scale: 0.9 }}
      transition={{ type: 'spring', stiffness: 400, damping: 30, mass: 0.8 }}
      style={{ width: '360px', maxWidth: '100%', pointerEvents: 'auto', marginBottom: '0.75rem' }}
    >
      <div
        className={`alert ${bg} alert-dismissible d-flex align-items-center shadow-sm border-0 rounded-3`}
        role="alert"
      >
        <motion.div
          initial={{ scale: 0, rotate: -15 }}
          animate={{ scale: 1, rotate: 0 }}
          transition={{ type: 'spring', stiffness: 500, damping: 15, delay: 0.1 }}
        >
          <Icon size={16} className="flex-shrink-0 me-2" />
        </motion.div>
        <motion.span
          initial={{ opacity: 0, x: -10 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: 0.1 }}
          className="flex-grow-1 small"
        >
          {message}
        </motion.span>
        <motion.button
          type="button"
          className="btn-close ms-2 flex-shrink-0"
          onClick={() => onDismiss?.(id)}
          aria-label="Close"
          whileHover={{ scale: 1.2, rotate: 90 }}
          whileTap={{ scale: 0.9 }}
          style={{ pointerEvents: 'auto', opacity: 0.5 }}
        />
      </div>
    </motion.div>
  )
}
