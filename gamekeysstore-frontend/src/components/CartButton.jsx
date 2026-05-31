import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import './CartButton.css'

export default function CartButton() {
  const { user } = useAuth()
  const { cart, removeFromCart } = useCart()
  const [open, setOpen] = useState(false)

  if (!user || user.role === 'Seller') return null

  return (
    <div className="cart-float">
      {/* Botón flotante */}
      <button
        className="cart-float-btn"
        onClick={() => setOpen(!open)}
      >
        🛒
        {cart?.count > 0 && (
          <span className="cart-badge">{cart.count}</span>
        )}
      </button>

      {/* Dropdown */}
      {open && (
        <div className="cart-dropdown">
          <div className="cart-dropdown-header">
            <h3>Mi carrito</h3>
            <span className="cart-count">{cart?.count}/3 keys</span>
          </div>

          {cart?.items?.length === 0 ? (
            <div className="cart-empty">
              <span>🎮</span>
              <p>El carrito está vacío</p>
            </div>
          ) : (
            <>
              <div className="cart-items">
                {cart?.items?.map(item => (
                  <div key={item.id} className="cart-item">
                    <div className="cart-item-info">
                      {item.gameImageUrl && (
                        <img src={item.gameImageUrl} alt={item.gameTitle} className="cart-item-img" />
                      )}
                      <div>
                        <span className="cart-item-title">{item.gameTitle}</span>
                        <span className="cart-item-seller">{item.sellerName}</span>
                      </div>
                    </div>
                    <div className="cart-item-right">
                      <span className="cart-item-price">${item.price.toFixed(2)}</span>
                      <button
                        className="cart-item-remove"
                        onClick={() => removeFromCart(item.id)}
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                ))}
              </div>

              <div className="cart-dropdown-footer">
                <div className="cart-total">
                  <span>Total</span>
                  <span>${cart?.total?.toFixed(2)}</span>
                </div>
                <Link
                  to="/cart"
                  className="btn-cart-checkout"
                  onClick={() => setOpen(false)}
                >
                  Ver carrito →
                </Link>
              </div>
            </>
          )}
        </div>
      )}

      {/* Overlay para cerrar */}
      {open && (
        <div className="cart-overlay" onClick={() => setOpen(false)} />
      )}
    </div>
  )
}