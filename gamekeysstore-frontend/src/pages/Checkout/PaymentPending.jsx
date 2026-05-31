import { Link } from 'react-router-dom'
import './Checkout.css'

export default function PaymentPending() {
  return (
    <div className="payment-result">
      <div className="payment-result-card pending">
        <div className="result-icon">⏳</div>
        <h1>Pago pendiente</h1>
        <p>Tu pago está siendo procesado. Te notificaremos por email cuando se confirme.</p>
        <div className="result-actions">
          <Link to="/" className="btn-primary">
            Volver al catálogo
          </Link>
        </div>
      </div>
    </div>
  )
}