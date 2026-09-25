import { useEffect, useRef } from 'react'
import { Modal as BsModal } from 'bootstrap'
import { AlertTriangle } from 'lucide-react'

export default function ConfirmationModal({ title, message, onConfirm, onClose }) {
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
    <div className="modal fade" ref={modalRef} tabIndex="-1" style={{ backgroundColor: 'rgba(0, 0, 0, 0.3)' }}>
      <div className="modal-dialog modal-dialog-centered">
        <div className="modal-content">
          <div className="modal-header border-bottom">
            <h5 className="modal-title d-flex align-items-center gap-2 text-danger">
              <AlertTriangle size={18} />
              {title}
            </h5>
            <button type="button" className="btn-close" onClick={onClose}></button>
          </div>
          <div className="modal-body">
            <p className="mb-0">{message}</p>
          </div>
          <div className="modal-footer">
            <button className="btn btn-sm btn-outline-secondary" onClick={onClose}>Cancel</button>
            <button className="btn btn-sm btn-danger" onClick={onConfirm}>Delete</button>
          </div>
        </div>
      </div>
    </div>
  )
}
