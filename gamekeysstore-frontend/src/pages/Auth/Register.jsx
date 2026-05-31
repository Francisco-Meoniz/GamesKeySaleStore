import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { register } from '../../api/auth'
import './Auth.css'

export default function Register() {
  const [form, setForm] = useState({
    username: '',
    email: '',
    password: '',
    isSeller: false,
    sellerDisplayName: '',
    firstName: '',
    lastName: '',
    dni: '',
    birthDate: '',
    mercadoPagoAccount: ''
  })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  const validateSellerFields = () => {
    if (!form.firstName.trim()) return 'El nombre es obligatorio.'
    if (!form.lastName.trim()) return 'El apellido es obligatorio.'
    if (!form.dni.trim()) return 'El DNI es obligatorio.'
    if (!/^\d{7,8}$/.test(form.dni)) return 'El DNI debe tener 7 u 8 dígitos.'
    if (!form.birthDate) return 'La fecha de nacimiento es obligatoria.'

    const birth = new Date(form.birthDate)
    const today = new Date()
    let age = today.getFullYear() - birth.getFullYear()
    const m = today.getMonth() - birth.getMonth()
    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--
    if (age < 18) return 'Debés ser mayor de 18 años para registrarte como vendedor.'

    return null
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')

    if (form.isSeller) {
      const validationError = validateSellerFields()
      if (validationError) {
        setError(validationError)
        return
      }
    }

    setLoading(true)
    try {
      await register(form)
      localStorage.setItem('pendingEmail', form.email)
      navigate('/pending-verification', { state: { email: form.email } })
    } catch (err) {
      setError(err.response?.data?.error || 'Error al registrarse')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1 className="auth-title">🎮 GameKeys<span>Store</span></h1>
        <h2>Crear cuenta</h2>

        {error && <div className="auth-error">{error}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label>Usuario</label>
            <input
              type="text"
              placeholder="tunombre"
              value={form.username}
              onChange={e => setForm({ ...form, username: e.target.value })}
              required
            />
          </div>

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

          <div className="form-toggle">
            <label className="toggle-label">
              <input
                type="checkbox"
                checked={form.isSeller}
                onChange={e => setForm({ ...form, isSeller: e.target.checked })}
              />
              <span>Quiero registrarme como vendedor</span>
            </label>
          </div>

          {form.isSeller && (
            <>
              <div className="form-group">
                <label>Nombre de tu tienda</label>
                <input
                  type="text"
                  placeholder="Ej: Pepe Store"
                  value={form.sellerDisplayName}
                  onChange={e => setForm({ ...form, sellerDisplayName: e.target.value })}
                  required
                />
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>Nombre</label>
                  <input
                    type="text"
                    placeholder="Tu nombre"
                    value={form.firstName}
                    onChange={e => setForm({ ...form, firstName: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Apellido</label>
                  <input
                    type="text"
                    placeholder="Tu apellido"
                    value={form.lastName}
                    onChange={e => setForm({ ...form, lastName: e.target.value })}
                    required
                  />
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label>DNI</label>
                  <input
                    type="text"
                    placeholder="12345678"
                    value={form.dni}
                    onChange={e => setForm({ ...form, dni: e.target.value.replace(/\D/g, '') })}
                    maxLength={8}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Fecha de nacimiento</label>
                  <input
                    type="date"
                    value={form.birthDate}
                    onChange={e => setForm({ ...form, birthDate: e.target.value })}
                    max={new Date(new Date().setFullYear(new Date().getFullYear() - 18))
                      .toISOString().split('T')[0]}
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label>Alias o CBU de MercadoPago <span className="field-optional">(opcional)</span></label>
                  <input
                  type="text"
                  placeholder="Ej: mi.alias.mp"
                  value={form.mercadoPagoAccount}
                  onChange={e => setForm({ ...form, mercadoPagoAccount: e.target.value })}
                />
                  <span className="field-hint">Podés agregarlo después desde tu perfil</span>
                </div>
            </>
          )}

          <button type="submit" className="btn-primary" disabled={loading}>
            {loading ? 'Creando cuenta...' : 'Crear cuenta'}
          </button>
        </form>

        <p className="auth-footer">
          ¿Ya tenés cuenta?{' '}
          <Link to="/login">Iniciá sesión</Link>
        </p>
      </div>
    </div>
  )
}