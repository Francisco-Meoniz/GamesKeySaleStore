import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getGames } from '../../api/games'
import GameCard from './GameCard'
import './Home.css'

const PLATFORMS = ['Todas', 'Steam', 'Epic Games', 'GOG', 'EA App', 'Ubisoft Connect', 'Battle.net']

export default function Home() {
  const { data: games, isLoading, isError } = useQuery({
    queryKey: ['games'],
    queryFn: getGames,
  })

  const [searchInput, setSearchInput] = useState('')
  const [searchQuery, setSearchQuery] = useState('')
  const [platform, setPlatform] = useState('Todas')
  const [minPrice, setMinPrice] = useState('')
  const [maxPrice, setMaxPrice] = useState('')
  const [onlyInStock, setOnlyInStock] = useState(false)

  const handleSearch = () => {
    setSearchQuery(searchInput)
  }

  const handleClear = () => {
    setSearchInput('')
    setSearchQuery('')
    setPlatform('Todas')
    setMinPrice('')
    setMaxPrice('')
    setOnlyInStock(false)
  }

  const filtered = games?.filter(g => {
    if (searchQuery && !g.title.toLowerCase().includes(searchQuery.toLowerCase()))
      return false
    if (platform !== 'Todas' && g.platform !== platform)
      return false
    if (minPrice && (g.lowestPrice == null || g.lowestPrice < parseFloat(minPrice)))
      return false
    if (maxPrice && (g.lowestPrice == null || g.lowestPrice > parseFloat(maxPrice)))
      return false
    if (onlyInStock && g.availableKeys === 0)
      return false
    return true
  })

  const hasFilters = searchQuery || platform !== 'Todas' || minPrice || maxPrice || onlyInStock

  return (
    <main className="home">
      <div className="home-hero">
        <h1>Conseguí tus <span>game keys</span> al mejor precio</h1>
        <p>Miles de vendedores, los mejores precios. Siempre.</p>
      </div>

      {/* Buscador */}
      <div className="search-section">
        <div className="search-bar">
          <input
            type="text"
            placeholder="Buscar juego..."
            value={searchInput}
            onChange={e => setSearchInput(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleSearch()}
            className="search-input"
          />
          <button className="search-btn" onClick={handleSearch}>
            🔍 Buscar
          </button>
        </div>

        {/* Filtros */}
        <div className="filters-bar">
          <div className="filter-group">
            <label>Plataforma</label>
            <select
              value={platform}
              onChange={e => setPlatform(e.target.value)}
              className="filter-select"
            >
              {PLATFORMS.map(p => (
                <option key={p} value={p}>{p}</option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label>Precio mínimo</label>
            <input
              type="number"
              placeholder="$ mín"
              value={minPrice}
              onChange={e => setMinPrice(e.target.value)}
              className="filter-input"
              min="0"
            />
          </div>

          <div className="filter-group">
            <label>Precio máximo</label>
            <input
              type="number"
              placeholder="$ máx"
              value={maxPrice}
              onChange={e => setMaxPrice(e.target.value)}
              className="filter-input"
              min="0"
            />
          </div>

          <div className="filter-group filter-check">
            <label className="check-label">
              <input
                type="checkbox"
                checked={onlyInStock}
                onChange={e => setOnlyInStock(e.target.checked)}
              />
              <span>Solo con stock</span>
            </label>
          </div>

          {hasFilters && (
            <button className="clear-btn" onClick={handleClear}>
              ✕ Limpiar
            </button>
          )}
        </div>
      </div>

      <section className="home-catalog">
        <div className="catalog-header">
          <h2>Catálogo</h2>
          {filtered && (
            <span className="results-count">
              {filtered.length} {filtered.length === 1 ? 'juego' : 'juegos'}
            </span>
          )}
        </div>

        {isLoading && (
          <div className="games-grid">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="game-card skeleton" />
            ))}
          </div>
        )}

        {isError && (
          <p className="error-msg">Error al cargar los juegos.
            Verificá que el backend esté corriendo.</p>
        )}

        {filtered && filtered.length === 0 && (
          <div className="no-results">
            <span>🔍</span>
            <p>No se encontraron juegos con esos filtros.</p>
            <button className="clear-btn" onClick={handleClear}>
              Limpiar filtros
            </button>
          </div>
        )}

        {filtered && filtered.length > 0 && (
          <div className="games-grid">
            {filtered.map(game => (
              <GameCard key={game.id} game={game} />
            ))}
          </div>
        )}
      </section>
    </main>
  )
}