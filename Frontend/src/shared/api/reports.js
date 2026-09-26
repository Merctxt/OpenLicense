import api from './client'

export function updateReportPreferences(reportsOptIn) {
  return api.put('/api/v1/auth/report-preferences', { reportsOptIn })
}
