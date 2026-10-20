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
import { OrderHistoryPage } from './pages/customer/OrderHistoryPage';
import { CheckoutPage } from './pages/customer/CheckoutPage';
import { OrderSuccessPage } from './pages/customer/OrderSuccessPage';
import { OrderDetailPage } from './pages/customer/OrderDetailPage';
import { ReturnHistoryPage } from './pages/customer/ReturnHistoryPage';
import { ProfilePage } from './pages/customer/ProfilePage';
import { ChangePasswordPage } from './pages/customer/ChangePasswordPage';
import { AddressBookPage } from './pages/customer/AddressBookPage';
import { OrderManagementPage } from './pages/sales/OrderManagementPage';
import { OrderManagementDetailPage } from './pages/sales/OrderManagementDetailPage';
import { ReturnManagementPage } from './pages/sales/ReturnManagementPage';
import { RepairManagementPage } from './pages/sales/RepairManagementPage';
import { RepairRequestPage } from './pages/customer/RepairRequestPage';
import { ReviewManagementPage } from './pages/sales/ReviewManagementPage';
import { StaffProductDetailPage } from './pages/sales/StaffProductDetailPage';
import { CustomerManagementPage } from './pages/business/CustomerManagementPage';
import { StaffManagementPage } from './pages/business/StaffManagementPage';
import { BusinessDashboardPage } from './pages/business/BusinessDashboardPage';
import { ProductManagementPage } from './pages/business/ProductManagementPage';
import { CategoryManagementPage } from './pages/business/CategoryManagementPage';
import { ReportsPage } from './pages/business/ReportsPage';
import { VoucherManagementPage } from './pages/business/VoucherManagementPage';
import { TechnicalDashboardPage } from './pages/technical/TechnicalDashboardPage';
import { UserManagementPage } from './pages/technical/UserManagementPage';
import { SystemLogsPage } from './pages/technical/SystemLogsPage';
import { Button } from './components/ui/button';
import { Toaster } from 'sonner';
import { Loader2 } from 'lucide-react';
import { SalesDashboardPage } from './pages/sales/salesdashboardpage';

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
        <Route path="*" element={<Navigate to="/auth/login" replace />} />
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
            <Route path="/customer/order-success" element={<OrderSuccessPage />} />
            <Route path="/customer/orders" element={<OrderHistoryPage />} />
            <Route path="/customer/orders/:id" element={<OrderDetailPage />} />
            <Route path="/customer/returns" element={<ReturnHistoryPage />} />
            <Route path="/customer/repairs" element={<RepairRequestPage />} />
            <Route path="/customer/profile" element={<ProfilePage />} />
            <Route path="/customer/change-password" element={<ChangePasswordPage />} />
            <Route path="/customer/addresses" element={<AddressBookPage />} />
          </>
        )}

        {/* Sales Routes */}
        {user.role === 'Staff' && (
          <>
            <Route path="/sales" element={<SalesDashboardPage />} />
            <Route path="/sales/orders" element={<OrderManagementPage />} />
            <Route path="/sales/orders/:id" element={<OrderManagementDetailPage />} />
            <Route path="/sales/returns" element={<ReturnManagementPage />} />
            <Route path="/sales/repairs" element={<RepairManagementPage />} />
            <Route path="/sales/reviews" element={<ReviewManagementPage />} />
            <Route path="/sales/products" element={<ProductManagementPage readOnly={true} />} />
            <Route path="/sales/products/:id" element={<StaffProductDetailPage />} />
          </>
        )}

        {/* Business Routes */}
        {user.role === 'Business Admin' && (
          <>
            <Route path="/business" element={<BusinessDashboardPage />} />
            <Route path="/business/products" element={<ProductManagementPage />} />
            <Route path="/business/categories" element={<CategoryManagementPage />} />
            <Route path="/business/products/:id" element={<StaffProductDetailPage />} />
            <Route path="/business/customers" element={<CustomerManagementPage />} />
            <Route path="/business/staff" element={<StaffManagementPage />} />
            <Route path="/business/vouchers" element={<VoucherManagementPage />} />
            <Route path="/business/reports" element={<ReportsPage />} />
            <Route path="/business/orders/:id" element={<OrderManagementDetailPage readOnly={true} />} />
            <Route path="/business/profile" element={<ProfilePage />} />
          </>
        )}

        {/* Technical Routes */}
        {user.role === 'Technical Admin' && (
          <>
            <Route path="/technical" element={<TechnicalDashboardPage />} />
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
