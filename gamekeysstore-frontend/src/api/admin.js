import client from './client'

export const getStats = () => client.get('/admin/stats').then(r => r.data)
export const getUsers = () => client.get('/admin/users').then(r => r.data)
export const updateUserRole = (userId, role) =>
  client.put(`/admin/users/${userId}/role`, { role }).then(r => r.data)
export const toggleBanUser = (userId) =>
  client.put(`/admin/users/${userId}/ban`).then(r => r.data)
export const getOrders = () => client.get('/admin/orders').then(r => r.data)
export const createGame = (data) => client.post('/admin/games', data).then(r => r.data)
export const updateGame = (id, data) => client.put(`/admin/games/${id}`, data).then(r => r.data)
export const deleteGame = (id) => client.delete(`/admin/games/${id}`).then(r => r.data)
export const getPendingWithdrawals = () => client.get('/withdrawal/admin/pending').then(r => r.data)
export const resolveWithdrawal = (id, data) => client.post(`/withdrawal/admin/resolve/${id}`, data).then(r => r.data)