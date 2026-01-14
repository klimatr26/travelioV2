import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import useAuthStore from '../store/authStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Login = () => {
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    let value = e.target.value;
    const name = e.target.name;
    
    // No permitir espacios
    value = value.replace(/\s/g, '');
    
    // Email en minúsculas
    if (name === 'email') {
      value = value.toLowerCase();
    }
    
    setFormData({ ...formData, [name]: value });
    setErrors({ ...errors, [name]: '' });
  };

  const validate = () => {
    const newErrors = {};
    
    if (!formData.email) {
      newErrors.email = 'El correo electrónico es requerido';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Ingresa un correo electrónico válido';
    }
    
    if (!formData.password) {
      newErrors.password = 'La contraseña es requerida';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validate()) return;
    
    setLoading(true);
    
    try {
      const response = await api.post(ENDPOINTS.LOGIN, {
        email: formData.email,
        password: formData.password,
      });

      if (response.data.success) {
        const userData = response.data.data;
        
        login({
          id: userData.id,
          email: userData.email,
          nombre: userData.nombre,
          apellido: userData.apellido,
          isAdmin: userData.isAdmin || false,
        });

        Swal.fire({
          icon: 'success',
          title: userData.isAdmin ? '¡Bienvenido Administrador!' : '¡Bienvenido!',
          timer: 1500,
          showConfirmButton: false,
        });

        navigate(userData.isAdmin ? '/admin' : '/');
      } else {
        setErrors({ general: response.data.message || 'Credenciales inválidas' });
      }
    } catch (error) {
      const message = error.response?.data?.message || 'Error al iniciar sesión. Intenta de nuevo.';
      setErrors({ general: message });
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="container d-flex align-items-center justify-content-center py-5" style={{ minHeight: 'calc(100vh - 140px)' }}>
      <div className="card shadow-sm p-4 w-100" style={{ maxWidth: '420px' }}>
        <h2 className="mb-3 text-center fw-bold text-primary">Travelio</h2>
        <h5 className="mb-4 text-center text-muted">Bienvenido de nuevo</h5>

        <form onSubmit={handleSubmit} noValidate>
          {errors.general && (
            <div className="alert alert-danger py-2 small">{errors.general}</div>
          )}

          <div className="mb-3">
            <label className="form-label">Correo electrónico</label>
            <input
              type="email"
              name="email"
              className={`form-control ${errors.email ? 'is-invalid' : ''}`}
              placeholder="nombre@ejemplo.com"
              maxLength={100}
              autoComplete="email"
              value={formData.email}
              onChange={handleChange}
            />
            {errors.email && <div className="text-danger small">{errors.email}</div>}
          </div>

          <div className="mb-4">
            <label className="form-label">Contraseña</label>
            <div className="input-group">
              <input
                type={showPassword ? 'text' : 'password'}
                name="password"
                className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                placeholder="••••••••"
                maxLength={100}
                value={formData.password}
                onChange={handleChange}
              />
              <button
                className="btn btn-outline-secondary"
                type="button"
                onClick={() => setShowPassword(!showPassword)}
              >
                <i className={`bi ${showPassword ? 'bi-eye-slash' : 'bi-eye'}`}></i>
              </button>
            </div>
            {errors.password && <div className="text-danger small">{errors.password}</div>}
          </div>

          <button 
            className="btn btn-primary w-100 py-2 fw-bold" 
            type="submit"
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="spinner-border spinner-border-sm me-2"></span>
                Iniciando...
              </>
            ) : (
              'Iniciar sesión'
            )}
          </button>
        </form>

        <hr className="my-4" />
        <p className="text-center mb-0">
          ¿Nuevo aquí? <Link to="/register" className="fw-bold text-decoration-none">Crear cuenta</Link>
        </p>
        
        <div className="mt-3 text-center">
          <small className="text-muted">
            Admin demo: admin@admin.com / Admin123!
          </small>
        </div>
      </div>
    </section>
  );
};

export default Login;
