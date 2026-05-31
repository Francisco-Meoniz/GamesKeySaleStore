import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getPendingWithdrawals, resolveWithdrawal } from '../../api/admin'

export default function AdminWithdrawals() {
  const qc = useQueryClient()
  const [note, setNote] = useState({})
  const [success, setSuccess] = useState('')

  const { data: withdrawals, isLoading } = useQuery({
    queryKey: ['pendingWithdrawals'],
    queryFn: getPendingWithdrawals
  })

  const resolveMutation = useMutation({
    mutationFn: ({ id, approve, adminNote }) => resolveWithdrawal(id, { approve, adminNote }),
    onSuccess: (_, { approve }) => {
      qc.invalidateQueries(['pendingWithdrawals'])
      setSuccess(approve ? 'Retiro aprobado correctamente.' : 'Retiro rechazado y saldo devuelto al vendedor.')
      setTimeout(() => setSuccess(''), 4000)
    }
  })

  if (isLoading) return <div className="admin-loading">Cargando retiros...</div>

  return (
    <div>
      {success && (
        <div className="profile-success" style={{ marginBottom: '20px' }}>{success}</div>
      )}

      {!withdrawals?.length ? (
        <div className="admin-form">
          <p style={{ color: 'var(--text-muted)' }}>No hay solicitudes de retiro pendientes.</p>
        </div>
      ) : (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Vendedor</th>
                <th>Email</th>
                <th>Monto</th>
                <th>Alias / CBU</th>
                <th>Nota (opcional)</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {withdrawals.map(w => (
                <tr key={w.id}>
                  <td style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                    {new Date(w.requestedAt).toLocaleDateString('es-AR')}
                  </td>
                  <td style={{ fontWeight: 600 }}>{w.seller}</td>
                  <td style={{ fontSize: '13px', color: 'var(--text-muted)' }}>{w.sellerEmail}</td>
                  <td style={{ fontWeight: 700, color: 'var(--green)' }}>
                    ${w.amount.toFixed(2)}
                  </td>
                  <td>
                    <span style={{
                      background: 'var(--surface)',
                      padding: '4px 10px',
                      borderRadius: '4px',
                      fontSize: '13px',
                      fontFamily: 'monospace'
                    }}>
                      {w.alias}
                    </span>
                  </td>
                  <td>
                    <input
                      type="text"
                      placeholder="Ej: Transferido ✓"
                      value={note[w.id] || ''}
                      onChange={e => setNote({ ...note, [w.id]: e.target.value })}
                      style={{ width: '160px', padding: '6px 10px', fontSize: '13px' }}
                    />
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-action unban"
                        onClick={() => resolveMutation.mutate({
                          id: w.id,
                          approve: true,
                          adminNote: note[w.id] || null
                        })}
                        disabled={resolveMutation.isPending}
                      >
                        ✓ Aprobar
                      </button>
                      <button
                        className="btn-action ban"
                        onClick={() => resolveMutation.mutate({
                          id: w.id,
                          approve: false,
                          adminNote: note[w.id] || null
                        })}
                        disabled={resolveMutation.isPending}
                      >
                        ✕ Rechazar
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}