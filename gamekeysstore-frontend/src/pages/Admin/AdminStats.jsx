import { useQuery } from '@tanstack/react-query'
import { getStats } from '../../api/admin'

export default function AdminStats() {
  const { data, isLoading } = useQuery({ queryKey: ['adminStats'], queryFn: getStats })

  if (isLoading) return <div className="admin-loading">Cargando estadísticas...</div>

  return (
    <div>
      <div className="stats-grid">
        <div className="stat-card">
          <span className="stat-card-icon">👥</span>
          <span className="stat-card-value">{data.totalUsers}</span>
          <span className="stat-card-label">Usuarios totales</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">🏪</span>
          <span className="stat-card-value">{data.totalSellers}</span>
          <span className="stat-card-label">Vendedores</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">🎮</span>
          <span className="stat-card-value">{data.totalGames}</span>
          <span className="stat-card-label">Juegos</span>
        </div>
        <div className="stat-card">
          <span className="stat-card-icon">📦</span>
          <span className="stat-card-value">{data.paidOrders}</span>
          <span className="stat-card-label">Órdenes pagadas</span>
        </div>
        <div className="stat-card accent">
          <span className="stat-card-icon">💰</span>
          <span className="stat-card-value">${data.totalRevenue?.toFixed(2)}</span>
          <span className="stat-card-label">Ingresos totales</span>
        </div>
      </div>

      <h3 className="admin-subtitle">Órdenes recientes</h3>
      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Email</th>
              <th>Total</th>
              <th>Estado</th>
              <th>Fecha</th>
            </tr>
          </thead>
          <tbody>
            {data.recentOrders.map(o => (
              <tr key={o.id}>
                <td>#{o.id}</td>
                <td>{o.buyerEmail}</td>
                <td>${o.totalAmount?.toFixed(2)}</td>
                <td><span className={`status-badge ${o.status.toLowerCase()}`}>{o.status}</span></td>
                <td>{new Date(o.createdAt).toLocaleDateString('es-AR')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}