import { useEffect } from 'react'

export function useMountEffect(fn) {
  useEffect(() => {
    fn()
  }, [])
}
