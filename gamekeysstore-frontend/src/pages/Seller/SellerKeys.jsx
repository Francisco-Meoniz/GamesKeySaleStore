import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getMyKeys, publishKey, toggleKey, updateKeyPrice, deleteKey } from '../../api/seller'
import { getGames } from '../../api/games'

export default function SellerKeys() {
  const qc = useQueryClient()
  const { data: keys, isLoading } = useQuery({ queryKey: ['myKeys'], queryFn: getMyKeys })
  const { data: games } = useQuery({ queryKey: ['games'], queryFn: getGames })

  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ gameId: '', keyCode: '', price: '' })
  const [editingPrice, setEditingPrice] = useState(null)
  const [newPrice, setNewPrice] = useState('')
  const [error, setError] = useState('')
  const [filter, setFilter] = useState('all')

  const publishMutation = useMutation({
    mutationFn: publishKey,
    onSuccess: () => {
      qc.invalidateQueries(['myKeys'])
      qc.invalidateQueries(['sellerStats'])
      setForm({ gameId: '', keyCode: '', price: '' })
      setShowForm(false)
      setError('')
    },
    onError: (err) => setError(err.response?.data?.error || 'Error al publicar.')
  })

  const toggleMutation = useMutation({
    mutationFn: toggleKey,
    onSuccess: () => qc.invalidateQueries(['myKeys'])
  })

  const priceMutation = useMutation({
    mutationFn: ({ keyId, price }) => updateKeyPrice(keyId, price),
    onSuccess: () => {
      qc.invalidateQueries(['myKeys'])
      setEditingPrice(null)
    }
  })

  const deleteMutation = useMutation({
    mutationFn: deleteKey,
    onSuccess: () => {
      qc.invalidateQueries(['myKeys'])
      qc.invalidateQueries(['sellerStats'])
    }
  })

  const filtered = keys?.filter(k => {
    if (filter === 'active') return k.isActive && !k.isSold
    if (filter === 'inactive') return !k.isActive && !k.isSold
    if (filter === 'sold') return k.isSold
    return true
  })

  if (isLoading) return <div className="admin-loading">Cargando keys...</div>

  return (
    <div>
      {/* Barra de acciones */}
      <div className="seller-actions-bar">
        <button className="btn-admin-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancelar' : '+ Publicar key'}
        </button>
        <div className="filter-tabs">
          {['all', 'active', 'inactive', 'sold'].map(f => (
            <button
              key={f}
              className={`filter-tab ${filter === f ? 'active' : ''}`}
              onClick={() => setFilter(f)}
            >
              {f === 'all' ? 'Todas' : f === 'active' ? 'Activas' :
               f === 'inactive' ? 'Inactivas' : 'Vendidas'}
            </button>
          ))}
        </div>
      </div>

      {/* Formulario publicar key */}
      {showForm && (
        <div className="admin-form">
          <h3>Publicar nueva key</h3>
          {error && <div className="checkout-error" style={{ marginBottom: '16px' }}>{error}</div>}
          <div className="admin-form-grid">
            <div className="form-group">
              <label>Juego</label>
              <select
                className="role-select"
                value={form.gameId}
                onChange={e => setForm({ ...form, gameId: e.target.value })}
                style={{ width: '100%', padding: '10px 14px' }}
              >
                <option value="">Seleccioná un juego</option>
                {games?.map(g => (
                  <option key={g.id} value={g.id}>{g.title} — {g.platform}</option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Precio (ARS)</label>
              <input
                type="number"
                placeholder="Ej: 9999"
                value={form.price}
                onChange={e => setForm({ ...form, price: e.target.value })}
                min="1"
              />
            </div>
            <div className="form-group full-width">
              <label>Código de la key</label>
              <input
                type="text"
                placeholder="XXXX-YYYY-ZZZZ-WWWW"
                value={form.keyCode}
                onChange={e => setForm({ ...form, keyCode: e.target.value })}
              />
            </div>
          </div>
          <div className="admin-form-actions">
            <button
              className="btn-admin-primary"
              onClick={() => publishMutation.mutate({
                gameId: parseInt(form.gameId),
                keyCode: form.keyCode,
                price: parseFloat(form.price)
              })}
              disabled={!form.gameId || !form.keyCode || !form.price}
            >
              Publicar key
            </button>
            <button className="btn-admin-secondary" onClick={() => setShowForm(false)}>
              Cancelar
            </button>
          </div>
        </div>
      )}

      {/* Lista de keys */}
      {filtered?.length === 0 ? (
        <p className="admin-loading">No hay keys en esta categoría.</p>
      ) : (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead>
              <tr>
                <th>Juego</th>
                <th>Key</th>
                <th>Precio</th>
                <th>Estado</th>
                <th>Fecha</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {filtered?.map(k => (
                <tr key={k.id}>
                  <td>
                    <div className="game-row-info">
                      {k.gameImageUrl && (
                        <img src={k.gameImageUrl} alt={k.gameTitle} className="game-thumb" />
                      )}
                      <div>
                        <span style={{ display: 'block', fontWeight: 600 }}>{k.gameTitle}</span>
                        <span style={{ fontSize: '11px', color: 'var(--neon-blue)' }}>{k.platform}</span>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className="key-code">
                      {k.isSold ? k.keyCode : '••••-••••-••••'}
                    </span>
                  </td>
                  <td>
                    {editingPrice === k.id ? (
                      <div className="price-edit">
                        <input
                          type="number"
                          value={newPrice}
                          onChange={e => setNewPrice(e.target.value)}
                          className="price-input"
                          autoFocus
                        />
                        <button
                          className="btn-action edit"
                          onClick={() => priceMutation.mutate({ keyId: k.id, price: parseFloat(newPrice) })}
                        >
                          ✓
                        </button>
                        <button
                          className="btn-action"
                          style={{ borderColor: 'var(--border)', color: 'var(--text-muted)' }}
                          onClick={() => setEditingPrice(null)}
                        >
                          ✕
                        </button>
                      </div>
                    ) : (
                      <span
                        className="key-price-cell"
                        onClick={() => {
                          if (!k.isSold) {
                            setEditingPrice(k.id)
                            setNewPrice(k.price.toString())
                          }
                        }}
                      >
                        ${k.price.toFixed(2)}
                        {!k.isSold && <span className="edit-hint">✏️</span>}
                      </span>
                    )}
                  </td>
                  <td>
                    {k.isSold
                      ? <span className="status-badge paid">Vendida</span>
                      : k.isActive
                        ? <span className="status-badge paid">Activa</span>
                        : <span className="status-badge cancelled">Inactiva</span>
                    }
                  </td>
                  <td>
                    <span style={{ fontSize: '12px', color: 'var(--text-muted)' }}>
                      {new Date(k.createdAt).toLocaleDateString('es-AR')}
                    </span>
                    {k.soldAt && (
                      <span style={{ display: 'block', fontSize: '11px', color: 'var(--green)' }}>
                        Vendida: {new Date(k.soldAt).toLocaleDateString('es-AR')}
                      </span>
                    )}
                  </td>
                  <td>
                    {!k.isSold && (
                      <div className="action-buttons">
                        <button
                          className={`btn-action ${k.isActive ? 'ban' : 'unban'}`}
                          onClick={() => toggleMutation.mutate(k.id)}
                        >
                          {k.isActive ? 'Desactivar' : 'Activar'}
                        </button>
                        <button
                          className="btn-action ban"
                          onClick={() => {
                            if (confirm('¿Eliminar esta key?')) deleteMutation.mutate(k.id)
                          }}
                        >
                          Eliminar
                        </button>
                      </div>
                    )}
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