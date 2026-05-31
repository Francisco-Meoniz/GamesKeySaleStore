import { useState } from 'react'
import { Link } from 'react-router-dom'
import { forgotPassword } from '../../api/auth'
import './Auth.css'

export default function ForgotPassword() {
  const [email, setEmail] = useState('')
  const [loading, setLoading] = useState(false)
  const [sent, setSent] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await forgotPassword(email)
      setSent(true)
    } catch {
      setError('Ocurrió un error. Intentá de nuevo.')
    } finally {
      setLoading(false)
    }
  }

  if (sent) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <h1 className="auth-title">🎮 GameKeys<span>Store</span></h1>
          <div className="auth-success">
            <span className="auth-success-icon">📧</span>
            <h2>¡Email enviado!</h2>
            <p>Si el email está registrado, vas a recibir un link para restablecer tu contraseña en los próximos minutos.</p>
            <Link to="/login" className="btn-primary" style={{ display: 'block', textAlign: 'center', marginTop: '24px' }}>
              Volver al login
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">🎮 GameKeys<span>Store</span></h1>
        <h2>Recuperar contraseña</h2>

        {error && <div className="auth-error">{error}</div>}

        <p className="auth-hint">
          Ingresá tu email y te mandamos un link para restablecer tu contraseña.
        </p>

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              placeholder="tu@email.com"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
            />
          </div>
          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? 'Enviando...' : 'Enviar link'}
          </button>
        </form>

        <p className="auth-footer">
          <Link to="/login">← Volver al login</Link>
        </p>
      </div>
    </div>
  )
}