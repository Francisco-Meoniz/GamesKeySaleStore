import { createContext, useContext, useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getCart, addToCart, removeFromCart, clearCart } from '../api/cart'
import { useAuth } from './AuthContext'

const CartContext = createContext(null)

export function CartProvider({ children }) {
  const { user } = useAuth()
  const qc = useQueryClient()

  const { data: cart } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    enabled: !!user,
    initialData: { items: [], total: 0, count: 0 }
  })

  const addMutation = useMutation({
    mutationFn: addToCart,
    onSuccess: () => qc.invalidateQueries(['cart']),
  })

  const removeMutation = useMutation({
    mutationFn: removeFromCart,
    onSuccess: () => qc.invalidateQueries(['cart']),
  })

  const clearMutation = useMutation({
    mutationFn: clearCart,
    onSuccess: () => qc.invalidateQueries(['cart']),
  })

  return (
    <CartContext.Provider value={{
      cart,
      addToCart: addMutation.mutate,
      removeFromCart: removeMutation.mutate,
      clearCart: clearMutation.mutate,
      isAdding: addMutation.isPending,
      addError: addMutation.error?.response?.data?.error || null
    }}>
      {children}
    </CartContext.Provider>
  )
}

export const useCart = () => useContext(CartContext)