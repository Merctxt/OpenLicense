import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './System.css'
import './components/Layout.css'
import { ThemeProvider } from './context/ThemeContext'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ThemeProvider>
      <App />
    </ThemeProvider>
  </StrictMode>,
)
