import { ReactNode } from 'react';
import { NavLink } from 'react-router';
import {
  LayoutDashboard,
  ShoppingCart,
  Package,
  Users,
  BarChart3,
  Settings,
  FileText,
  UserCircle,
  Database,
  Shield,
  MapPin,
  Star,
  Ticket
} from 'lucide-react';
import { cn } from '../../utils/cn';

interface SidebarProps {
  role: 'Customer' | 'Staff' | 'Business Admin' | 'Technical Admin';
}

interface NavItem {
  icon: ReactNode;
  label: string;
  path: string;
}

const roleNavItems: Record<string, NavItem[]> = {
  Customer: [
    { icon: <LayoutDashboard className="h-5 w-5" />, label: 'Home', path: '/customer' },
    { icon: <Package className="h-5 w-5" />, label: 'Products', path: '/customer/products' },
    { icon: <ShoppingCart className="h-5 w-5" />, label: 'Cart', path: '/customer/cart' },
    { icon: <FileText className="h-5 w-5" />, label: 'Orders', path: '/customer/orders' },
    { icon: <MapPin className="h-5 w-5" />, label: 'Addresses', path: '/customer/addresses' },
    { icon: <UserCircle className="h-5 w-5" />, label: 'Profile', path: '/customer/profile' },
  ],
  Staff: [
    { icon: <LayoutDashboard className="h-5 w-5" />, label: 'Dashboard', path: '/sales' },
    { icon: <Package className="h-5 w-5" />, label: 'Products', path: '/sales/products' },
    { icon: <ShoppingCart className="h-5 w-5" />, label: 'Orders', path: '/sales/orders' },
    { icon: <Star className="h-5 w-5" />, label: 'Reviews', path: '/sales/reviews' },
  ],
  'Business Admin': [
    { icon: <LayoutDashboard className="h-5 w-5" />, label: 'Dashboard', path: '/business' },
    { icon: <BarChart3 className="h-5 w-5" />, label: 'Analytics', path: '/business/analytics' },
    { icon: <Package className="h-5 w-5" />, label: 'Products', path: '/business/products' },
    { icon: <ShoppingCart className="h-5 w-5" />, label: 'Orders', path: '/business/orders' },
    { icon: <Users className="h-5 w-5" />, label: 'Customers', path: '/business/customers' },
    { icon: <FileText className="h-5 w-5" />, label: 'Reports', path: '/business/reports' },
    { icon: <Ticket className="h-5 w-5" />, label: 'Vouchers', path: '/business/vouchers' },
    { icon: <Settings className="h-5 w-5" />, label: 'Settings', path: '/business/settings' },
  ],
  'Technical Admin': [
    { icon: <Users className="h-5 w-5" />, label: 'User Management', path: '/technical/users' },
    { icon: <FileText className="h-5 w-5" />, label: 'System Logs', path: '/technical/logs' },
  ],
};

export function Sidebar({ role }: SidebarProps) {
  const navItems = roleNavItems[role] || [];

  return (
    <div className="flex h-full w-64 flex-col border-r border-sidebar-border bg-sidebar">
      <div className="flex h-16 items-center border-b border-sidebar-border px-6">
        <h1 className="text-lg font-semibold text-sidebar-foreground">TechSales</h1>
      </div>
      <nav className="flex-1 space-y-1 p-4">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            end={item.path === '/customer' || item.path === '/sales' || item.path === '/business' || item.path === '/technical'}
            className={({ isActive }) => cn(
              'flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
              isActive
                ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground'
            )}
          >
            {item.icon}
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
