import Sidebar from '../shared/components/Sidebar/Sidebar'
import { Outlet } from 'react-router-dom'
import { MotionPage } from '../shared/components/Motion/Motion'

export default function AppLayout() {
  return (
    <Sidebar>
      <MotionPage>
        <Outlet />
      </MotionPage>
    </Sidebar>
  )
}
