import { useEffect, useState } from 'react'
import { useSearchParams, useNavigate } from 'react-router-dom'
import { verifyEmail } from '../../api/auth'
import { useAuth } from '../../context/AuthContext'
import './Auth.css'

export default function VerifyEmail() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const [status, setStatus] = useState('loading')
  const [error, setError] = useState('')
  const { loginUser } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!token) {
      setStatus('error')
      setError('Token inválido.')
      return
    }

    verifyEmail(token)
      .then((data) => {
        setStatus('success')
        loginUser(data)
        // Redirigir al inicio después de 2 segundos
        setTimeout(() => navigate('/'), 2000)
      })
      .catch(err => {
        setStatus('error')
        setError(err.response?.data?.error || 'Error al verificar el email.')
      })
  }, [token])

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">🎮 GameKeys<span>Store</span></h1>

        {status === 'loading' && (
          <div className="auth-success">
            <span className="auth-success-icon">⏳</span>
            <h2 style={{ color: 'var(--neon-blue)' }}>Verificando...</h2>
          </div>
        )}

        {status === 'success' && (
          <div className="auth-success">
            <span className="auth-success-icon">✅</span>
            <h2>¡Email verificado!</h2>
            <p>Tu cuenta fue activada. Redirigiendo al inicio...</p>
          </div>
        )}

        {status === 'error' && (
          <div>
            <div className="auth-error" style={{ marginTop: '20px' }}>{error}</div>
            <p className="auth-footer" style={{ marginTop: '16px' }}>
              <a href="/login">← Volver al login</a>
            </p>
          </div>
        )}
      </div>
    </div>
  )
}