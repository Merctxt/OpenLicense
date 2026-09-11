import { Routes, Route, Navigate } from 'react-router-dom'
import UseTitle from '../shared/hooks/UseTitle'
import AppLayout from '../layouts/AppLayout'
import ProtectedRoute from '../layouts/ProtectedRoute'
import LoginPage from '../features/auth/pages/login/LoginPage'
import RegisterPage from '../features/auth/pages/register/RegisterPage'
import ForgotPasswordPage from '../features/auth/pages/forgot-password/ForgotPasswordPage'
import VerifyTokenPage from '../features/auth/pages/verify-token/VerifyTokenPage'
import ResetPasswordPage from '../features/auth/pages/reset-password/ResetPasswordPage'
import ProductsPage from '../features/products/pages/ProductsPage'
import LicensesPage from '../features/licenses/pages/LicensesPage'
import AccountPage from '../features/account/pages/AccountPage'

const registrationEnabled = import.meta.env.VITE_REGISTRATION_ENABLED !== 'false'

export default function Router() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      {registrationEnabled && <Route path="/register" element={<RegisterPage />} />}
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/verify-token" element={<VerifyTokenPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route element={<AppLayout />}>
        <Route path="/" element={<Navigate to="/products" replace />} />
        <Route path="/products" element={<ProtectedRoute><ProductsPage /></ProtectedRoute>} />
        <Route path="/licenses" element={<ProtectedRoute><LicensesPage /></ProtectedRoute>} />
        <Route path="/account" element={<ProtectedRoute><AccountPage /></ProtectedRoute>} />
      </Route>
      <Route path="*" element={<Navigate to="/products" replace />} />
    </Routes>
  )
}
