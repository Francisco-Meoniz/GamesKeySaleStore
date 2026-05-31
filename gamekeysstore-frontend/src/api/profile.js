import client from './client'

export const getProfile = () => client.get('/profile').then(r => r.data)

export const updateProfile = (data) => client.put('/profile', {
  username: data.username,
  email: data.email,
  sellerDisplayName: data.sellerDisplayName,
  // 👇 NUEVOS CAMPOS
  firstName: data.firstName,
  lastName: data.lastName,
  dni: data.dni,
  birthDate: data.birthDate,
  mercadoPagoAccount: data.mercadoPagoAccount
}).then(r => r.data)

export const changePassword = (data) => client.put('/profile/change-password', data).then(r => r.data)

export const uploadAvatar = (file) => {
  const formData = new FormData()
  formData.append('file', file)
  return client.post('/profile/avatar', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  }).then(r => r.data)
}

export const deleteAccount = () => client.delete('/profile/delete-account').then(r => r.data)

export const getPurchaseHistory = () => client.get('/profile/purchases').then(r => r.data)

export const downloadReceipt = (orderId) => client.get(`/receipt/${orderId}`, { responseType: 'blob' }).then(r => r.data)