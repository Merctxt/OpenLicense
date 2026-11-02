export const MESSAGES = {
  auth: {
    invalidCredentials: 'Invalid credentials',
    accountCreated: 'Account created successfully! Please sign in.',
    passwordReset: 'Password reset successfully! Please sign in.',
    passwordResetSuccess: 'Token verified! Redirecting...',
    registrationFailed: 'Registration failed',
    emailChangeRequired: 'Please check your email for a confirmation link to complete the email change.',
    emailChangesLimit: 'You have reached the maximum number of allowed email changes. Please contact support.',
  },
  products: {
    loadFailed: 'Failed to load products',
    created: 'Product created',
    updated: 'Product updated',
    deleted: 'Product deleted',
    createFailed: 'Failed to create product',
    updateFailed: 'Failed to update product',
    deleteFailed: 'Failed to delete product',
    limitReached: null,
  },
  licenses: {
    loadFailed: 'Failed to load licenses',
    created: 'License created successfully!',
    updated: 'License updated',
    deleted: 'License deleted',
    createFailed: 'Failed to create license',
    updateFailed: 'Failed to update license',
    deleteFailed: 'Failed to delete license',
  },
  activations: {
    loadFailed: 'Failed to load activations',
    removed: 'Activation removed',
    toggled: (action) => `Activation ${action}d`,
    toggleFailed: (action) => `Failed to ${action} activation`,
  },
  account: {
    profileUpdated: 'Profile updated',
    accountDeleted: 'Account deleted',
    deleteFailed: 'Failed to delete account',
    apiKeyCreated: 'API key created!',
    apiKeyDeleted: 'API key deleted',
    apikeyCreateFailed: 'Failed to create API key',
    apikeyToggleFailed: (action) => `Failed to ${action} API key`,
    apikeyLimitReached: null,
    reportGenerated: 'Report generated successfully!',
  },
  recovery: {
    emailFailed: 'Failed to send recovery email',
    invalidToken: 'Invalid token',
    resetFailed: 'Failed to reset password',
  },
}

function getMessage(key, fallback) {
  const msg = key instanceof Function ? key() : MESSAGES[key]
  if (msg === null) return fallback
  return msg || fallback
}

function normalizeErrorMessage(err) {
  if (err?.response?.data?.message) return err.response.data.message
  if (err?.response?.data?.error?.join) return err.response.data.error.join(' ')
  return null
}

export function handleError(err, key, fallback) {
  const apiMsg = normalizeErrorMessage(err)
  return apiMsg || getMessage(key, fallback)
}
