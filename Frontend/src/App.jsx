import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './shared/context/AuthContext'
import { AlertProvider } from './shared/context/AlertContext'
import { ConfirmationProvider } from './shared/context/ConfirmationContext'
import Router from './router'

export default function App() {
  return (
    <ConfirmationProvider>
      <AlertProvider>
        <AuthProvider>
          <BrowserRouter>
            <Router />
          </BrowserRouter>
        </AuthProvider>
      </AlertProvider>
    </ConfirmationProvider>
  )
}
