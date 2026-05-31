import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function SellerRoute({ children }) {
  const { user } = useAuth()
  if (!user || (user.role !== 'Seller' && user.role !== 'Admin'))
    return <Navigate to="/" />
  return children
}