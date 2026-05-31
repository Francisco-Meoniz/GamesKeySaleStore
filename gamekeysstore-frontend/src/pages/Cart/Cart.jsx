import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../../context/CartContext'
import { useAuth } from '../../context/AuthContext'
import './Cart.css'

export default function Cart() {
  const { cart, removeFromCart, clearCart } = useCart()
  const { user } = useAuth()
  const navigate = useNavigate()

  const handleCheckout = () => {
    navigate('/checkout', {
      state: {
        fromCart: true,
        selectedKeys: cart.items.map(item => ({
          id: item.gameKeyId,
          price: item.price,
          seller: { displayName: item.sellerName }
        })),
        cartItems: cart.items
      }
    })
  }

  if (!cart?.items?.length) {
    return (
      <div className="cart-page">
        <div className="cart-empty-page">
          <span>🛒</span>
          <h2>Tu carrito está vacío</h2>
          <p>Agregá keys desde la página de cada juego.</p>
          <Link to="/" className="btn-primary">Ver catálogo</Link>
        </div>
      </div>
    )
  }

  return (
    <div className="cart-page">
      <div className="cart-container">

        {/* Lista de items */}
        <div className="cart-list-section">
          <div className="cart-header">
            <h1>Mi carrito <span>({cart.count}/3 keys)</span></h1>
            <button className="btn-clear-cart" onClick={() => clearCart()}>
              🗑 Vaciar carrito
            </button>
          </div>

          <div className="cart-list">
            {cart.items.map(item => (
              <div key={item.id} className="cart-list-item">
                <div className="cart-list-item-left">
                  {item.gameImageUrl && (
                    <img
                      src={item.gameImageUrl}
                      alt={item.gameTitle}
                      className="cart-list-img"
                    />
                  )}
                  <div className="cart-list-info">
                    <span className="cart-list-platform">{item.platform}</span>
                    <h3 className="cart-list-title">{item.gameTitle}</h3>
                    <span className="cart-list-seller">Vendedor: {item.sellerName}</span>
                  </div>
                </div>
                <div className="cart-list-right">
                  <span className="cart-list-price">${item.price.toFixed(2)}</span>
                  <button
                    className="btn-remove-cart"
                    onClick={() => removeFromCart(item.id)}
                  >
                    ✕ Eliminar
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Resumen */}
        <div className="cart-summary-section">
          <div className="cart-summary-card">
            <h3>Resumen</h3>
            <div className="cart-summary-rows">
              {cart.items.map(item => (
                <div key={item.id} className="cart-summary-row">
                  <span>{item.gameTitle}</span>
                  <span>${item.price.toFixed(2)}</span>
                </div>
              ))}
            </div>
            <div className="cart-summary-total">
              <span>Total</span>
              <span>${cart.total.toFixed(2)}</span>
            </div>
            <button className="btn-pay" onClick={handleCheckout}>
              Pagar ahora →
            </button>
            <Link to="/" className="btn-back">
              ← Seguir comprando
            </Link>
          </div>
        </div>

      </div>
    </div>
  )
}