import { createContext, useContext, useCallback, useState } from 'react'
import { AlertToast } from '../components/Alert/Alert'

const AlertContext = createContext(null)

export function AlertProvider({ children }) {
  const [items, setItems] = useState([])

  const dismissAlert = useCallback((id) => {
    setItems((current) => current.filter((item) => item.id !== id))
  }, [])

  const showAlert = useCallback(({ type = 'success', message, duration = 5000 } = {}) => {
    const id = `${Date.now()}-${Math.random().toString(16).slice(2)}`
    setItems((current) => [...current, { id, type, message }])

    if (duration > 0) {
      window.setTimeout(() => dismissAlert(id), duration)
    }

    return id
  }, [dismissAlert])

  const showSuccess = useCallback((message, duration = 5000) => showAlert({ type: 'success', message, duration }), [showAlert])
  const showError = useCallback((message, duration = 5000) => showAlert({ type: 'error', message, duration }), [showAlert])
  const showInfo = useCallback((message, duration = 5000) => showAlert({ type: 'info', message, duration }), [showAlert])

  return (
    <AlertContext.Provider value={{ showAlert, showSuccess, showError, showInfo, dismissAlert }}>
      {children}
      <div className="toast-container position-fixed top-0 end-0 p-3" style={{ zIndex: 1080, pointerEvents: 'auto' }}>
        {items.map((item) => (
          <AlertToast
            key={item.id}
            id={item.id}
            type={item.type}
            message={item.message}
            onDismiss={dismissAlert}
          />
        ))}
      </div>
    </AlertContext.Provider>
  )
}

export function useAlert() {
  const ctx = useContext(AlertContext)
  if (!ctx) throw new Error('useAlert must be used within AlertProvider')
  return ctx
}
