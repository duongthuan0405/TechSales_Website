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
  MessageSquare,
  FolderTree,
  Ticket,
  RotateCcw,
  Wrench
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
    { icon: <LayoutDashboard className="h-5 w-5" />, label: 'Trang chủ', path: '/customer' },
    { icon: <Package className="h-5 w-5" />, label: 'Sản phẩm', path: '/customer/products' },
    { icon: <ShoppingCart className="h-5 w-5" />, label: 'Giỏ hàng', path: '/customer/cart' },
    { icon: <FileText className="h-5 w-5" />, label: 'Đơn hàng', path: '/customer/orders' },
    { icon: <RotateCcw className="h-5 w-5" />, label: 'Yêu cầu hoàn trả', path: '/customer/returns' },
    { icon: <Wrench className="h-5 w-5" />, label: 'Sửa chữa & bảo trì', path: '/customer/repairs' },
    { icon: <MapPin className="h-5 w-5" />, label: 'Sổ địa chỉ', path: '/customer/addresses' },
    { icon: <UserCircle className="h-5 w-5" />, label: 'Hồ sơ cá nhân', path: '/customer/profile' },
  ],
  Staff: [
    { icon: <LayoutDashboard className="h-5 w-5" />, label: 'Tổng quan', path: '/sales' },
    { icon: <ShoppingCart className="h-5 w-5" />, label: 'Quản lý đơn hàng', path: '/sales/orders' },
    { icon: <RotateCcw className="h-5 w-5" />, label: 'Đổi trả & Hoàn tiền', path: '/sales/returns' },
    { icon: <Wrench className="h-5 w-5" />, label: 'Sửa chữa & bảo trì', path: '/sales/repairs' },
    { icon: <MessageSquare className="h-5 w-5" />, label: 'Đánh giá', path: '/sales/reviews' },
    { icon: <Package className="h-5 w-5" />, label: 'Sản phẩm', path: '/sales/products' },
    { icon: <Users className="h-5 w-5" />, label: 'Khách hàng', path: '/sales/customers' },
  ],
  'Business Admin': [
    { icon: <LayoutDashboard className="h-5 w-5" />, label: 'Tổng quan', path: '/business' },
    { icon: <Package className="h-5 w-5" />, label: 'Quản lý sản phẩm', path: '/business/products' },
    { icon: <FolderTree className="h-5 w-5" />, label: 'Quản lý danh mục', path: '/business/categories' },
    { icon: <Users className="h-5 w-5" />, label: 'Quản lý khách hàng', path: '/business/customers' },
    { icon: <UserCircle className="h-5 w-5" />, label: 'Quản lý nhân viên', path: '/business/staff' },
    { icon: <FileText className="h-5 w-5" />, label: 'Báo cáo doanh thu', path: '/business/reports' },
    { icon: <Ticket className="h-5 w-5" />, label: 'Khuyến mãi & Voucher', path: '/business/vouchers' },
    { icon: <UserCircle className="h-5 w-5" />, label: 'Hồ sơ cá nhân', path: '/business/profile' },
  ],
  'Technical Admin': [
    { icon: <Users className="h-5 w-5" />, label: 'Quản lý người dùng', path: '/technical/users' },
    { icon: <FileText className="h-5 w-5" />, label: 'Nhật ký hệ thống', path: '/technical/logs' },
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
