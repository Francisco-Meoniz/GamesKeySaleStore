import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login } from '../../api/auth'
import { useAuth } from '../../context/AuthContext'
import './Auth.css'

export default function Login() {
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { loginUser } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
  e.preventDefault()
  setError('')
  setLoading(true)
  try {
    const data = await login(form)
    loginUser(data)
    navigate('/')
  } catch (err) {
    const errorMsg = err.response?.data?.error || 'Error al iniciar sesión'
    // Si el error es por email no verificado, redirigir
    if (errorMsg.includes('verificar tu email')) {
      localStorage.setItem('pendingEmail', form.email)
      navigate('/pending-verification')
    } else {
      setError(errorMsg)
    }
  } finally {
    setLoading(false)
  }
}

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">🎮 GameKeys<span>Store</span></h1>
        <h2>Iniciar sesión</h2>

        {error && <div className="auth-error">{error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              placeholder="tu@email.com"
              value={form.email}
              onChange={e => setForm({ ...form, email: e.target.value })}
              required
            />
          </div>
          <div className="form-group">
            <label>Contraseña</label>
            <input
              type="password"
              placeholder="••••••••"
              value={form.password}
              onChange={e => setForm({ ...form, password: e.target.value })}
              required
            />
          </div>
          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? 'Ingresando...' : 'Ingresar'}
          </button>
          <p className="auth-forgot">
            <Link to="/forgot-password">¿Olvidaste tu contraseña?</Link>
          </p>
        </form>

        <p className="auth-footer">
          ¿No tenés cuenta?{' '}
          <Link to="/register">Registrate</Link>
        </p>
      </div>
    </div>
  )
}