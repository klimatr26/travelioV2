import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import useAuthStore from '../store/authStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Checkout = () => {
  const navigate = useNavigate();
  const { user, isLoggedIn } = useAuthStore();
  const { items, getSubtotal, getIva, getTotal, clearCart } = useCartStore();
  const [loading, setLoading] = useState(false);
  
  const [formData, setFormData] = useState({
    nombreCompleto: user ? `${user.nombre} ${user.apellido || ''}`.trim() : '',
    tipoDocumento: 'Cedula',
    numeroDocumento: '',
    correo: user?.email || '',
    numeroCuentaBancaria: '',
  });

  const [errors, setErrors] = useState({});

  // Redireccionar si no está logueado o carrito vacío
  if (!isLoggedIn) {
    navigate('/login');
    return null;
  }

  if (items.length === 0) {
    navigate('/carrito');
    return null;
  }

  const handleChange = (e) => {
    let { name, value } = e.target;
    
    if (name === 'numeroCuentaBancaria') {
      value = value.replace(/\D/g, '');
    }
    
    setFormData({ ...formData, [name]: value });
    setErrors({ ...errors, [name]: '' });
  };

  const validate = () => {
    const newErrors = {};
    
    if (!formData.nombreCompleto.trim()) {
      newErrors.nombreCompleto = 'El nombre es requerido';
    }
    
    if (!formData.numeroDocumento.trim()) {
      newErrors.numeroDocumento = 'El documento es requerido';
    }
    
    if (!formData.correo) {
      newErrors.correo = 'El correo es requerido';
    } else if (!/\S+@\S+\.\S+/.test(formData.correo)) {
      newErrors.correo = 'Correo inválido';
    }
    
    if (!formData.numeroCuentaBancaria) {
      newErrors.numeroCuentaBancaria = 'El número de cuenta es requerido';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handlePagar = async () => {
    if (!validate()) return;

    const result = await Swal.fire({
      title: 'Confirmar Pago',
      html: `
        <p>Se realizará un cargo de <strong>${formatCurrency(getTotal())}</strong> a tu cuenta bancaria.</p>
        <p class="text-muted small">Este proceso puede tardar unos segundos.</p>
      `,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Confirmar Pago',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#198754',
      showLoaderOnConfirm: true,
      preConfirm: async () => {
        setLoading(true);
        try {
          // Preparar los items para el backend
          const checkoutData = {
            clienteId: user?.id || 0,
            nombreCompleto: formData.nombreCompleto,
            tipoDocumento: formData.tipoDocumento,
            numeroDocumento: formData.numeroDocumento,
            correo: formData.correo,
            numeroCuentaBancaria: parseInt(formData.numeroCuentaBancaria, 10),
            items: items.map(item => ({
              tipo: item.tipo,
              titulo: item.titulo,
              detalle: item.detalle || '',
              cantidad: item.cantidad,
              precioOriginal: item.precioOriginal || item.precioFinal,
              precioFinal: item.precioFinal,
              servicioId: item.servicioId || 0,
              idProducto: item.idProducto || '',
              fechaInicio: item.fechaInicio || null,
              fechaFin: item.fechaFin || null,
              numeroPersonas: item.numeroPersonas || 1,
              imagenUrl: item.imagenUrl || ''
            }))
          };

          const response = await api.post(ENDPOINTS.CHECKOUT_PROCESAR, checkoutData);
          
          if (response.data.success) {
            return { success: true, data: response.data.data };
          } else {
            Swal.showValidationMessage(response.data.message || 'Error al procesar el pago');
            return { success: false };
          }
        } catch (error) {
          console.error('Error checkout:', error);
          Swal.showValidationMessage(error.response?.data?.message || 'Error al procesar el pago');
          return { success: false };
        }
      },
      allowOutsideClick: () => !Swal.isLoading(),
    });

    setLoading(false);

    if (result.isConfirmed && result.value?.success) {
      clearCart();
      
      const resData = result.value.data;
      await Swal.fire({
        icon: 'success',
        title: '¡Pago exitoso!',
        html: `
          <p>Tu compra ha sido procesada correctamente.</p>
          ${resData?.compraId ? `<p><strong>Número de compra:</strong> #${resData.compraId}</p>` : ''}
          <p class="small text-muted">Recibirás un correo con los detalles de tu reserva.</p>
        `,
        confirmButtonColor: '#198754',
      });
      
      navigate('/profile');
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  const getTypeIcon = (tipo) => {
    switch (tipo) {
      case 'HOTEL': return 'bi-building text-success';
      case 'CAR': return 'bi-car-front text-primary';
      case 'FLIGHT': return 'bi-airplane text-warning';
      case 'RESTAURANT': return 'bi-cup-straw text-danger';
      case 'PACKAGE': return 'bi-backpack2 text-info';
      default: return 'bi-ticket-perforated text-secondary';
    }
  };

  return (
    <section className="container py-5">
      <h2 className="fw-bold mb-4">
        <i className="bi bi-credit-card"></i> Finalizar Compra
      </h2>

      <div className="row g-4">
        {/* Resumen del pedido */}
        <div className="col-lg-7">
          <div className="card shadow-sm border-0 mb-4">
            <div className="card-header bg-white">
              <h5 className="mb-0 fw-bold">
                <i className="bi bi-box-seam"></i> Resumen del Pedido
              </h5>
            </div>
            <ul className="list-group list-group-flush">
              {items.map((item, index) => (
                <li key={index} className="list-group-item d-flex justify-content-between align-items-center py-3">
                  <div className="d-flex align-items-center gap-3">
                    {item.imagenUrl ? (
                      <img
                        src={item.imagenUrl}
                        alt={item.titulo}
                        className="rounded"
                        style={{ width: '60px', height: '60px', objectFit: 'cover' }}
                      />
                    ) : (
                      <div
                        className="bg-light rounded d-flex align-items-center justify-content-center"
                        style={{ width: '60px', height: '60px', fontSize: '1.5rem' }}
                      >
                        <i className={`bi ${getTypeIcon(item.tipo)}`}></i>
                      </div>
                    )}
                    <div>
                      <h6 className="mb-1 fw-bold">{item.titulo}</h6>
                      <small className="text-muted">{item.detalle}</small>
                    </div>
                  </div>
                  <div className="text-end">
                    <span className="fw-bold">{formatCurrency(item.precioFinal)}</span>
                  </div>
                </li>
              ))}
            </ul>
          </div>

          {/* Formulario de datos de facturación */}
          <div className="card shadow-sm border-0">
            <div className="card-header bg-white">
              <h5 className="mb-0 fw-bold">
                <i className="bi bi-file-text"></i> Datos de Facturación
              </h5>
            </div>
            <div className="card-body">
              <div className="row g-3">
                <div className="col-12">
                  <label className="form-label">Nombre completo</label>
                  <input
                    type="text"
                    name="nombreCompleto"
                    className={`form-control ${errors.nombreCompleto ? 'is-invalid' : ''}`}
                    value={formData.nombreCompleto}
                    onChange={handleChange}
                  />
                  {errors.nombreCompleto && <div className="text-danger small">{errors.nombreCompleto}</div>}
                </div>

                <div className="col-md-6">
                  <label className="form-label">Tipo de documento</label>
                  <select
                    name="tipoDocumento"
                    className="form-select"
                    value={formData.tipoDocumento}
                    onChange={handleChange}
                  >
                    <option value="Cedula">Cédula</option>
                    <option value="RUC">RUC</option>
                    <option value="Pasaporte">Pasaporte</option>
                  </select>
                </div>

                <div className="col-md-6">
                  <label className="form-label">Número de documento</label>
                  <input
                    type="text"
                    name="numeroDocumento"
                    className={`form-control ${errors.numeroDocumento ? 'is-invalid' : ''}`}
                    value={formData.numeroDocumento}
                    onChange={handleChange}
                  />
                  {errors.numeroDocumento && <div className="text-danger small">{errors.numeroDocumento}</div>}
                </div>

                <div className="col-12">
                  <label className="form-label">Correo electrónico</label>
                  <input
                    type="email"
                    name="correo"
                    className={`form-control ${errors.correo ? 'is-invalid' : ''}`}
                    value={formData.correo}
                    onChange={handleChange}
                  />
                  {errors.correo && <div className="text-danger small">{errors.correo}</div>}
                  <small className="text-muted">Aquí recibirás tus facturas y confirmaciones.</small>
                </div>

                <div className="col-12">
                  <hr className="my-3" />
                  <h6 className="fw-bold mb-3">
                    <i className="bi bi-bank"></i> Datos Bancarios
                  </h6>
                </div>

                <div className="col-12">
                  <label className="form-label">Número de cuenta bancaria</label>
                  <input
                    type="text"
                    name="numeroCuentaBancaria"
                    className={`form-control ${errors.numeroCuentaBancaria ? 'is-invalid' : ''}`}
                    placeholder="Ej: 12345"
                    value={formData.numeroCuentaBancaria}
                    onChange={handleChange}
                  />
                  {errors.numeroCuentaBancaria && <div className="text-danger small">{errors.numeroCuentaBancaria}</div>}
                  <small className="text-muted">Ingresa el número de tu cuenta en MiBanca para realizar el pago.</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Resumen de pago */}
        <div className="col-lg-5">
          <div className="card border-0 shadow-sm sticky-top" style={{ top: '2rem' }}>
            <div className="card-body p-4">
              <h5 className="fw-bold mb-4">Resumen de Pago</h5>

              <div className="d-flex justify-content-between mb-2">
                <span className="text-muted">Subtotal ({items.length} items)</span>
                <span className="fw-bold">{formatCurrency(getSubtotal())}</span>
              </div>

              <div className="d-flex justify-content-between mb-3 pb-3 border-bottom">
                <span className="text-muted">IVA (12%)</span>
                <span className="fw-bold">{formatCurrency(getIva())}</span>
              </div>

              <div className="d-flex justify-content-between align-items-center mb-4">
                <span className="h5 fw-bold mb-0">Total a Pagar</span>
                <span className="h3 fw-bold text-primary mb-0">{formatCurrency(getTotal())}</span>
              </div>

              <button
                type="button"
                className="btn btn-success w-100 btn-lg fw-bold"
                onClick={handlePagar}
                disabled={loading}
              >
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Procesando...
                  </>
                ) : (
                  <>
                    <i className="bi bi-lock-fill me-2"></i> Pagar Ahora
                  </>
                )}
              </button>

              <div className="mt-3 text-center">
                <small className="text-muted">
                  <i className="bi bi-shield-check"></i> Pago seguro procesado por MiBanca
                </small>
              </div>

              <hr className="my-4" />

              <div className="small text-muted">
                <p className="mb-2"><strong>Al realizar el pago aceptas:</strong></p>
                <ul className="ps-3 mb-0">
                  <li>Las políticas de cancelación de cada proveedor</li>
                  <li>Los términos y condiciones de Travelio</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Checkout;
