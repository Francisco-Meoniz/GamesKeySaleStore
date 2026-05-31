import { Link } from 'react-router-dom'
import './Home.css'

export default function GameCard({ game }) {
  return (
    <Link to={`/games/${game.id}`} className="game-card">
      <div className="game-card-img">
        {game.imageUrl
          ? <img src={game.imageUrl} alt={game.title} />
          : <div className="game-card-placeholder">🎮</div>
        }
      </div>
      <div className="game-card-info">
        <span className="game-platform">{game.platform}</span>
        <h3 className="game-title">{game.title}</h3>
        <div className="game-card-footer">
          {game.lowestPrice ? (
            <div className="game-price">
              <span className="price-label">Desde</span>
              <span className="price-value">${game.lowestPrice.toFixed(2)}</span>
            </div>
          ) : (
            <span className="no-stock">Sin stock</span>
          )}
          <span className="game-keys-count">
            {game.availableKeys} keys
          </span>
        </div>
      </div>
    </Link>
  )
}