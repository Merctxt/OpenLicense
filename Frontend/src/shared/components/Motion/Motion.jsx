import { AnimatePresence, motion } from 'framer-motion'
import { useLocation } from 'react-router-dom'

const pageTransition = {
  type: 'tween',
  ease: [0.16, 1, 0.3, 1],
  duration: 0.3,
}

export function MotionPage({ children }) {
  const location = useLocation()
  return (
    <AnimatePresence mode="wait">
      <motion.div
        key={location.pathname}
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -6 }}
        transition={pageTransition}
      >
        {children}
      </motion.div>
    </AnimatePresence>
  )
}
