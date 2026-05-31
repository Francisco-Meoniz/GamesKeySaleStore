import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getProfile, updateProfile, changePassword, uploadAvatar } from '../../api/profile'
import { useAuth } from '../../context/AuthContext'
import { useNavigate } from 'react-router-dom'
import PurchaseHistory from './PurchaseHistory'
import './Profile.css'

export default function Profile() {
  const { user, loginUser, logoutUser } = useAuth()
  const qc = useQueryClient()
  const navigate = useNavigate()

  const { data: profile, isLoading } = useQuery({
    queryKey: ['profile'],
    queryFn: getProfile
  })

  const [profileForm, setProfileForm] = useState(null)
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '', newPassword: '', confirmPassword: ''
  })
  const [profileError, setProfileError] = useState('')
  const [profileSuccess, setProfileSuccess] = useState('')
  const [passwordError, setPasswordError] = useState('')
  const [passwordSuccess, setPasswordSuccess] = useState('')
  const [avatarError, setAvatarError] = useState('')
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(0)
  const [showMp, setShowMp] = useState(false)

  useEffect(() => {
    if (profile && !profileForm) {
      setProfileForm({
        username: profile.username,
        email: profile.email,
        sellerDisplayName: profile.sellerDisplayName || '',
        // 👇 NUEVOS CAMPOS
        firstName: profile.firstName || '',
        lastName: profile.lastName || '',
        dni: profile.dni || '',
        birthDate: profile.birthDate ? profile.birthDate.split('T')[0] : '',
        mercadoPagoAccount: profile.mercadoPagoAccount || ''
      })
    }
  }, [profile])

  const updateMutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: (data) => {
      qc.invalidateQueries(['profile'])
      loginUser({ ...user, username: data.username, email: data.email })
      setProfileSuccess('Perfil actualizado correctamente.')
      setProfileError('')
      setTimeout(() => setProfileSuccess(''), 3000)
    },
    onError: (err) => {
      setProfileError(err.response?.data?.error || 'Error al actualizar.')
      setProfileSuccess('')
    }
  })

  const passwordMutation = useMutation({
    mutationFn: changePassword,
    onSuccess: () => {
      setPasswordSuccess('Contraseña cambiada correctamente.')
      setPasswordError('')
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' })
      setTimeout(() => setPasswordSuccess(''), 3000)
    },
    onError: (err) => {
      setPasswordError(err.response?.data?.error || 'Error al cambiar contraseña.')
      setPasswordSuccess('')
    }
  })

  const avatarMutation = useMutation({
    mutationFn: uploadAvatar,
    onSuccess: (data) => {
      qc.invalidateQueries(['profile'])
      loginUser({ ...user, avatarUrl: data.avatarUrl })
      setAvatarError('')
    },
    onError: (err) => {
      setAvatarError(err.response?.data?.error || 'Error al subir imagen.')
    }
  })

  const handleProfileSubmit = (e) => {
    e.preventDefault()
    updateMutation.mutate(profileForm)
  }

  const handlePasswordSubmit = (e) => {
    e.preventDefault()
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordError('Las contraseñas no coinciden.')
      return
    }
    if (passwordForm.newPassword.length < 6) {
      setPasswordError('La contraseña debe tener al menos 6 caracteres.')
      return
    }
    passwordMutation.mutate({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword
    })
  }

  const handleAvatarChange = (e) => {
    const file = e.target.files[0]
    if (file) avatarMutation.mutate(file)
  }

  const handleDeleteAccount = async () => {
    const token = localStorage.getItem('token')
    logoutUser()
    try {
      await fetch('http://localhost:5086/api/profile/delete-account', {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      })
    } catch {
      // ignoramos
    } finally {
      window.location.href = '/'
    }
  }

  const backendUrl = 'http://localhost:5086'

  if (isLoading) return <div className="profile-loading">Cargando perfil...</div>

  return (
    <div className="profile-page">
      <div className="profile-container">
        {/* Sidebar con avatar */}
        <div className="profile-sidebar">
          <div className="avatar-section">
            <div className="avatar-wrapper">
              {profile?.avatarUrl ? (
                <img
                  src={`${backendUrl}${profile.avatarUrl}`}
                  alt="Avatar"
                  className="avatar-img"
                />
              ) : (
                <div className="avatar-placeholder">
                  {profile?.username?.[0]?.toUpperCase()}
                </div>
              )}
              <label className="avatar-upload-btn" title="Cambiar foto">
                📷
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={handleAvatarChange}
                  style={{ display: 'none' }}
                />
              </label>
            </div>
            {avatarError && <p className="avatar-error">{avatarError}</p>}
            {avatarMutation.isPending && <p className="avatar-loading">Subiendo...</p>}
            <h2 className="profile-username">{profile?.username}</h2>
            <span className={`profile-role-badge ${profile?.role?.toLowerCase()}`}>
              {profile?.role}
            </span>
            <p className="profile-since">
              Miembro desde {new Date(profile?.createdAt).toLocaleDateString('es-AR', {
                month: 'long', year: 'numeric'
              })}
            </p>
          </div>
        </div>

        {/* Formularios */}
        <div className="profile-forms">
          {/* Datos del perfil */}
          <div className="profile-card">
            <h3 className="profile-card-title">Información del perfil</h3>
            {profileError && <div className="profile-error">{profileError}</div>}
            {profileSuccess && <div className="profile-success">{profileSuccess}</div>}
            {profileForm && (
              <form onSubmit={handleProfileSubmit} className="profile-form">
                <div className="form-group">
                  <label htmlFor="username">Nombre de usuario</label>
                  <input
                    id="username"
                    name="username"
                    type="text"
                    autoComplete="username"
                    value={profileForm.username}
                    onChange={e => setProfileForm({ ...profileForm, username: e.target.value })}
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="email">Email</label>
                  <input
                    id="email"
                    name="email"
                    type="email"
                    autoComplete="email"
                    value={profileForm.email}
                    onChange={e => setProfileForm({ ...profileForm, email: e.target.value })}
                    required
                  />
                </div>

                {/* Campos solo para vendedores */}
                {profile?.role === 'Seller' && (
                  <>
                    <div className="form-group">
                      <label htmlFor="sellerDisplayName">Nombre de tienda</label>
                      <input
                        id="sellerDisplayName"
                        name="sellerDisplayName"
                        type="text"
                        autoComplete="organization"
                        value={profileForm.sellerDisplayName}
                        onChange={e => setProfileForm({
                          ...profileForm, sellerDisplayName: e.target.value
                        })}
                        placeholder="Ej: Pepe Store"
                      />
                    </div>

                    {/* 👇 NUEVOS CAMPOS PARA VENDEDOR */}
                    <div className="form-group">
                      <label htmlFor="firstName">Nombre</label>
                      <input
                        id="firstName"
                        name="firstName"
                        type="text"
                        value={profileForm.firstName}
                        onChange={e => setProfileForm({ ...profileForm, firstName: e.target.value })}
                        placeholder="Tu nombre"
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="lastName">Apellido</label>
                      <input
                        id="lastName"
                        name="lastName"
                        type="text"
                        value={profileForm.lastName}
                        onChange={e => setProfileForm({ ...profileForm, lastName: e.target.value })}
                        placeholder="Tu apellido"
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="dni">DNI</label>
                      <input
                        id="dni"
                        name="dni"
                        type="text"
                        value={profileForm.dni}
                        onChange={e => setProfileForm({ ...profileForm, dni: e.target.value })}
                        placeholder="12345678"
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="birthDate">Fecha de nacimiento</label>
                      <input
                        id="birthDate"
                        name="birthDate"
                        type="date"
                        value={profileForm.birthDate}
                        onChange={e => setProfileForm({ ...profileForm, birthDate: e.target.value })}
                      />
                    </div>

                    <div className="form-group">
                      <label htmlFor="mercadoPagoAccount">Alias o CBU de MercadoPago</label>
                      <div className="input-with-toggle">
                        <input
                          id="mercadoPagoAccount"
                          name="mercadoPagoAccount"
                          type={showMp ? 'text' : 'password'}
                          value={profileForm.mercadoPagoAccount}
                          onChange={e => setProfileForm({ ...profileForm, mercadoPagoAccount: e.target.value })}
                          placeholder="Ej: mi.alias.mp"
                        />
                        <button
                          type="button"
                          className="toggle-visibility-btn"
                          onClick={() => setShowMp(v => !v)}
                          title={showMp ? 'Ocultar' : 'Mostrar'}
                        >
                          {showMp ? '🙈' : '👁️'}
                        </button>
                      </div>
                      <span className="field-hint">Opcional - para recibir pagos</span>
                    </div>
                  </>
                )}

                <button
                  type="submit"
                  className="btn-primary"
                  disabled={updateMutation.isPending}
                >
                  {updateMutation.isPending ? 'Guardando...' : 'Guardar cambios'}
                </button>
              </form>
            )}
          </div>

          {/* Cambiar contraseña */}
          <div className="profile-card">
            <h3 className="profile-card-title">Cambiar contraseña</h3>
            {passwordError && <div className="profile-error">{passwordError}</div>}
            {passwordSuccess && <div className="profile-success">{passwordSuccess}</div>}
            <form onSubmit={handlePasswordSubmit} className="profile-form">
              <div className="form-group">
                <label htmlFor="currentPassword">Contraseña actual</label>
                <input
                  id="currentPassword"
                  name="currentPassword"
                  type="password"
                  autoComplete="current-password"
                  placeholder="••••••••"
                  value={passwordForm.currentPassword}
                  onChange={e => setPasswordForm({
                    ...passwordForm, currentPassword: e.target.value
                  })}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="newPassword">Nueva contraseña</label>
                <input
                  id="newPassword"
                  name="newPassword"
                  type="password"
                  autoComplete="new-password"
                  placeholder="••••••••"
                  value={passwordForm.newPassword}
                  onChange={e => setPasswordForm({
                    ...passwordForm, newPassword: e.target.value
                  })}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="confirmPassword">Confirmar nueva contraseña</label>
                <input
                  id="confirmPassword"
                  name="confirmPassword"
                  type="password"
                  autoComplete="new-password"
                  placeholder="••••••••"
                  value={passwordForm.confirmPassword}
                  onChange={e => setPasswordForm({
                    ...passwordForm, confirmPassword: e.target.value
                  })}
                  required
                />
              </div>
              <button
                type="submit"
                className="btn-primary"
                disabled={passwordMutation.isPending}
              >
                {passwordMutation.isPending ? 'Cambiando...' : 'Cambiar contraseña'}
              </button>
            </form>
          </div>

          {/* Historial de compras */}
          <div className="profile-card">
            <h3 className="profile-card-title">Historial de compras</h3>
            <PurchaseHistory />
          </div>

          {/* Zona de peligro - solo visible para usuarios normales? opcional */}
          <div className="profile-card danger-card">
            <h3 className="profile-card-title danger-title">Zona de peligro</h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '14px', marginBottom: '20px', lineHeight: '1.6' }}>
              Al eliminar tu cuenta se borrarán todos tus datos permanentemente.
              Esta acción no se puede deshacer. Podrás registrarte de nuevo con el mismo email.
            </p>
            {showDeleteConfirm === 0 && (
              <button className="btn-danger" onClick={() => setShowDeleteConfirm(1)}>
                Eliminar mi cuenta
              </button>
            )}
            {showDeleteConfirm === 1 && (
              <div className="delete-confirm">
                <p style={{ color: 'var(--accent)', fontSize: '14px', marginBottom: '16px' }}>
                  ⚠️ ¿Estás seguro que querés eliminar tu cuenta?
                </p>
                <div style={{ display: 'flex', gap: '12px' }}>
                  <button className="btn-danger" onClick={() => setShowDeleteConfirm(2)}>
                    Sí, eliminar
                  </button>
                  <button className="btn-admin-secondary" onClick={() => setShowDeleteConfirm(0)}>
                    Cancelar
                  </button>
                </div>
              </div>
            )}
            {showDeleteConfirm === 2 && (
              <div className="delete-confirm">
                <p style={{ color: 'var(--accent)', fontSize: '14px', marginBottom: '16px' }}>
                  🚨 Última confirmación. ¿Confirmás que querés borrar todo permanentemente?
                </p>
                <div style={{ display: 'flex', gap: '12px' }}>
                  <button className="btn-danger" onClick={handleDeleteAccount}>
                    Sí, borrar todo
                  </button>
                  <button className="btn-admin-secondary" onClick={() => setShowDeleteConfirm(0)}>
                    Cancelar
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}