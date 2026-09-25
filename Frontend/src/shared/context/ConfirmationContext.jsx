import { createContext, useContext, useState, useCallback } from 'react'
import ConfirmationModal from '../components/ConfirmationModal/ConfirmationModal'

const ConfirmationContext = createContext(null)

export function ConfirmationProvider({ children }) {
  const [config, setConfig] = useState(null)

  const confirm = useCallback((title, message) => {
    return new Promise((resolve) => {
      setConfig({ title, message, resolve })
    })
  }, [])

  const handleConfirm = useCallback(() => {
    setConfig((c) => {
      c?.resolve(true)
      return null
    })
  }, [])

  const handleCancel = useCallback(() => {
    setConfig((c) => {
      c?.resolve(false)
      return null
    })
  }, [])

  return (
    <ConfirmationContext.Provider value={{ confirm }}>
      {children}
      {config && (
        <ConfirmationModal
          title={config.title}
          message={config.message}
          onConfirm={handleConfirm}
          onClose={handleCancel}
        />
      )}
    </ConfirmationContext.Provider>
  )
}

export function useConfirmation() {
  const ctx = useContext(ConfirmationContext)
  if (!ctx) throw new Error('useConfirmation must be used within ConfirmationProvider')
  return ctx
}
