import { useMemo } from 'react'

const PASSWORD_RULES = [
  { key: 'length', label: '8+ characters', test: pw => pw?.length >= 8 },
  { key: 'upper', label: '1 uppercase letter', test: pw => /[A-Z]/.test(pw || '') },
  { key: 'lower', label: '1 lowercase letter', test: pw => /[a-z]/.test(pw || '') },
  { key: 'digit', label: '1 number', test: pw => /[0-9]/.test(pw || '') },
  { key: 'special', label: '1 special character', test: pw => /[^A-Za-z0-9]/.test(pw || '') },
]

export function validatePassword(password) {
  const results = PASSWORD_RULES.map(rule => ({
    ...rule,
    passed: !password || rule.test(password),
  }))
  return { results, allPassed: password === '' || results.every(r => r.passed) }
}

export default function PasswordValidation({ password, showLabel = true }) {
  const { results, allPassed } = useMemo(
    () => validatePassword(password),
    [password]
  )

  if (!password) return null

  return (
    <div className="mt-2">
      {showLabel && (
        <p className="small text-body-secondary mb-1">Password must contain:</p>
      )}
      <ul className="list-unstyled small" style={{ fontSize: '0.8rem' }}>
        {results.map(rule => (
          <li key={rule.key} className={rule.passed ? 'text-success' : 'text-danger'}>
            {rule.passed ? '\u2713' : '\u2717'} {rule.label}
          </li>
        ))}
      </ul>
    </div>
  )
}
