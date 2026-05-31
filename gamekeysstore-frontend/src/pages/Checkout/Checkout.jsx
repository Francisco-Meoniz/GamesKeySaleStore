import { useState } from 'react'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { createOrder, createPaymentPreference } from '../../api/orders'
import './Checkout.css'

export default function Checkout() {
  const { state } = useLocation()
  const navigate = useNavigate()
  const { user } = useAuth()

  const selectedKeys = state?.selectedKeys || []
  const cartItems = state?.cartItems || null
  const game = state?.game
  const fromCart = state?.fromCart || false

  const [form, setForm] = useState({
    buyerName: user?.username || '',
    buyerEmail: user?.email || '',
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const displayItems = fromCart
    ? cartItems?.map(item => ({
        gameTitle: item.gameTitle,
        platform: item.platform,
        sellerName: item.sellerName,
        price: item.price
      }))
    : selectedKeys.map(k => ({
        gameTitle: game?.title,
        platform: game?.platform,
        sellerName: k.seller?.displayName,
        price: k.price
      }))

  const total = displayItems?.reduce((sum, item) => sum + item.price, 0) || 0

  if (!displayItems || displayItems.length === 0) {
    return (
      <div className="checkout-empty">
        <h2>No hay keys seleccionadas</h2>
        <Link to="/" className="btn-primary">Volver al catálogo</Link>
      </div>
    )
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      let items = []

      if (fromCart && cartItems) {
        // Desde el carrito — cada item es una key individual
        items = cartItems.map(item => ({
          gameId: item.gameId,
          quantity: 1
        }))
      } else {
        // Desde la página del juego
        const itemsMap = {}
        selectedKeys.forEach(k => {
          const gameId = game.id
          if (!itemsMap[gameId]) itemsMap[gameId] = 0
          itemsMap[gameId]++
        })
        items = Object.entries(itemsMap).map(([gameId, quantity]) => ({
          gameId: parseInt(gameId),
          quantity
        }))
      }

      const order = await createOrder({
        buyerId: user?.userId,
        buyerName: form.buyerName,
        buyerEmail: form.buyerEmail,
        items
      })

      const { paymentUrl } = await createPaymentPreference(order.id)
      window.location.href = paymentUrl

    } catch (err) {
      setError(err.response?.data?.error || 'Error al procesar la orden.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="checkout-page">
      <div className="checkout-container">

        {/* Resumen */}
        <div className="checkout-summary">
          <h2 className="checkout-section-title">Resumen de compra</h2>

          {game && !fromCart && (
            <div className="summary-game">
              {game?.imageUrl && (
                <img src={game.imageUrl} alt={game.title} className="summary-game-img" />
              )}
              <div>
                <span className="summary-platform">{game?.platform}</span>
                <h3 className="summary-game-title">{game?.title}</h3>
              </div>
            </div>
          )}

          <div className="summary-keys">
            {displayItems.map((item, i) => (
              <div key={i} className="summary-key-row">
                <div>
                  <span className="summary-seller">{item.gameTitle}</span>
                  <span className="summary-key-label">{item.sellerName} · 1x Key</span>
                </div>
                <span className="summary-key-price">${item.price.toFixed(2)}</span>
              </div>
            ))}
          </div>

          <div className="summary-total">
            <span>Total</span>
            <span>${total.toFixed(2)}</span>
          </div>

          <div className="summary-badges">
            <span className="badge">🔒 Pago seguro</span>
            <span className="badge">📧 Key por email</span>
            <span className="badge">⚡ Entrega inmediata</span>
          </div>
        </div>

        {/* Formulario */}
        <div className="checkout-form-section">
          <h2 className="checkout-section-title">Tus datos</h2>

          {error && <div className="checkout-error">{error}</div>}

          <form onSubmit={handleSubmit} className="checkout-form">
            <div className="form-group">
              <label>Nombre completo</label>
              <input
                type="text"
                placeholder="Tu nombre"
                value={form.buyerName}
                onChange={e => setForm({ ...form, buyerName: e.target.value })}
                required
              />
            </div>

            <div className="form-group">
              <label>Email</label>
              <input
                type="email"
                placeholder="tu@email.com"
                value={form.buyerEmail}
                onChange={e => setForm({ ...form, buyerEmail: e.target.value })}
                required
              />
              <span className="field-hint">
                Recibirás las keys y el comprobante en este email
              </span>
            </div>

            <div className="checkout-mp-info">
              <img
                src="https://http2.mlstatic.com/frontend-assets/mp-web-navigation/ui-navigation/5.21.22/mercadopago/logo__large@2x.png"
                alt="MercadoPago"
                className="mp-logo"
              />
              <p>Serás redirigido a MercadoPago para completar el pago de forma segura.</p>
            </div>

            <button
              type="submit"
              className="btn-pay"
              disabled={loading}
            >
              {loading ? 'Procesando...' : `Pagar $${total.toFixed(2)} →`}
            </button>

            <Link
              to={fromCart ? '/cart' : `/games/${game?.id}`}
              className="btn-back"
            >
              ← Volver y modificar selección
            </Link>
          </form>
        </div>

      </div>
    </div>
  )
}