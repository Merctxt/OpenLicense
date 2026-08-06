import { useEffect, useRef } from 'react'
import { Modal as BsModal } from 'bootstrap'

export default function Modal({ title, children, onClose, footer, size }) {
  const modalRef = useRef(null)

  useEffect(() => {
    const el = modalRef.current
    if (!el) return
    const bsModal = new BsModal(el, { backdrop: 'static', keyboard: true })
    bsModal.show()

    const handleHidden = () => onClose()
    el.addEventListener('hidden.bs.modal', handleHidden)
    return () => {
      el.removeEventListener('hidden.bs.modal', handleHidden)
      bsModal.dispose()
    }
  }, [onClose])

  return (
    <div className="modal fade" ref={modalRef} tabIndex="-1">
      <div className={`modal-dialog ${size === 'lg' ? 'modal-lg' : size === 'sm' ? 'modal-sm' : ''}`}>
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{title}</h5>
            <button type="button" className="btn-close" onClick={onClose}></button>
          </div>
          <div className="modal-body">
            {children}
          </div>
          {footer && (
            <div className="modal-footer">
              {footer}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
