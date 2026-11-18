import { createContext, useContext, useState, useEffect, useCallback } from 'react'

const ThemeContext = createContext(null)

function applyTheme(theme) {
  const isDark = theme === 'system'
    ? window.matchMedia('(prefers-color-scheme: dark)').matches
    : theme === 'dark'

  document.documentElement.setAttribute('data-bs-theme', isDark ? 'dark' : 'light')
}

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState(() => {
    return localStorage.getItem('ol_theme') || 'system'
  })

  useEffect(() => {
    localStorage.setItem('ol_theme', theme)
  }, [theme])

  const toggleTheme = useCallback((target, event) => {
    return new Promise((resolve) => {
      const x = event?.clientX ?? window.innerWidth / 2
      const y = event?.clientY ?? window.innerHeight / 2

      if (!document.startViewTransition) {
        applyTheme(target)
        localStorage.setItem('ol_theme', target)
        setTheme(target)
        resolve()
        return
      }

      const transition = document.startViewTransition(() => {
        applyTheme(target)
      })

      localStorage.setItem('ol_theme', target)
      setTheme(target)

      transition.ready.then(() => {
        const maxRadius = Math.hypot(
          Math.max(x, window.innerWidth - x),
          Math.max(y, window.innerHeight - y)
        )

        document.documentElement.animate(
          {
            clipPath: [
              `circle(0px at ${x}px ${y}px)`,
              `circle(${maxRadius}px at ${x}px ${y}px)`
            ]
          },
          {
            duration: 500,
            easing: 'ease-in-out',
            pseudoElement: '::view-transition-new(root)'
          }
        )
      })

      resolve()
    })
  }, [])

  return (
    <ThemeContext.Provider value={{ theme, setTheme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  )
}

export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme must be used within ThemeProvider')
  return ctx
}
