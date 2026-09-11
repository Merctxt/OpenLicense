import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './shared/context/AuthContext'
import { AlertProvider } from './shared/context/AlertContext'
import UseTitle from './shared/hooks/UseTitle'
import Router from './router'

export default function App() {
  return (
    <AlertProvider>
      <AuthProvider>
        <BrowserRouter>
          <UseTitle />
          <Router />
        </BrowserRouter>
      </AuthProvider>
    </AlertProvider>
  )
}
