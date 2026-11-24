import { ReactNode } from 'react';
import { Sidebar } from './Sidebar';
import { TopNav } from './TopNav';

interface DashboardLayoutProps {
  role: 'Customer' | 'Staff' | 'Business Admin' | 'Technical Admin';
  userName: string;
  userRole: string;
  userAvatar?: string;
  guest?: boolean;
  onLogout: () => void;
  children: ReactNode;
}

export function DashboardLayout({
  role,
  userName,
  userRole,
  userAvatar,
  onLogout,
  children,
  guest = false,
}: DashboardLayoutProps) {
  return (
    <div className="flex h-screen w-full overflow-hidden">
      <Sidebar role={role} guest={guest} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <TopNav userName={userName} userRole={userRole} userAvatar={userAvatar} onLogout={onLogout} />
        <main className="flex-1 overflow-auto bg-background p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
