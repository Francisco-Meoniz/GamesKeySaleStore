import { useState } from 'react'
import AdminStats from './AdminStats'
import AdminUsers from './AdminUsers'
import AdminGames from './AdminGames'
import AdminOrders from './AdminOrders'
import AdminWithdrawals from './AdminWithdrawals'
import './Admin.css'

const TABS = [
  { key: 'stats',       label: '📊 Estadísticas' },
  { key: 'users',       label: '👥 Usuarios' },
  { key: 'games',       label: '🎮 Juegos' },
  { key: 'orders',      label: '📦 Órdenes' },
  { key: 'withdrawals', label: '💸 Retiros' },
]

export default function AdminDashboard() {
  const [activeTab, setActiveTab] = useState('stats')

  return (
    <div className="admin-page">
      <div className="admin-header">
        <h1>Panel de <span>Administración</span></h1>
        <p>Gestioná usuarios, juegos y órdenes de la plataforma</p>
      </div>

      <div className="admin-tabs">
        {TABS.map(tab => (
          <button
            key={tab.key}
            className={`admin-tab ${activeTab === tab.key ? 'active' : ''}`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="admin-content">
        {activeTab === 'stats'       && <AdminStats />}
        {activeTab === 'users'       && <AdminUsers />}
        {activeTab === 'games'       && <AdminGames />}
        {activeTab === 'orders'      && <AdminOrders />}
        {activeTab === 'withdrawals' && <AdminWithdrawals />}
      </div>
    </div>
  )
}