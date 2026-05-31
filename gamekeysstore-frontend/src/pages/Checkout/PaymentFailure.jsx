import { Link, useSearchParams } from 'react-router-dom'
import './Checkout.css'

export default function PaymentFailure() {
  const [searchParams] = useSearchParams()
  const orderId = searchParams.get('orderId')

  return (
    <div className="payment-result">
      <div className="payment-result-card failure">
        <div className="result-icon">❌</div>
        <h1>Pago fallido</h1>
        <p>No pudimos procesar tu pago. No se realizó ningún cargo.</p>
        <div className="result-actions">
          <Link to={`/`} className="btn-primary">
            Volver al catálogo
          </Link>
        </div>
      </div>
    </div>
  )
}