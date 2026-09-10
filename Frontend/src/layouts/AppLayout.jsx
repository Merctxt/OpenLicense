import Sidebar from '../shared/components/Sidebar/Sidebar'
import { Outlet } from 'react-router-dom'

export default function AppLayout() {
  return <Sidebar><Outlet /></Sidebar>
}
