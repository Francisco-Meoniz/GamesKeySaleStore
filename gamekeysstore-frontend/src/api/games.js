import client from './client'

export const getGames = () => client.get('/games').then(r => r.data)
export const getGameById = (id) => client.get(`/games/${id}`).then(r => r.data)
export const getGameKeys = (id) => client.get(`/games/${id}/keys`).then(r => r.data)