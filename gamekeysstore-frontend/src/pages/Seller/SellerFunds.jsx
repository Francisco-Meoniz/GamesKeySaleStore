import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getBalance, requestWithdrawal } from '../../api/seller'
import { getProfile } from '../../api/profile'

export default function SellerFunds() {
  const qc = useQueryClient()
  const [amount, setAmount] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const { data: balance, isLoading } = useQuery({
    queryKey: ['sellerBalance'],
    queryFn: getBalance
  })

  const { data: profile } = useQuery({
    queryKey: ['profile'],
    queryFn: getProfile
  })

  const withdrawMutation = useMutation({
    mutationFn: () => requestWithdrawal(parseFloat(amount)),
    onSuccess: () => {
      qc.invalidateQueries(['sellerBalance'])
      setAmount('')
      setError('')
      setSuccess('Solicitud enviada correctamente. Procesaremos tu retiro en breve.')
      setTimeout(() => setSuccess(''), 5000)
    },
    onError: (err) => {
      setError(err.response?.data?.error || 'Error al solicitar el retiro.')
      setSuccess('')
    }
  })

  const handleWithdraw = () => {
    setError('')
    if (!amount || parseFloat(amount) <= 0) {
      setError('Ingresá un monto válido.')
      return
    }
    if (parseFloat(amount) > balance?.balance) {
      setError('El monto supera tu saldo disponible.')
      return
    }
    withdrawMutation.mutate()
  }

  const hasMp = !!profile?.mercadoPagoAccount
  const hasPending = balance?.withdrawals?.some(w => w.status === 'Pending')

  if (isLoading) return <div className="admin-loading">Cargando fondos...</div>

  return (
    <div>
      {/* Saldo disponible */}
      <div className="stats-grid" style={{ marginBottom: '32px' }}>
        <div className="stat-card accent">
          <span className="stat-card-icon">💰</span>
          <span className="stat-card-value">${balance?.balance?.toFixed(2) ?? '0.00'}</span>
          <span className="stat-card-label">Saldo disponible</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">📤</span>
          <span className="stat-card-value">
            ${balance?.withdrawals
              ?.filter(w => w.status === 'Approved')
              ?.reduce((acc, w) => acc + w.amount, 0)
              ?.toFixed(2) ?? '0.00'}
          </span>
          <span className="stat-card-label">Total retirado</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">⏳</span>
          <span className="stat-card-value">
            {balance?.withdrawals?.filter(w => w.status === 'Pending').length ?? 0}
          </span>
          <span className="stat-card-label">Retiros pendientes</span>
        </div>
      </div>

      {/* Formulario de retiro */}
      <div className="admin-form" style={{ marginBottom: '32px' }}>
        <h3>Solicitar retiro</h3>

        {!hasMp && (
          <div className="profile-error" style={{ marginBottom: '16px' }}>
            ⚠️ No tenés una cuenta de MercadoPago registrada. Agregá tu alias o CBU desde tu perfil antes de retirar fondos.
          </div>
        )}

        {hasPending && (
          <div className="profile-error" style={{ marginBottom: '16px' }}>
            ⏳ Ya tenés una solicitud de retiro pendiente. Esperá a que sea procesada.
          </div>
        )}

        {error && <div className="checkout-error" style={{ marginBottom: '16px' }}>{error}</div>}
        {success && <div className="profile-success" style={{ marginBottom: '16px' }}>{success}</div>}

        <div className="admin-form-grid">
          <div className="form-group">
            <label>Monto a retirar (ARS)</label>
            <input
              type="number"
              placeholder="Ej: 5000"
              value={amount}
              onChange={e => setAmount(e.target.value)}
              min="1"
              max={balance?.balance}
              disabled={!hasMp || hasPending}
            />
          </div>
          {hasMp && (
            <div className="form-group">
              <label>Se enviará a</label>
              <input
                type="text"
                value={profile?.mercadoPagoAccount}
                disabled
                style={{ opacity: 0.6 }}
              />
            </div>
          )}
        </div>

        <div className="admin-form-actions">
          <button
            className="btn-admin-primary"
            onClick={handleWithdraw}
            disabled={!hasMp || hasPending || withdrawMutation.isPending || !amount}
          >
            {withdrawMutation.isPending ? 'Enviando...' : 'Solicitar retiro'}
          </button>
          {balance?.balance > 0 && (
            <button
              className="btn-admin-secondary"
              onClick={() => setAmount(balance.balance.toFixed(2))}
              disabled={!hasMp || hasPending}
            >
              Retirar todo
            </button>
          )}
        </div>
      </div>

      {/* Historial de retiros */}
      <div className="admin-form">
        <h3>Historial de retiros</h3>
        {!balance?.withdrawals?.length ? (
          <p style={{ color: 'var(--text-muted)', marginTop: '12px' }}>
            Todavía no realizaste ningún retiro.
          </p>
        ) : (
          <div className="admin-table-wrap" style={{ marginTop: '16px' }}>
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Monto</th>
                  <th>Alias / CBU</th>
                  <th>Estado</th>
                  <th>Nota</th>
                </tr>
              </thead>
              <tbody>
                {balance.withdrawals.map(w => (
                  <tr key={w.id}>
                    <td style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                      {new Date(w.requestedAt).toLocaleDateString('es-AR')}
                    </td>
                    <td>${w.amount.toFixed(2)}</td>
                    <td style={{ fontSize: '13px' }}>{w.alias}</td>
                    <td>
                      {w.status === 'Pending' && (
                        <span className="status-badge pending">Pendiente</span>
                      )}
                      {w.status === 'Approved' && (
                        <span className="status-badge paid">Aprobado</span>
                      )}
                      {w.status === 'Rejected' && (
                        <span className="status-badge cancelled">Rechazado</span>
                      )}
                    </td>
                    <td style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                      {w.adminNote ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}