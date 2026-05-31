import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getGameById, getGameKeys } from '../../api/games'
import { useCart } from '../../context/CartContext'
import { useAuth } from '../../context/AuthContext'
import StarRating from '../../components/ui/StarRating'
import './GameDetail.css'
import '../../components/ui/StarRating.css'

export default function GameDetail() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { cart, addToCart, isAdding } = useCart()
  const { user } = useAuth()

  const { data: game, isLoading: loadingGame } = useQuery({
    queryKey: ['game', id],
    queryFn: () => getGameById(id),
  })

  const { data: keys, isLoading: loadingKeys } = useQuery({
    queryKey: ['gameKeys', id],
    queryFn: () => getGameKeys(id),
  })

  const isInCart = (keyId) => cart?.items?.some(i => i.gameKeyId === keyId)
  const cartFull = cart?.count >= 3

  if (loadingGame) return <div className="detail-loading">Cargando...</div>
  if (!game) return <div className="detail-loading">Juego no encontrado.</div>

  return (
    <div className="game-detail">

      {/* Hero */}
      <div className="detail-hero">
        {game.imageUrl
          ? <img src={game.imageUrl} alt={game.title} className="detail-hero-img" />
          : <div className="detail-hero-placeholder">🎮</div>
        }
        <div className="detail-hero-overlay" />
        <div className="detail-hero-content">
          <span className="detail-platform">{game.platform}</span>
          <h1 className="detail-title">{game.title}</h1>
          <p className="detail-description">{game.description}</p>
          <div className="detail-stats">
            {game.lowestPrice && (
              <div className="detail-stat">
                <span className="stat-label">Desde</span>
                <span className="stat-value green">${game.lowestPrice.toFixed(2)}</span>
              </div>
            )}
            <div className="detail-stat">
              <span className="stat-label">Keys disponibles</span>
              <span className="stat-value">{game.availableKeys}</span>
            </div>
          </div>
        </div>
      </div>

      <div className="detail-body">

        {/* Lista de keys */}
        <div className="detail-keys-section">
          <div className="section-header">
            <h2>Keys disponibles</h2>
            {cart?.count > 0 && (
              <span className="selected-badge">
                {cart.count}/3 en carrito
              </span>
            )}
          </div>

          {loadingKeys && (
            <div className="keys-list">
              {[...Array(3)].map((_, i) => (
                <div key={i} className="key-row skeleton" />
              ))}
            </div>
          )}

          {keys && keys.length === 0 && (
            <p className="no-keys">No hay keys disponibles por el momento.</p>
          )}

          {keys && keys.length > 0 && (
            <div className="keys-list">
              {keys.map(key => {
                const inCart = isInCart(key.id)
                const disabled = !inCart && cartFull

                return (
                  <div
                    key={key.id}
                    className={`key-row 
                      ${key.isCheapest ? 'cheapest' : ''} 
                      ${inCart ? 'selected' : ''} 
                      ${disabled ? 'disabled' : ''}`}
                  >
                    {key.isCheapest && (
                      <span className="cheapest-badge">⚡ Mejor precio</span>
                    )}

                    <div className="key-seller">
                      <span className="seller-name">{key.seller.displayName}</span>
                      <div className="seller-meta">
                        <StarRating rating={key.seller.averageRating} />
                        <span className="seller-sales">
                          {key.seller.totalSales} ventas
                        </span>
                        <span className="seller-time">
                          {key.seller.monthsOnPlatform < 1
                            ? 'Nuevo'
                            : `${key.seller.monthsOnPlatform} meses`}
                        </span>
                      </div>
                    </div>

                    <div className="key-right">
                      <span className="key-price">${key.price.toFixed(2)}</span>
                      <button
                        className={`key-select-btn ${inCart ? 'selected' : ''}`}
                        disabled={disabled || isAdding || user?.role === 'Seller'}
                        onClick={() => {
                          if (!user) { navigate('/login'); return }
                          if (user.role === 'Seller') return
                          if (!inCart) addToCart(key.id)
                        }}
                      >
                        {user?.role === 'Seller'
                          ? 'Solo usuarios pueden comprar'
                          : inCart ? '✓ En carrito'
                          : disabled ? 'Carrito lleno'
                          : 'Agregar al carrito'}
                      </button>
                    </div>
                  </div>
                )
              })}
            </div>
          )}
        </div>

        {/* Panel del carrito */}
        {cart?.count > 0 && (
          <div className="checkout-panel">
            <h3>Tu carrito</h3>
            <div className="checkout-items">
              {cart.items.map(k => (
                <div key={k.id} className="checkout-item">
                  <span>{k.gameTitle}</span>
                  <span className="checkout-item-price">${k.price.toFixed(2)}</span>
                </div>
              ))}
            </div>
            <div className="checkout-total">
              <span>Total</span>
              <span>${cart.total.toFixed(2)}</span>
            </div>
            <button className="btn-checkout" onClick={() => navigate('/cart')}>
              Ver carrito ({cart.count}/3) →
            </button>
            <p className="checkout-note">Máximo 3 keys por orden</p>
          </div>
        )}

      </div>
    </div>
  )
}