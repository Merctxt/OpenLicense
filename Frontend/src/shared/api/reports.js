import api from './client'

export function updateReportPreferences(reportsOptIn) {
  return api.put('/api/auth/report-preferences', { reportsOptIn })
}
