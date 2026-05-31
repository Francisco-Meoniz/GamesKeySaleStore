import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getGames } from '../../api/games'
import { createGame, updateGame, deleteGame } from '../../api/admin'

const EMPTY_FORM = { title: '', description: '', imageUrl: '', platform: 'Steam' }
const PLATFORMS = ['Steam', 'Epic Games', 'GOG', 'EA App', 'Ubisoft Connect', 'Battle.net']

export default function AdminGames() {
  const qc = useQueryClient()
  const { data: games, isLoading } = useQuery({ queryKey: ['games'], queryFn: getGames })
  const [form, setForm] = useState(EMPTY_FORM)
  const [editingId, setEditingId] = useState(null)
  const [showForm, setShowForm] = useState(false)

  const createMutation = useMutation({
    mutationFn: createGame,
    onSuccess: () => { qc.invalidateQueries(['games']); resetForm() }
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => updateGame(id, data),
    onSuccess: () => { qc.invalidateQueries(['games']); resetForm() }
  })

  const deleteMutation = useMutation({
    mutationFn: deleteGame,
    onSuccess: () => qc.invalidateQueries(['games'])
  })

  const resetForm = () => {
    setForm(EMPTY_FORM)
    setEditingId(null)
    setShowForm(false)
  }

  const handleEdit = (game) => {
    setForm({
      title: game.title,
      description: game.description,
      imageUrl: game.imageUrl,
      platform: game.platform
    })
    setEditingId(game.id)
    setShowForm(true)
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    if (editingId) {
      updateMutation.mutate({ id: editingId, data: form })
    } else {
      createMutation.mutate(form)
    }
  }

  if (isLoading) return <div className="admin-loading">Cargando juegos...</div>

  return (
    <div>
      <div className="admin-actions-bar">
        <button className="btn-admin-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancelar' : '+ Agregar juego'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="admin-form">
          <h3>{editingId ? 'Editar juego' : 'Nuevo juego'}</h3>
          <div className="admin-form-grid">
            <div className="form-group">
              <label>Título</label>
              <input
                type="text"
                value={form.title}
                onChange={e => setForm({ ...form, title: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Plataforma</label>
              <select
                value={form.platform}
                onChange={e => setForm({ ...form, platform: e.target.value })}
                className="role-select"
              >
                {PLATFORMS.map(p => <option key={p} value={p}>{p}</option>)}
              </select>
            </div>
            <div className="form-group full-width">
              <label>Descripción</label>
              <textarea
                value={form.description}
                onChange={e => setForm({ ...form, description: e.target.value })}
                rows={3}
                required
              />
            </div>
            <div className="form-group full-width">
              <label>URL de imagen</label>
              <input
                type="text"
                value={form.imageUrl}
                onChange={e => setForm({ ...form, imageUrl: e.target.value })}
                placeholder="https://..."
              />
            </div>
          </div>
          <div className="admin-form-actions">
            <button type="submit" className="btn-admin-primary">
              {editingId ? 'Guardar cambios' : 'Crear juego'}
            </button>
            <button type="button" className="btn-admin-secondary" onClick={resetForm}>
              Cancelar
            </button>
          </div>
        </form>
      )}

      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>Juego</th>
              <th>Plataforma</th>
              <th>Keys disponibles</th>
              <th>Precio mínimo</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {games?.map(g => (
              <tr key={g.id}>
                <td>
                  <div className="game-row-info">
                    {g.imageUrl && (
                      <img src={g.imageUrl} alt={g.title} className="game-thumb" />
                    )}
                    <span>{g.title}</span>
                  </div>
                </td>
                <td>{g.platform}</td>
                <td>{g.availableKeys ?? g.availableStock}</td>
                <td>{g.lowestPrice ? `$${g.lowestPrice.toFixed(2)}` : '-'}</td>
                <td>
                  <div className="action-buttons">
                    <button className="btn-action edit" onClick={() => handleEdit(g)}>
                      Editar
                    </button>
                    <button
                      className="btn-action ban"
                      onClick={() => {
                        if (confirm(`¿Eliminar ${g.title}?`)) deleteMutation.mutate(g.id)
                      }}
                    >
                      Eliminar
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}