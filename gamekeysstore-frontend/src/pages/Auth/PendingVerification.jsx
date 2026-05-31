import { useState, useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { resendVerification } from '../../api/auth'
import './Auth.css'

export default function PendingVerification() {
  const location = useLocation()
  const email = location.state?.email || localStorage.getItem('pendingEmail') || ''

  const [sent, setSent] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [cooldown, setCooldown] = useState(0)

  // Countdown timer
  useEffect(() => {
    if (cooldown <= 0) return
    const timer = setTimeout(() => setCooldown(c => c - 1), 1000)
    return () => clearTimeout(timer)
  }, [cooldown])

  const handleResend = async () => {
    setLoading(true)
    setError('')
    setSent(false)
    try {
      await resendVerification(email)
      setSent(true)
      setCooldown(30)
    } catch (err) {
      setError(err.response?.data?.error || 'Error al reenviar. Intentá de nuevo.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">🎮 GameKeys<span>Store</span></h1>

        <div className="auth-success">
          <span className="auth-success-icon">📧</span>
          <h2 style={{ color: 'var(--neon-blue)' }}>Verificá tu email</h2>
          <p>
            Te mandamos un email de verificación a <strong>{email}</strong>.
            Revisá tu bandeja de entrada y hacé click en el link.
          </p>

          {error && <div className="auth-error" style={{ marginTop: '16px' }}>{error}</div>}

          {sent && (
            <div className="profile-success" style={{ marginTop: '16px' }}>
              ✅ Email reenviado correctamente.
            </div>
          )}

          <button
            className="btn-primary"
            style={{ marginTop: '24px', width: '100%' }}
            onClick={handleResend}
            disabled={loading || cooldown > 0}
          >
            {loading
              ? 'Enviando...'
              : cooldown > 0
                ? `Reenviar en ${cooldown}s`
                : '¿No te llegó? Reenviar email'
            }
          </button>

          <p className="auth-footer" style={{ marginTop: '16px' }}>
            <Link to="/login">← Volver al login</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
  