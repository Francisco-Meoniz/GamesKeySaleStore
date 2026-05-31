import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getUsers, updateUserRole, toggleBanUser } from '../../api/admin'

const ROLES = ['User', 'Seller', 'Admin']

export default function AdminUsers() {
  const qc = useQueryClient()
  const { data: users, isLoading } = useQuery({ queryKey: ['adminUsers'], queryFn: getUsers })

  const roleMutation = useMutation({
    mutationFn: ({ userId, role }) => updateUserRole(userId, role),
    onSuccess: () => qc.invalidateQueries(['adminUsers'])
  })

  const banMutation = useMutation({
    mutationFn: (userId) => toggleBanUser(userId),
    onSuccess: () => qc.invalidateQueries(['adminUsers'])
  })

  if (isLoading) return <div className="admin-loading">Cargando usuarios...</div>

  return (
    <div>
      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Email</th>
              <th>Rol</th>
              <th>Ventas</th>
              <th>Registro</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {users.map(u => (
              <tr key={u.id} className={u.isBanned ? 'banned-row' : ''}>
                <td>
                  <span className="user-name">{u.username}</span>
                  {u.sellerDisplayName && (
                    <span className="seller-tag">{u.sellerDisplayName}</span>
                  )}
                  {u.isBanned && <span className="banned-tag">BANEADO</span>}
                </td>
                <td>{u.email}</td>
                <td>
                  <select
                    className="role-select"
                    value={u.role}
                    onChange={e => roleMutation.mutate({ userId: u.id, role: e.target.value })}
                  >
                    {ROLES.map(r => <option key={r} value={r}>{r}</option>)}
                  </select>
                </td>
                <td>{u.totalSales ?? '-'}</td>
                <td>{new Date(u.createdAt).toLocaleDateString('es-AR')}</td>
                <td>
                  <button
                    className={`btn-action ${u.isBanned ? 'unban' : 'ban'}`}
                    onClick={() => banMutation.mutate(u.id)}
                  >
                    {u.isBanned ? 'Desbanear' : 'Banear'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}