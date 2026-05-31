import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createRating } from '../../api/ratings'
import './RatingForm.css'

export default function RatingForm({ orderId, sellerProfileId, sellerName, onDone }) {
  const [stars, setStars] = useState(0)
  const [hovered, setHovered] = useState(0)
  const [comment, setComment] = useState('')
  const [error, setError] = useState('')
  const qc = useQueryClient()

  const mutation = useMutation({
    mutationFn: createRating,
    onSuccess: () => {
      qc.invalidateQueries(['purchaseHistory'])
      onDone()
    },
    onError: (err) => {
      setError(err.response?.data?.error || 'Error al enviar calificación.')
    }
  })

  const handleSubmit = () => {
    if (stars === 0) {
      setError('Seleccioná al menos una estrella.')
      return
    }
    setError('')
    mutation.mutate({ orderId, sellerProfileId, stars, comment })
  }

  return (
    <div className="rating-form">
      <h4>Calificar a <span>{sellerName}</span></h4>

      <div className="stars-selector">
        {[1, 2, 3, 4, 5].map(s => (
          <button
            key={s}
            className={`star-btn ${s <= (hovered || stars) ? 'active' : ''}`}
            onMouseEnter={() => setHovered(s)}
            onMouseLeave={() => setHovered(0)}
            onClick={() => setStars(s)}
          >
            ★
          </button>
        ))}
        {stars > 0 && (
          <span className="stars-label">
            {['', 'Muy malo', 'Malo', 'Regular', 'Bueno', 'Excelente'][stars]}
          </span>
        )}
      </div>

      <textarea
        className="rating-comment"
        placeholder="Comentario (opcional)..."
        value={comment}
        onChange={e => setComment(e.target.value)}
        rows={3}
        maxLength={300}
      />
      <span className="comment-count">{comment.length}/300</span>

      {error && <p className="rating-error">{error}</p>}

      <div className="rating-actions">
        <button
          className="btn-admin-primary"
          onClick={handleSubmit}
          disabled={mutation.isPending}
        >
          {mutation.isPending ? 'Enviando...' : 'Enviar calificación'}
        </button>
        <button className="btn-admin-secondary" onClick={onDone}>
          Cancelar
        </button>
      </div>
    </div>
  )
}