import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './System.css'
import './components/Layout.css'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
