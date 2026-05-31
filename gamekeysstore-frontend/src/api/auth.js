import client from './client'

export const register = (data) => client.post('/auth/register', {
  username: data.username,
  email: data.email,
  password: data.password,
  isSeller: data.isSeller,
  sellerDisplayName: data.sellerDisplayName,
  firstName: data.firstName,
  lastName: data.lastName,
  dni: data.dni,
  birthDate: data.birthDate,
  mercadoPagoAccount: data.mercadoPagoAccount
}).then(r => r.data)

export const login = (data) => client.post('/auth/login', data).then(r => r.data)

export const forgotPassword = (email) => client.post('/auth/forgot-password', { email }).then(r => r.data)

export const resetPassword = (token, newPassword) => client.post('/auth/reset-password', { token, newPassword }).then(r => r.data)

export const verifyEmail = (token) => client.post('/auth/verify-email', { token }).then(r => r.data)

export const resendVerification = (email) => client.post('/auth/resend-verification', { email }).then(r => r.data)