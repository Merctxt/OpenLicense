import api from './client'
import { API_VERSION } from './version'

const apiPrefix = `/api/${API_VERSION}`

export function generateReportPdf() {
  return api.get(`${apiPrefix}/reports/generate`, {
    responseType: 'blob',
  })
}
