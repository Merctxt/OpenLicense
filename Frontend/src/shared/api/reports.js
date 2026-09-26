import api from './client'
import { API_VERSION } from './version'

const apiPrefix = `/api/${API_VERSION}`

export function updateReportPreferences(reportsOptIn) {
  return api.put(`${apiPrefix}/auth/report-preferences`, { reportsOptIn })
}
