import client from './client'

export const getSellerStats = () => client.get('/seller/stats').then(r => r.data)
export const getMyKeys = () => client.get('/seller/keys').then(r => r.data)
export const publishKey = (data) => client.post('/seller/keys', data).then(r => r.data)
export const toggleKey = (keyId) => client.put(`/seller/keys/${keyId}/toggle`).then(r => r.data)
export const updateKeyPrice = (keyId, price) =>
  client.put(`/seller/keys/${keyId}/price`, { price }).then(r => r.data)
export const deleteKey = (keyId) => client.delete(`/seller/keys/${keyId}`).then(r => r.data)
export const getBalance = () => client.get('/withdrawal/balance').then(r => r.data)
export const requestWithdrawal = (amount) => client.post('/withdrawal/request', { amount }).then(r => r.data)