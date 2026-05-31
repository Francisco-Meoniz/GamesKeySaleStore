import client from './client'

export const getCart = () => client.get('/cart').then(r => r.data)
export const addToCart = (gameKeyId) =>
  client.post('/cart', { gameKeyId }).then(r => r.data)
export const removeFromCart = (cartItemId) =>
  client.delete(`/cart/${cartItemId}`).then(r => r.data)
export const clearCart = () => client.delete('/cart').then(r => r.data)