import client from './client'

export const getSellerRatings = (sellerProfileId) =>
  client.get(`/ratings/seller/${sellerProfileId}`).then(r => r.data)

export const canRate = (orderId, sellerProfileId) =>
  client.get(`/ratings/can-rate`, { params: { orderId, sellerProfileId } }).then(r => r.data)

export const createRating = (data) =>
  client.post('/ratings', data).then(r => r.data)