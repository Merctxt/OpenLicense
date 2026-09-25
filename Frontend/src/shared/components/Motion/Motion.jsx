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

export function MotionStaggerList({ children, delay = 0.04 }) {
  return (
    <motion.div
      initial="hidden"
      animate="visible"
      variants={{
        hidden: { opacity: 0 },
        visible: {
          opacity: 1,
          transition: { staggerChildren: delay, delayChildren: 0.1 },
        },
      }}
    >
      {children}
    </motion.div>
  )
}

export function MotionStaggerItem({ children, delay }) {
  return (
    <motion.div
      variants={itemVariants}
      style={delay != null ? { transitionDelay: `${delay}s` } : undefined}
    >
      {children}
    </motion.div>
  )
}

export function MotionBadge({ children, delay }) {
  return (
    <motion.span
      initial={{ scale: 0.8, opacity: 0 }}
      animate={{ scale: 1, opacity: 1 }}
      transition={{ type: 'spring', stiffness: 500, damping: 20, delay: delay || 0 }}
    >
      {children}
    </motion.span>
  )
}

export function MotionButton({ children, destructive, ...props }) {
  return (
    <motion.button
      whileHover={{ scale: destructive ? 1.02 : 1.03 }}
      whileTap={{ scale: 0.97 }}
      style={{ transition: 'none' }}
      {...props}
    >
      {children}
    </motion.button>
  )
}
