import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { AuthProvider } from './context/AuthContext'
import { CartProvider } from './context/CartContext'
import Navbar from './components/navbar.jsx'
import Home from './pages/Home/Home'
import Cart from './pages/Cart/Cart'
import CartButton from './components/CartButton'
import Login from './pages/Auth/Login'
import Register from './pages/Auth/Register'
import ForgotPassword from './pages/Auth/ForgotPassword'
import ResetPassword from './pages/Auth/ResetPassword'
import GameDetail from './pages/Game/GameDetail'
import Checkout from './pages/Checkout/Checkout'
import PaymentSuccess from './pages/Checkout/PaymentSuccess'
import PaymentFailure from './pages/Checkout/PaymentFailure'
import PaymentPending from './pages/Checkout/PaymentPending'
import AdminDashboard from './pages/Admin/AdminDashboard'
import AdminRoute from './components/AdminRoute'
import SellerDashboard from './pages/Seller/SellerDashboard'
import SellerRoute from './components/SellerRoute'
import Profile from './pages/Profile/Profile'
import VerifyEmail from './pages/Auth/VerifyEmail'
import PendingVerification from './pages/Auth/PendingVerification'

const queryClient = new QueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <CartProvider>
        <BrowserRouter>
          <Navbar />
          <CartButton />
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/cart" element={<Cart />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />
            <Route path="/games/:id" element={<GameDetail />} />
            <Route path="/checkout" element={<Checkout />} />
            <Route path="/payment/success" element={<PaymentSuccess />} />
            <Route path="/payment/failure" element={<PaymentFailure />} />
            <Route path="/payment/pending" element={<PaymentPending />} />
            <Route path="/admin" element={
              <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />
            <Route path="/seller/dashboard" element={
              <SellerRoute>
                <SellerDashboard />
              </SellerRoute>
            } />
            <Route path="/profile" element={<Profile />} />
            <Route path="/verify-email" element={<VerifyEmail />} />
            <Route path="/pending-verification" element={<PendingVerification />} />  
          </Routes>
        </BrowserRouter>
        </CartProvider>
      </AuthProvider>
    </QueryClientProvider>
  )
}