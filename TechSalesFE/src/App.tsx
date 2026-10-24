import { Routes, Route, Navigate } from 'react-router';
import { AuthProvider, useAuth } from './context/AuthContext';
import { LoginPage } from './pages/auth/LoginPage';
import { SignUpPage } from './pages/auth/SignUpPage';
import { ForgotPasswordPage } from './pages/auth/ForgotPasswordPage';
import { DashboardLayout } from './components/layout/DashboardLayout';
import { ProductCatalogPage } from './pages/customer/ProductCatalogPage';
import { ProductDetailPage } from './pages/customer/ProductDetailPage';
import { HomePage } from './pages/customer/HomePage';
import { ShoppingCartPage } from './pages/customer/ShoppingCartPage';
import { OrderHistoryPage } from './pages/customer/OrderHistoryPage';
import { CheckoutPage } from './pages/customer/CheckoutPage';
import { OrderSuccessPage } from './pages/customer/OrderSuccessPage';
import { OrderDetailPage } from './pages/customer/OrderDetailPage';
import { ProfilePage } from './pages/customer/ProfilePage';
import { ChangePasswordPage } from './pages/customer/ChangePasswordPage';
import { AddressBookPage } from './pages/customer/AddressBookPage';
import { TechnicalDashboardPage } from './pages/technical/TechnicalDashboardPage';
import { UserManagementPage } from './pages/technical/UserManagementPage';
import { OrderManagementDetailPage } from './pages/sales/OrderManagementDetailPage';
import { ReviewManagementPage } from './pages/sales/ReviewManagementPage';
import { Toaster } from 'sonner';

import { Loader2 } from 'lucide-react';
import { ResetPasswordPage } from './pages/auth/ResetPasswordPage';
import { VerifyEmailPage } from './pages/auth/VerifyEmailPage';
import { ProductManagementPage } from './pages/business/ProductManagementPage';
import { SalesDashboardPage } from './pages/sales/SalesDashboardPage';
import { OrderManagementPage } from './pages/sales/OrderManagementPage';
import { BusinessDashboardPage } from './pages/business/BusinessDashboardPage';

function AppContent() {
  const { user, isLoading, logout } = useAuth();

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <>
      <Toaster position="top-right" />
      <Routes>
        {/* Public Routes */}
        {!user ? (
          <>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/signup" element={<SignUpPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
            <Route path="/verify-email" element={<VerifyEmailPage />} />
            <Route path="*" element={<Navigate to="/login" replace />} />
          </>
        ) : (
          <Route element={<DashboardLayout user={user} onLogout={logout} />}>
            {/* Common Routes */}
            <Route path="/" element={<Navigate to={
              user.role === 'Customer' ? '/customer' : 
              user.role === 'Staff' ? '/sales' : 
              user.role === 'Business Admin' ? '/business' : 
              '/technical'
            } replace />} />

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
                <Route path="/customer/profile" element={<ProfilePage />} />
                <Route path="/customer/change-password" element={<ChangePasswordPage />} />
                <Route path="/customer/addresses" element={<AddressBookPage />} />
              </>
            )}

            {/* Sales Routes */}
            {user.role === 'Staff' && (
              <>
                <Route path="/sales" element={<SalesDashboardPage />} />
                <Route path="/sales/products" element={<ProductCatalogPage />} />
                <Route path="/sales/products/:id" element={<ProductDetailPage />} />
                <Route path="/sales/orders" element={<OrderManagementPage />} />
                <Route path="/sales/orders/:id" element={<OrderManagementDetailPage />} />
                <Route path="/sales/reviews" element={<ReviewManagementPage />} />
              </>
            )}

            {/* Business Routes */}
            {user.role === 'Business Admin' && (
              <>
                <Route path="/business" element={<BusinessDashboardPage />} />
                <Route path="/business/products" element={<ProductManagementPage />} />
              </>
            )}

            {user.role === 'Technical Admin' && (
              <>
                <Route path="/technical" element={<TechnicalDashboardPage />} />
                <Route path="/technical/users" element={<UserManagementPage />} />
              </>
            )}

            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        )}
      </Routes>
    </>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}
