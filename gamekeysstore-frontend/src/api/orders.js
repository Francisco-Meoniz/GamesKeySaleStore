import client from './client'

export const createOrder = (data) =>
  client.post('/orders', data).then(r => r.data)

export const createPaymentPreference = (orderId) =>
  client.post(`/payments/${orderId}/create-preference`).then(r => r.data)

export const getOrder = (orderId) =>
  client.get(`/orders/${orderId}`).then(r => r.data)

export const downloadReceipt = (orderId) =>
  client.get(`/receipt/${orderId}`, { responseType: 'blob' }).then(r => r.data)