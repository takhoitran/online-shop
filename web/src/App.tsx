import { Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout';
import { AdminLayout } from './components/AdminLayout';
import { RequireAuth } from './auth/RequireAuth';
import { RequireBuyer } from './auth/RequireBuyer';
import { RequireAdmin, RequireStaff } from './auth/RequireStaff';
import { HomePage } from './pages/HomePage';
import { ProductListingPage } from './pages/ProductListingPage';
import { ProductDetailPage } from './pages/ProductDetailPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { CartPage } from './pages/CartPage';
import { WishlistPage } from './pages/WishlistPage';
import { CheckoutPage } from './pages/CheckoutPage';
import { CheckoutSuccessPage } from './pages/CheckoutSuccessPage';
import { OrdersPage } from './pages/OrdersPage';
import { OrderDetailPage } from './pages/OrderDetailPage';
import { TelegramLinkPage } from './pages/TelegramLinkPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { AdminOrdersPage } from './pages/admin/AdminOrdersPage';
import { AdminOrderDetailPage } from './pages/admin/AdminOrderDetailPage';
import { AdminProductsPage } from './pages/admin/AdminProductsPage';
import { AdminProductFormPage } from './pages/admin/AdminProductFormPage';
import { AdminCategoriesPage } from './pages/admin/AdminCategoriesPage';
import { AdminInventoryPage } from './pages/admin/AdminInventoryPage';
import { AdminUsersPage } from './pages/admin/AdminUsersPage';
import { AdminAiReportPage } from './pages/admin/AdminAiReportPage';
import { AdminVouchersPage } from './pages/admin/AdminVouchersPage';
import { ProfilePage } from './pages/ProfilePage';
import { DealsPage } from './pages/DealsPage';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="deals" element={<DealsPage />} />
        <Route path="products" element={<ProductListingPage />} />
        <Route path="categories/:categoryId" element={<ProductListingPage />} />
        <Route path="products/:id" element={<ProductDetailPage />} />
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />

        <Route element={<RequireAuth />}>
          <Route path="profile" element={<ProfilePage />} />
        </Route>

        <Route element={<RequireBuyer />}>
          <Route path="cart" element={<CartPage />} />
          <Route path="wishlist" element={<WishlistPage />} />
          <Route path="checkout" element={<CheckoutPage />} />
          <Route path="checkout/success" element={<CheckoutSuccessPage />} />
          <Route path="orders" element={<OrdersPage />} />
          <Route path="orders/:id" element={<OrderDetailPage />} />
          <Route path="telegram-link" element={<TelegramLinkPage />} />
        </Route>

        <Route path="admin" element={<RequireStaff />}>
          <Route element={<AdminLayout />}>
            <Route index element={<AdminOrdersPage />} />
            <Route path="orders" element={<AdminOrdersPage />} />
            <Route path="orders/:id" element={<AdminOrderDetailPage />} />
            <Route path="products" element={<AdminProductsPage />} />
            <Route path="products/new" element={<AdminProductFormPage />} />
            <Route path="products/:id" element={<AdminProductFormPage />} />
            <Route path="categories" element={<AdminCategoriesPage />} />
            <Route path="inventory" element={<AdminInventoryPage />} />
            <Route path="ai-report" element={<AdminAiReportPage />} />
            <Route path="vouchers" element={<AdminVouchersPage />} />
            <Route element={<RequireAdmin />}>
              <Route path="users" element={<AdminUsersPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
