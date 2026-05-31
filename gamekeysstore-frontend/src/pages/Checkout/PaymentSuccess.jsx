import { useEffect, useState } from 'react'
import { useSearchParams, Link } from 'react-router-dom'
import { getOrder, downloadReceipt } from '../../api/orders'
import { useQueryClient } from '@tanstack/react-query'
import './Checkout.css'

export default function PaymentSuccess() {
  const [searchParams] = useSearchParams()
  const orderId = searchParams.get('orderId')
  const paymentId = searchParams.get('payment_id')
  const externalReference = searchParams.get('external_reference')
  const qc = useQueryClient()

  const [order, setOrder] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
  if (!orderId) return

  const confirmAndLoad = async () => {
  try {
    let url = ''
    if (paymentId && externalReference) {
      url = `http://localhost:5086/api/payments/success?payment_id=${paymentId}&external_reference=${externalReference}`
    } else if (orderId) {
      url = `http://localhost:5086/api/payments/success?payment_id=&external_reference=${orderId}`
    }

    console.log('Confirmando pago en:', url)
    const res = await fetch(url)
    console.log('Respuesta:', res.status)

    qc.invalidateQueries(['cart'])

    const data = await getOrder(orderId)
    setOrder(data)
  } catch (err) {
    console.error('Error:', err)
    setError('Error al confirmar el pago.')
  } finally {
    setLoading(false)
  }
}

  confirmAndLoad()
}, [orderId])

  const handleDownload = async () => {
    try {
      const blob = await downloadReceipt(orderId)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `comprobante-orden-${orderId}.pdf`
      a.click()
      URL.revokeObjectURL(url)
    } catch {
      alert('Error al descargar el comprobante.')
    }
  }

  if (loading) return (
    <div className="payment-result">
      <div className="payment-result-card success">
        <div className="result-icon">⏳</div>
        <h1>Confirmando pago...</h1>
        <p>Esperá un momento mientras procesamos tu compra.</p>
      </div>
    </div>
  )

  return (
    <div className="payment-result">
      <div className="payment-result-card success">
        <div className="result-icon">✅</div>
        <h1>¡Pago exitoso!</h1>
        <p>Tu compra fue procesada correctamente.<br />
          Revisá tu email, te enviamos las keys y el comprobante.</p>

        {error && <div style={{ color: 'var(--accent)', marginBottom: '16px' }}>{error}</div>}

        {order && (
          <div className="result-order-info">
            <div className="result-row">
              <span>Orden</span>
              <span>#{order.id}</span>
            </div>
            <div className="result-row">
              <span>Total</span>
              <span>${order.totalAmount?.toFixed(2)}</span>
            </div>
            <div className="result-row">
              <span>Email</span>
              <span>{order.buyerEmail}</span>
            </div>
            <div className="result-row">
              <span>Estado</span>
              <span style={{ color: order.status === 'Paid' ? 'var(--green)' : 'var(--yellow)' }}>
                {order.status === 'Paid' ? '✓ Pagado' : order.status}
              </span>
            </div>
          </div>
        )}

        <div className="result-actions">
          {order?.status === 'Paid' && (
            <button className="btn-download" onClick={handleDownload}>
              ⬇ Descargar comprobante PDF
            </button>
          )}
          <Link to="/" className="btn-primary">
            Volver al catálogo
          </Link>
          <Link to="/profile" className="btn-admin-secondary" style={{ textAlign: 'center', padding: '12px', borderRadius: '4px' }}>
            Ver historial de compras
          </Link>
        </div>
      </div>
    </div>
  )
}