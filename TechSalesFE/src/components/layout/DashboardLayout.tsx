import { Outlet } from 'react-router';
import { Sidebar } from './Sidebar';
import { TopNav } from './TopNav';
import { AuthUser } from '../../models/ui_types/user';

interface DashboardLayoutProps {
  user: AuthUser;
  onLogout: () => void;
}

export function DashboardLayout({
  user,
  onLogout,
}: DashboardLayoutProps) {
  return (
    <div className="flex h-screen w-full overflow-hidden">
      <Sidebar role={user.role as any} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <TopNav userName={user.name} userRole={user.role} onLogout={onLogout} />
        <main className="flex-1 overflow-auto bg-background p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
