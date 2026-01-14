import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Register = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    nombre: '',
    apellido: '',
    email: '',
    tipoIdentificacion: 'Cedula',
    documentoIdentidad: '',
    fechaNacimiento: '',
    telefono: '',
    pais: '',
    password: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const passwordStrength = () => {
    const pwd = formData.password;
    let strength = 0;
    if (pwd.length >= 8) strength += 20;
    if (/[a-z]/.test(pwd)) strength += 20;
    if (/[A-Z]/.test(pwd)) strength += 20;
    if (/\d/.test(pwd)) strength += 20;
    if (/[^A-Za-z0-9]/.test(pwd)) strength += 20;
    return strength;
  };

  const getPasswordColor = () => {
    const strength = passwordStrength();
    if (strength <= 40) return 'bg-danger';
    if (strength <= 60) return 'bg-warning';
    if (strength <= 80) return 'bg-info';
    return 'bg-success';
  };

  const handleChange = (e) => {
    let { name, value } = e.target;

    // Validaciones específicas según el campo
    if (name === 'email') {
      value = value.replace(/\s/g, '').toLowerCase();
    }
    
    if (name === 'telefono') {
      value = value.replace(/\D/g, '').slice(0, 10);
    }
    
    if (name === 'documentoIdentidad') {
      if (formData.tipoIdentificacion === 'Cedula') {
        value = value.replace(/\D/g, '').slice(0, 10);
      } else if (formData.tipoIdentificacion === 'RUC') {
        value = value.replace(/\D/g, '').slice(0, 13);
      } else {
        value = value.replace(/[^A-Za-z0-9]/g, '').toUpperCase().slice(0, 20);
      }
    }

    setFormData({ ...formData, [name]: value });
    setErrors({ ...errors, [name]: '' });
  };

  const validate = () => {
    const newErrors = {};

    if (!formData.nombre.trim()) newErrors.nombre = 'El nombre es requerido';
    if (!formData.apellido.trim()) newErrors.apellido = 'El apellido es requerido';
    
    if (!formData.email) {
      newErrors.email = 'El correo es requerido';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Correo inválido';
    }
    
    if (!formData.documentoIdentidad) {
      newErrors.documentoIdentidad = 'El documento es requerido';
    } else {
      if (formData.tipoIdentificacion === 'Cedula' && formData.documentoIdentidad.length !== 10) {
        newErrors.documentoIdentidad = 'La cédula debe tener 10 dígitos';
      }
      if (formData.tipoIdentificacion === 'RUC' && formData.documentoIdentidad.length !== 13) {
        newErrors.documentoIdentidad = 'El RUC debe tener 13 dígitos';
      }
    }
    
    if (!formData.fechaNacimiento) {
      newErrors.fechaNacimiento = 'La fecha de nacimiento es requerida';
    }
    
    if (!formData.telefono) {
      newErrors.telefono = 'El teléfono es requerido';
    } else if (formData.telefono.length !== 10) {
      newErrors.telefono = 'El teléfono debe tener 10 dígitos';
    }
    
    if (!formData.pais) newErrors.pais = 'El país es requerido';
    
    if (!formData.password) {
      newErrors.password = 'La contraseña es requerida';
    } else if (passwordStrength() < 100) {
      newErrors.password = 'La contraseña no cumple todos los requisitos';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validate()) return;
    
    setLoading(true);
    
    try {
      const response = await api.post(ENDPOINTS.REGISTER, {
        email: formData.email,
        password: formData.password,
        nombre: formData.nombre,
        apellido: formData.apellido,
        tipoIdentificacion: formData.tipoIdentificacion,
        documentoIdentidad: formData.documentoIdentidad,
        fechaNacimiento: formData.fechaNacimiento,
        telefono: formData.telefono,
        pais: formData.pais,
      });

      if (response.data.success) {
        Swal.fire({
          icon: 'success',
          title: '¡Registro exitoso!',
          text: 'Ahora puedes iniciar sesión',
          confirmButtonColor: '#0d6efd',
        });
        
        navigate('/login');
      } else {
        setErrors({ general: response.data.message || 'Error al registrar' });
      }
    } catch (error) {
      const message = error.response?.data?.message || 'Error al registrar. Intenta de nuevo.';
      setErrors({ general: message });
    } finally {
      setLoading(false);
    }
  };

  const getDocHint = () => {
    switch (formData.tipoIdentificacion) {
      case 'Cedula': return '10 dígitos numéricos';
      case 'RUC': return '13 dígitos numéricos';
      case 'Pasaporte': return 'Alfanumérico';
      default: return '';
    }
  };

  const maxBirthDate = new Date();
  maxBirthDate.setFullYear(maxBirthDate.getFullYear() - 18);

  return (
    <section className="container d-flex align-items-center justify-content-center py-5" style={{ minHeight: 'calc(100vh - 140px)' }}>
      <div className="card shadow-sm p-4 w-100" style={{ maxWidth: '600px' }}>
        <h2 className="mb-3 text-center">Crear cuenta</h2>

        <form onSubmit={handleSubmit} noValidate>
          {errors.general && (
            <div className="alert alert-danger py-2 small">{errors.general}</div>
          )}

          <div className="row g-3">
            <div className="col-md-6">
              <label className="form-label">Nombre</label>
              <input
                type="text"
                name="nombre"
                className={`form-control ${errors.nombre ? 'is-invalid' : ''}`}
                value={formData.nombre}
                onChange={handleChange}
              />
              {errors.nombre && <div className="text-danger small">{errors.nombre}</div>}
            </div>

            <div className="col-md-6">
              <label className="form-label">Apellido</label>
              <input
                type="text"
                name="apellido"
                className={`form-control ${errors.apellido ? 'is-invalid' : ''}`}
                value={formData.apellido}
                onChange={handleChange}
              />
              {errors.apellido && <div className="text-danger small">{errors.apellido}</div>}
            </div>

            <div className="col-12">
              <label className="form-label">Correo electrónico</label>
              <input
                type="email"
                name="email"
                className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                value={formData.email}
                onChange={handleChange}
              />
              {errors.email && <div className="text-danger small">{errors.email}</div>}
            </div>

            <div className="col-md-6">
              <label className="form-label">Tipo de identificación</label>
              <select
                name="tipoIdentificacion"
                className="form-select"
                value={formData.tipoIdentificacion}
                onChange={handleChange}
              >
                <option value="Cedula">Cédula (10 dígitos)</option>
                <option value="Pasaporte">Pasaporte (alfanumérico)</option>
                <option value="RUC">RUC (13 dígitos)</option>
              </select>
            </div>

            <div className="col-md-6">
              <label className="form-label">Documento de identidad</label>
              <input
                type="text"
                name="documentoIdentidad"
                className={`form-control ${errors.documentoIdentidad ? 'is-invalid' : ''}`}
                placeholder="Ej: 0105123456"
                value={formData.documentoIdentidad}
                onChange={handleChange}
              />
              {errors.documentoIdentidad && <div className="text-danger small">{errors.documentoIdentidad}</div>}
              <small className="text-muted">{getDocHint()}</small>
            </div>

            <div className="col-md-6">
              <label className="form-label">Fecha de nacimiento</label>
              <input
                type="date"
                name="fechaNacimiento"
                className={`form-control ${errors.fechaNacimiento ? 'is-invalid' : ''}`}
                max={maxBirthDate.toISOString().split('T')[0]}
                value={formData.fechaNacimiento}
                onChange={handleChange}
              />
              {errors.fechaNacimiento && <div className="text-danger small">{errors.fechaNacimiento}</div>}
            </div>

            <div className="col-md-6">
              <label className="form-label">Teléfono</label>
              <input
                type="text"
                name="telefono"
                className={`form-control ${errors.telefono ? 'is-invalid' : ''}`}
                placeholder="Ej: 0991234567"
                maxLength={10}
                value={formData.telefono}
                onChange={handleChange}
              />
              {errors.telefono && <div className="text-danger small">{errors.telefono}</div>}
              <small className="text-muted">10 dígitos numéricos</small>
            </div>

            <div className="col-12">
              <label className="form-label">País</label>
              <select
                name="pais"
                className={`form-select ${errors.pais ? 'is-invalid' : ''}`}
                value={formData.pais}
                onChange={handleChange}
              >
                <option value="">Seleccionar país...</option>
                <option value="Ecuador">Ecuador</option>
                <option value="Colombia">Colombia</option>
                <option value="Perú">Perú</option>
                <option value="México">México</option>
                <option value="Argentina">Argentina</option>
                <option value="Chile">Chile</option>
                <option value="España">España</option>
                <option value="Estados Unidos">Estados Unidos</option>
                <option value="Otro">Otro</option>
              </select>
              {errors.pais && <div className="text-danger small">{errors.pais}</div>}
            </div>

            <div className="col-12">
              <label className="form-label">Contraseña</label>
              <div className="input-group">
                <input
                  type={showPassword ? 'text' : 'password'}
                  name="password"
                  className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                  value={formData.password}
                  onChange={handleChange}
                />
                <button
                  className="btn btn-outline-secondary"
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                >
                  👁
                </button>
              </div>
              {errors.password && <div className="text-danger small">{errors.password}</div>}
              
              <div className="form-text mt-1">
                Mín. 8 caracteres con mayúscula, minúscula, número y símbolo.
              </div>

              <div className="progress mt-2" style={{ height: '8px' }}>
                <div
                  className={`progress-bar ${getPasswordColor()}`}
                  style={{ width: `${passwordStrength()}%` }}
                ></div>
              </div>

              <div className="small mt-2">
                <span className={formData.password.length >= 8 ? 'text-success' : 'text-danger'}>• 8+ caracteres</span><br />
                <span className={/[a-z]/.test(formData.password) ? 'text-success' : 'text-danger'}>• minúscula</span><br />
                <span className={/[A-Z]/.test(formData.password) ? 'text-success' : 'text-danger'}>• mayúscula</span><br />
                <span className={/\d/.test(formData.password) ? 'text-success' : 'text-danger'}>• número</span><br />
                <span className={/[^A-Za-z0-9]/.test(formData.password) ? 'text-success' : 'text-danger'}>• símbolo</span>
              </div>
            </div>
          </div>

          <button
            className="btn btn-primary w-100 mt-3"
            type="submit"
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="spinner-border spinner-border-sm me-2"></span>
                Creando cuenta...
              </>
            ) : (
              'Crear cuenta'
            )}
          </button>
        </form>

        <p className="text-center mt-3 mb-0">
          ¿Ya tienes cuenta? <Link to="/login">Iniciar sesión</Link>
        </p>
      </div>
    </section>
  );
};

export default Register;
