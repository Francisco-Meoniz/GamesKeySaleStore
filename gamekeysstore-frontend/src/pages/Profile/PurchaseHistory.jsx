import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getPurchaseHistory, downloadReceipt } from '../../api/profile'
import { useAuth } from '../../context/AuthContext'
import RatingForm from '../../components/ui/RatingForm'
import './PurchaseHistory.css'

export default function PurchaseHistory() {
  const { user } = useAuth()
  const { data: orders, isLoading } = useQuery({
    queryKey: ['purchaseHistory'],
    queryFn: getPurchaseHistory
  })

  const [expandedOrder, setExpandedOrder] = useState(null)
  const [ratingForm, setRatingForm] = useState(null) // { orderId, sellerProfileId, sellerName }

  const handleDownload = async (orderId) => {
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

  if (isLoading) return <div className="history-loading">Cargando historial...</div>

  if (!orders || orders.length === 0) {
    return (
      <div className="history-empty">
        <span>🛒</span>
        <p>Todavía no realizaste ninguna compra.</p>
      </div>
    )
  }

  return (
    <div className="history-list">
      {orders.map(order => (
        <div key={order.orderId} className="history-card">

          {/* Header */}
          <div
            className="history-card-header"
            onClick={() => setExpandedOrder(
              expandedOrder === order.orderId ? null : order.orderId
            )}
          >
            <div className="history-order-info">
              <span className="history-order-id">Orden #{order.orderId}</span>
              <span className="history-order-date">
                {new Date(order.createdAt).toLocaleDateString('es-AR', {
                  day: '2-digit', month: 'long', year: 'numeric'
                })}
              </span>
            </div>
            <div className="history-order-right">
              <span className={`status-badge ${order.status.toLowerCase()}`}>
                {order.status === 'Paid' ? 'Pagado' :
                 order.status === 'Pending' ? 'Pendiente' :
                 order.status === 'Failed' ? 'Fallido' : order.status}
              </span>
              <span className="history-total">${order.totalAmount.toFixed(2)}</span>
              <span className="history-expand">
                {expandedOrder === order.orderId ? '▲' : '▼'}
              </span>
            </div>
          </div>

          {/* Detalle */}
          {expandedOrder === order.orderId && (
            <div className="history-card-body">
              {order.items.map((item, i) => (
                <div key={i} className="history-item">
                  <div className="history-item-left">
                    {item.gameImageUrl && (
                      <img
                        src={item.gameImageUrl}
                        alt={item.gameTitle}
                        className="history-item-img"
                      />
                    )}
                    <div className="history-item-info">
                      <span className="history-item-title">{item.gameTitle}</span>
                      <span className="history-item-platform">{item.platform}</span>
                      <span className="history-item-seller">
                        Vendedor: {item.sellerName}
                      </span>
                    </div>
                  </div>
                  <div className="history-item-right">
                    <span className="history-item-price">${item.unitPrice.toFixed(2)}</span>
                    {order.status === 'Paid' && (
                      <div className="history-key">
                        <span className="history-key-label">Tu key:</span>
                        <code className="history-key-code">{item.keyCode}</code>
                      </div>
                    )}
                    {order.status === 'Paid' && item.sellerProfileId && (
                      <button
                        className="btn-rate"
                        onClick={() => setRatingForm({
                          orderId: order.orderId,
                          sellerProfileId: item.sellerProfileId,
                          sellerName: item.sellerName
                        })}
                      >
                        ⭐ Calificar vendedor
                      </button>
                    )}
                  </div>
                </div>
              ))}

              {/* Formulario de calificación */}
              {ratingForm && ratingForm.orderId === order.orderId && (
                <RatingForm
                  orderId={ratingForm.orderId}
                  sellerProfileId={ratingForm.sellerProfileId}
                  sellerName={ratingForm.sellerName}
                  onDone={() => setRatingForm(null)}
                />
              )}

              {order.status === 'Paid' && (
                <div className="history-card-footer">
                  <button
                    className="btn-download-small"
                    onClick={() => handleDownload(order.orderId)}
                  >
                    ⬇ Descargar comprobante
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      ))}
    </div>
  )
}