import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './shared/context/AuthContext'
import UseTitle from './shared/hooks/UseTitle'
import Router from './router'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <UseTitle />
        <Router />
      </BrowserRouter>
    </AuthProvider>
  )
}
