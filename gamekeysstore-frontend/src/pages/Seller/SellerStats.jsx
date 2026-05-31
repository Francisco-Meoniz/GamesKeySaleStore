import { useQuery } from '@tanstack/react-query'
import { getSellerStats } from '../../api/seller'

export default function SellerStats() {
  const { data, isLoading } = useQuery({
    queryKey: ['sellerStats'],
    queryFn: getSellerStats
  })

  if (isLoading) return <div className="admin-loading">Cargando...</div>

  return (
    <div>
      <div className="stats-grid">
        <div className="stat-card">
          <span className="stat-card-icon">🔑</span>
          <span className="stat-card-value">{data.activeKeys}</span>
          <span className="stat-card-label">Keys activas</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">✅</span>
          <span className="stat-card-value">{data.soldKeys}</span>
          <span className="stat-card-label">Keys vendidas</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">⭐</span>
          <span className="stat-card-value">{data.averageRating.toFixed(1)}</span>
          <span className="stat-card-label">Reputación</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">🛒</span>
          <span className="stat-card-value">{data.totalSales}</span>
          <span className="stat-card-label">Ventas totales</span>
        </div>
        <div className="stat-card accent">
          <span className="stat-card-icon">💰</span>
          <span className="stat-card-value">${data.totalEarnings.toFixed(2)}</span>
          <span className="stat-card-label">Ganancias totales</span>
        </div>
      </div>
    </div>
  )
}