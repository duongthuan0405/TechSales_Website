import { UnavailableFeature } from './components/UnavailableFeature';
import { ShippingFeeManagementPage } from './pages/business/ShippingFeeManagementPage';

import { Routes, Route, Navigate, useNavigate } from 'react-router';
import { AuthProvider, useAuth } from './context/AuthContext';
import { LoginPage } from './pages/auth/LoginPage';
import { SignUpPage } from './pages/auth/SignUpPage';
import { ForgotPasswordPage } from './pages/auth/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/auth/ResetPasswordPage';
import { VerifyEmailPage } from './pages/auth/VerifyEmailPage';
import { DashboardLayout } from './components/layout/DashboardLayout';
import { ProductCatalogPage } from './pages/customer/ProductCatalogPage';
import { ProductDetailPage } from './pages/customer/ProductDetailPage';
import { HomePage } from './pages/customer/HomePage';
import { ShoppingCartPage } from './pages/customer/ShoppingCartPage';
import { CheckoutPage } from './pages/customer/CheckoutPage';
import { ProfilePage } from './pages/customer/ProfilePage';

import { AddressBookPage } from './pages/customer/AddressBookPage';
import { StaffProductDetailPage } from './pages/sales/StaffProductDetailPage';
import { CustomerManagementPage } from './pages/business/CustomerManagementPage';
import { StaffManagementPage } from './pages/business/StaffManagementPage';
import { ProductManagementPage } from './pages/business/ProductManagementPage';
import { CatalogManagementPage } from './pages/business/CatalogManagementPage';
import { VoucherManagementPage } from './pages/business/VoucherManagementPage';
import { TechnicalDashboardPage } from './pages/technical/TechnicalDashboardPage';
import { UserManagementPage } from './pages/technical/UserManagementPage';
import { SystemLogsPage } from './pages/technical/SystemLogsPage';
import { Button } from './components/ui/button';
import { Toaster } from 'sonner';
import { Loader2 } from 'lucide-react';

function AppContent() {
  const { user, isLoading, logout } = useAuth();
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="flex h-screen w-full items-center justify-center bg-background">
        <Loader2 className="h-10 w-10 animate-spin text-primary" />
      </div>
    );
  }

  if (!user) {
    return (
      <Routes>
        <Route path="/auth/login" element={<LoginPage />} />
        <Route path="/auth/signup" element={<SignUpPage />} />
        <Route path="/auth/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/auth/reset-password" element={<ResetPasswordPage />} />
        <Route path="/auth/verify-email" element={<VerifyEmailPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/verify-email" element={<VerifyEmailPage />} />
        {/* Convenience Redirects */}
        <Route path="/login" element={<Navigate to="/auth/login" replace />} />
        <Route path="/signup" element={<Navigate to="/auth/signup" replace />} />
        <Route path="/forgot-password" element={<Navigate to="/auth/forgot-password" replace />} />
        <Route path="*" element={
          <DashboardLayout role="Customer" userName="Khách" userRole="" onLogout={logout} guest>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/customer" element={<HomePage />} />
              <Route path="/customer/products" element={<ProductCatalogPage />} />
              <Route path="/customer/products/:id" element={<ProductDetailPage />} />
              <Route path="*" element={<Navigate to="/auth/login" replace />} />
            </Routes>
          </DashboardLayout>
        } />
      </Routes>
    );
  }

  const roleBasedDefaultPath: Record<string, string> = {
    Customer: '/customer',
    Staff: '/sales',
    'Business Admin': '/business',
    'Technical Admin': '/technical/users',
  };

  const getRoleDisplayName = (role: string) => {
    const roleNames: Record<string, string> = {
      Customer: 'Khách hàng',
      Staff: 'Nhân viên bán hàng',
      'Business Admin': 'Quản trị kinh doanh',
      'Technical Admin': 'Quản trị kỹ thuật',
    };
    return roleNames[role] || role;
  };

  return (
    <DashboardLayout
      role={user.role}
      userName={user.name}
      userRole={getRoleDisplayName(user.role)}
      userAvatar={user.avatarUrl}
      onLogout={logout}
    >
      <Routes>
        {/* Default Redirect */}
        <Route path="/" element={<Navigate to={roleBasedDefaultPath[user.role]} replace />} />

        {/* Redirect auth pages to home if already logged in */}
        <Route path="/auth/*" element={<Navigate to="/" replace />} />

        {/* Customer Routes */}
        {user.role === 'Customer' && (
          <>
            <Route path="/customer" element={<HomePage />} />
            <Route path="/customer/products" element={<ProductCatalogPage />} />
            <Route path="/customer/products/:id" element={<ProductDetailPage />} />
            <Route path="/customer/cart" element={<ShoppingCartPage />} />
            <Route path="/customer/checkout" element={<CheckoutPage />} />
            <Route path="/customer/order-success" element={<UnavailableFeature title="Đặt hàng" />} />
            <Route path="/customer/orders" element={<UnavailableFeature title="Quản lý đơn hàng" />} />
            <Route path="/customer/orders/:id" element={<UnavailableFeature title="Chi tiết đơn hàng" />} />
            <Route path="/customer/returns" element={<UnavailableFeature title="Yêu cầu hoàn trả" />} />
            <Route path="/customer/repairs" element={<UnavailableFeature title="Sửa chữa" />} />
            <Route path="/customer/profile" element={<ProfilePage />} />
            <Route path="/customer/change-password" element={<Navigate to="/customer/profile" replace />} />
            <Route path="/customer/addresses" element={<AddressBookPage />} />
          </>
        )}

        {/* Sales Routes */}
        {user.role === 'Staff' && (
          <>
            <Route path="/sales" element={<UnavailableFeature title="Tổng quan" />} />
            <Route path="/sales/profile" element={<ProfilePage />} />
            <Route path="/sales/change-password" element={<Navigate to="/sales/profile" replace />} />
            <Route path="/sales/inventory" element={<Navigate to="/sales/products" replace />} />
            <Route path="/sales/orders" element={<UnavailableFeature title="Quản lý đơn hàng" />} />
            <Route path="/sales/orders/:id" element={<UnavailableFeature title="Chi tiết đơn hàng" />} />
            <Route path="/sales/returns" element={<UnavailableFeature title="Quản lý hoàn trả" />} />
            <Route path="/sales/repairs" element={<UnavailableFeature title="Quản lý sửa chữa" />} />
            <Route path="/sales/reviews" element={<UnavailableFeature title="Quản lý đánh giá" />} />
            <Route path="/sales/products" element={<ProductManagementPage readOnly={true} />} />
            <Route path="/sales/products/:id" element={<StaffProductDetailPage />} />
            <Route path="/sales/customers" element={<CustomerManagementPage />} />
          </>
        )}

        {/* Business Routes */}
        {user.role === 'Business Admin' && (
          <>
            <Route path="/business" element={<UnavailableFeature title="Tổng quan" />} />
            <Route path="/business/products" element={<CatalogManagementPage />} />
            <Route path="/business/shipping-fees" element={<ShippingFeeManagementPage />} />
            <Route path="/business/inventory" element={<Navigate to="/business/products" replace />} />
            <Route path="/business/categories" element={<Navigate to="/business/products?tab=categories" replace />} />
            <Route path="/business/products/:id" element={<StaffProductDetailPage />} />
            <Route path="/business/customers" element={<CustomerManagementPage />} />
            <Route path="/business/staff" element={<StaffManagementPage />} />
            <Route path="/business/vouchers" element={<VoucherManagementPage />} />
            <Route path="/business/reports" element={<UnavailableFeature title="Báo cáo" />} />
            <Route path="/business/orders/:id" element={<UnavailableFeature title="Chi tiết đơn hàng" />} />
            <Route path="/business/profile" element={<ProfilePage />} />
            <Route path="/business/change-password" element={<Navigate to="/business/profile" replace />} />
          </>
        )}

        {/* Technical Routes */}
        {user.role === 'Technical Admin' && (
          <>
            <Route path="/technical" element={<TechnicalDashboardPage />} />
            <Route path="/technical/profile" element={<ProfilePage />} />
            <Route path="/technical/change-password" element={<Navigate to="/technical/profile" replace />} />
            <Route path="/business/categories" element={<Navigate to="/business/products?tab=categories" replace />} />
            <Route path="/business/products" element={<CatalogManagementPage />} />
            <Route path="/business/shipping-fees" element={<ShippingFeeManagementPage />} />
            <Route path="/business/inventory" element={<Navigate to="/business/products" replace />} />
            <Route path="/business/products/:id" element={<StaffProductDetailPage />} />
            <Route path="/technical/users" element={<UserManagementPage />} />
            <Route path="/technical/logs" element={<SystemLogsPage />} />
          </>
        )}

        {/* Catch all */}
        <Route path="*" element={
          <div className="flex h-full items-center justify-center">
            <div className="text-center">
              <h2 className="text-2xl font-bold">Không Tìm Thấy Trang</h2>
              <p className="text-muted-foreground">Trang bạn đang tìm kiếm không tồn tại hoặc đã được di chuyển.</p>
              <Button 
                className="mt-4" 
                onClick={() => navigate(roleBasedDefaultPath[user.role])}
              >
                Quay Lại Trang Chủ
              </Button>
            </div>
          </div>
        } />
      </Routes>
    </DashboardLayout>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
      <Toaster richColors position="top-right" />
    </AuthProvider>
  );
}
