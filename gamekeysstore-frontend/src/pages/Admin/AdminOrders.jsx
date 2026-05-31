import { useQuery } from '@tanstack/react-query'
import { getOrders } from '../../api/admin'

export default function AdminOrders() {
  const { data: orders, isLoading } = useQuery({
    queryKey: ['adminOrders'],
    queryFn: getOrders
  })

  if (isLoading) return <div className="admin-loading">Cargando órdenes...</div>

  return (
    <div>
      <div className="admin-table-wrap">
        <table className="admin-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Comprador</th>
              <th>Total</th>
              <th>Estado</th>
              <th>Fecha</th>
            </tr>
          </thead>
          <tbody>
            {orders.map(o => (
              <tr key={o.id}>
                <td>#{o.id}</td>
                <td>{o.buyerEmail}</td>
                <td>${o.totalAmount?.toFixed(2)}</td>
                <td>
                  <span className={`status-badge ${o.status.toLowerCase()}`}>
                    {o.status}
                  </span>
                </td>
                <td>{new Date(o.createdAt).toLocaleDateString('es-AR')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}