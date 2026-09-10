import Sidebar from './sidebar/Sidebar'
import { Outlet } from 'react-router-dom'

export default function Layout() {
  return <Sidebar><Outlet /></Sidebar>
}
