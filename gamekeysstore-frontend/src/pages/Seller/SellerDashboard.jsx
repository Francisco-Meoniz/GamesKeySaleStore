import { useState } from 'react'
import SellerStats from './SellerStats'
import SellerKeys from './SellerKeys'
import SellerFunds from './SellerFunds'
import './Seller.css'

const TABS = [
  { key: 'stats', label: '📊 Resumen' },
  { key: 'keys', label: '🔑 Mis Keys' },
  { key: 'funds', label: '💸 Mis Fondos' },
]

export default function SellerDashboard() {
  const [activeTab, setActiveTab] = useState('stats')

  return (
    <div className="seller-page">
      <div className="seller-header">
        <h1>Panel de <span>Vendedor</span></h1>
        <p>Gestioná tus keys y seguí tus ventas</p>
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

      <div className="seller-content">
        {activeTab === 'stats' && <SellerStats />}
        {activeTab === 'keys'  && <SellerKeys />}
        {activeTab === 'funds' && <SellerFunds />}
      </div>
    </div>
  )
}