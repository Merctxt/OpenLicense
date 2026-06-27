export default function useDocs() {
  const scalarUrl = import.meta.env.VITE_API_URL + '/scalar/v1'
  const apiUrl = import.meta.env.VITE_API_URL

  return { scalarUrl, apiUrl }
}
