import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import './Navbar.css'

export default function Navbar() {
  const { user, logoutUser } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logoutUser()
    navigate('/')
  }

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-logo">
        🎮 <span>GameKeys</span>Store
      </Link>

      <div className="navbar-links">
        <Link to="/">Catálogo</Link>
        {user ? (
          <>
            <span className="navbar-user">Hola, {user.username}</span>
            <Link to="/profile">Mi perfil</Link>
            {user.role === 'Seller' && (
              <Link to="/seller/dashboard">Mi Panel</Link>
            )}
            {user.role === 'Admin' && (
              <Link to="/admin">Admin</Link>
            )}
            <button className="btn-logout" onClick={handleLogout}>
              Salir
            </button>
          </>
        ) : (
          <>
            <Link to="/login">Iniciar sesión</Link>
            <Link to="/register" className="btn-register">
              Registrarse
            </Link>
          </>
        )}
      </div>
    </nav>
  )
}