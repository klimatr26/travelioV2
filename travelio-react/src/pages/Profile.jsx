import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import useAuthStore from '../store/authStore';
import PageHeader from '../components/common/PageHeader';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';
import Swal from 'sweetalert2';

const Profile = () => {
  const navigate = useNavigate();
  const { user, isLoggedIn } = useAuthStore();
  const [activeTab, setActiveTab] = useState('profile');
  const [loading, setLoading] = useState(false);
  const [loadingProfile, setLoadingProfile] = useState(false);
  const [orders, setOrders] = useState([]);
  const [reservas, setReservas] = useState([]);
  const [profileData, setProfileData] = useState(null);
  const [cancelando, setCancelando] = useState(null);

  // Cargar historial de compras
  const loadOrders = useCallback(async () => {
    if (!user?.id) return;
    setLoading(true);
    try {
      const response = await api.get(`${ENDPOINTS.CHECKOUT_HISTORIAL}/${user.id}`);
      if (response.data.success) {
        setOrders(response.data.data || []);
      }
    } catch (error) {
      console.error('Error cargando historial:', error);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  // Cargar reservas activas
  const loadReservas = useCallback(async () => {
    if (!user?.id) return;
    try {
      const response = await api.get(`${ENDPOINTS.CHECKOUT_RESERVAS}/${user.id}`);
      if (response.data.success) {
        setReservas(response.data.data || []);
      }
    } catch (error) {
      console.error('Error cargando reservas:', error);
    }
  }, [user?.id]);

  // Cargar perfil completo
  const loadProfile = useCallback(async () => {
    if (!user?.id || user.id === -1) return; // -1 es admin
    setLoadingProfile(true);
    try {
      const response = await api.get(`${ENDPOINTS.PROFILE}/${user.id}`);
      if (response.data.success) {
        setProfileData(response.data.data);
      }
    } catch (error) {
      console.error('Error cargando perfil:', error);
    } finally {
      setLoadingProfile(false);
    }
  }, [user?.id]);

  // Cancelar una reserva
  const handleCancelarReserva = async (reservaId) => {
    const result = await Swal.fire({
      title: '¿Cancelar reserva?',
      text: 'Esta acción no se puede deshacer. Se intentará reembolsar el pago.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, cancelar',
      cancelButtonText: 'No, mantener'
    });

    if (!result.isConfirmed) return;

    setCancelando(reservaId);
    try {
      const response = await api.post(`${ENDPOINTS.CHECKOUT_CANCELAR}/${reservaId}`, {
        clienteId: user.id,
        numeroCuentaBancaria: 0 // Por ahora sin cuenta específica
      });

      if (response.data.success) {
        Swal.fire({
          icon: 'success',
          title: 'Reserva cancelada',
          text: response.data.data?.montoReembolsado 
            ? `Se reembolsaron $${response.data.data.montoReembolsado.toFixed(2)}`
            : response.data.message,
        });
        loadReservas(); // Recargar lista
      } else {
        Swal.fire('Error', response.data.message || 'No se pudo cancelar', 'error');
      }
    } catch (error) {
      console.error('Error cancelando reserva:', error);
      Swal.fire('Error', 'Error al cancelar la reserva', 'error');
    } finally {
      setCancelando(null);
    }
  };

  // Ver factura del proveedor (abre en nueva pestaña)
  const handleVerFacturaProveedor = (facturaUrl) => {
    if (facturaUrl) {
      window.open(facturaUrl, '_blank');
    } else {
      Swal.fire('Info', 'La factura del proveedor no está disponible', 'info');
    }
  };

  // Ver/Descargar factura de Travelio
  const handleVerFacturaTravelio = async (compraId) => {
    if (!compraId) {
      Swal.fire('Info', 'No hay compra asociada a esta reserva', 'info');
      return;
    }

    try {
      const response = await api.get(`${ENDPOINTS.CHECKOUT_FACTURA_TRAVELIO}/${compraId}`);
      if (response.data.success) {
        const factura = response.data.data;
        
        // Mostrar factura en un modal elegante
        const itemsHtml = factura.items.map(item => `
          <tr>
            <td class="text-start">${item.descripcion}</td>
            <td><span class="badge bg-info">${item.tipo}</span></td>
            <td><code>${item.codigoReserva || 'N/A'}</code></td>
            <td class="text-end">$${item.precioUnitario.toFixed(2)}</td>
          </tr>
        `).join('');

        Swal.fire({
          title: `<i class="bi bi-receipt me-2"></i>Factura ${factura.numeroFactura}`,
          html: `
            <div class="text-start">
              <div class="mb-3 p-3 bg-light rounded">
                <h6 class="fw-bold text-primary mb-2"><i class="bi bi-building me-1"></i>TRAVELIO S.A.</h6>
                <small class="text-muted d-block">RUC: 0992123456001</small>
                <small class="text-muted d-block">Dir: Cuenca, Ecuador</small>
              </div>
              
              <div class="mb-3 p-3 border rounded">
                <h6 class="fw-bold mb-2"><i class="bi bi-person me-1"></i>Cliente</h6>
                <p class="mb-1"><strong>${factura.cliente.nombre}</strong></p>
                <small class="text-muted">${factura.cliente.tipoDocumento}: ${factura.cliente.documento}</small><br>
                <small class="text-muted">${factura.cliente.correo}</small>
              </div>

              <div class="mb-3">
                <small class="text-muted">
                  <i class="bi bi-calendar me-1"></i>Fecha: ${new Date(factura.fechaEmision).toLocaleDateString('es-ES', { year: 'numeric', month: 'long', day: 'numeric' })}
                </small>
              </div>
              
              <table class="table table-sm table-bordered">
                <thead class="table-light">
                  <tr>
                    <th class="text-start">Descripción</th>
                    <th>Tipo</th>
                    <th>Código</th>
                    <th class="text-end">Precio</th>
                  </tr>
                </thead>
                <tbody>
                  ${itemsHtml}
                </tbody>
                <tfoot class="table-light">
                  <tr>
                    <td colspan="3" class="text-end"><strong>Subtotal:</strong></td>
                    <td class="text-end">$${factura.subtotal.toFixed(2)}</td>
                  </tr>
                  <tr>
                    <td colspan="3" class="text-end"><strong>IVA (${factura.porcentajeIva}%):</strong></td>
                    <td class="text-end">$${factura.iva.toFixed(2)}</td>
                  </tr>
                  <tr class="table-success">
                    <td colspan="3" class="text-end"><strong>TOTAL:</strong></td>
                    <td class="text-end fw-bold">$${factura.total.toFixed(2)}</td>
                  </tr>
                </tfoot>
              </table>

              <div class="d-flex justify-content-between small text-muted mt-3">
                <span><i class="bi bi-credit-card me-1"></i>${factura.metodoPago}</span>
                <span class="badge bg-success">${factura.estadoPago}</span>
              </div>
            </div>
          `,
          width: 600,
          showCloseButton: true,
          showConfirmButton: true,
          confirmButtonText: '<i class="bi bi-printer me-1"></i>Imprimir',
          confirmButtonColor: '#198754',
          didOpen: () => {
            // Configurar impresión
            const confirmBtn = Swal.getConfirmButton();
            confirmBtn.addEventListener('click', () => {
              window.print();
            });
          }
        });
      } else {
        Swal.fire('Error', response.data.message || 'No se pudo cargar la factura', 'error');
      }
    } catch (error) {
      console.error('Error cargando factura:', error);
      Swal.fire('Error', 'Error al cargar la factura de Travelio', 'error');
    }
  };

  useEffect(() => {
    if (!isLoggedIn) {
      navigate('/login');
    }
  }, [isLoggedIn, navigate]);

  useEffect(() => {
    if (isLoggedIn && user?.id && activeTab === 'orders') {
      loadOrders();
      loadReservas();
    }
  }, [isLoggedIn, user?.id, activeTab, loadOrders, loadReservas]);

  // Cargar perfil al montar o cambiar a tab de perfil
  useEffect(() => {
    if (isLoggedIn && user?.id && activeTab === 'profile') {
      loadProfile();
    }
  }, [isLoggedIn, user?.id, activeTab, loadProfile]);

  if (!isLoggedIn || !user) {
    return null;
  }

  return (
    <div className="container py-4">
      <PageHeader
        title="Mi Perfil"
        subtitle={`Bienvenido de vuelta, ${user.nombres || user.email}`}
        icon="bi-person-circle"
        gradient="info"
      />

      <div className="row">
        {/* Sidebar */}
        <div className="col-md-3 mb-4">
          <div className="card shadow-sm">
            <div className="card-body text-center">
              <div 
                className="rounded-circle mx-auto mb-3 d-flex align-items-center justify-content-center"
                style={{ 
                  width: '100px', 
                  height: '100px', 
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  fontSize: '2.5rem',
                  color: 'white'
                }}
              >
                {user.nombres ? user.nombres[0].toUpperCase() : user.email[0].toUpperCase()}
              </div>
              <h5 className="fw-bold mb-1">{user.nombres} {user.apellidos}</h5>
              <p className="text-muted small">{user.email}</p>
              <span className="badge bg-success">Cliente verificado</span>
            </div>
            <ul className="list-group list-group-flush">
              <li 
                className={`list-group-item list-group-item-action ${activeTab === 'profile' ? 'active' : ''}`}
                onClick={() => setActiveTab('profile')}
                style={{ cursor: 'pointer' }}
              >
                <i className="bi bi-person me-2"></i> Información Personal
              </li>
              <li 
                className={`list-group-item list-group-item-action ${activeTab === 'orders' ? 'active' : ''}`}
                onClick={() => setActiveTab('orders')}
                style={{ cursor: 'pointer' }}
              >
                <i className="bi bi-receipt me-2"></i> Mis Reservas
              </li>
              <li 
                className={`list-group-item list-group-item-action ${activeTab === 'security' ? 'active' : ''}`}
                onClick={() => setActiveTab('security')}
                style={{ cursor: 'pointer' }}
              >
                <i className="bi bi-shield-lock me-2"></i> Seguridad
              </li>
            </ul>
          </div>
        </div>

        {/* Main Content */}
        <div className="col-md-9">
          {/* Información Personal */}
          {activeTab === 'profile' && (
            <div className="card shadow-sm">
              <div className="card-header bg-white d-flex justify-content-between align-items-center">
                <h5 className="mb-0"><i className="bi bi-person me-2"></i>Información Personal</h5>
                {loadingProfile && (
                  <div className="spinner-border spinner-border-sm text-primary" role="status">
                    <span className="visually-hidden">Cargando...</span>
                  </div>
                )}
              </div>
              <div className="card-body">
                <form>
                  <div className="row g-3">
                    <div className="col-md-6">
                      <label className="form-label text-muted">Nombres</label>
                      <input 
                        type="text" 
                        className="form-control bg-light" 
                        value={profileData?.nombre || user.nombres || ''} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">Apellidos</label>
                      <input 
                        type="text" 
                        className="form-control bg-light" 
                        value={profileData?.apellido || user.apellidos || ''} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">Email</label>
                      <input 
                        type="email" 
                        className="form-control bg-light" 
                        value={profileData?.email || user.email || ''} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">Teléfono</label>
                      <input 
                        type="tel" 
                        className="form-control bg-light" 
                        value={profileData?.telefono || 'No registrado'} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">Tipo de Documento</label>
                      <input 
                        type="text" 
                        className="form-control bg-light" 
                        value={profileData?.tipoIdentificacion || 'N/A'} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">Número de Documento</label>
                      <input 
                        type="text" 
                        className="form-control bg-light" 
                        value={profileData?.documentoIdentidad || 'N/A'} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">Fecha de Nacimiento</label>
                      <input 
                        type="text" 
                        className="form-control bg-light" 
                        value={profileData?.fechaNacimiento 
                          ? new Date(profileData.fechaNacimiento).toLocaleDateString('es-ES', { year: 'numeric', month: 'long', day: 'numeric' })
                          : 'No registrada'} 
                        readOnly 
                      />
                    </div>
                    <div className="col-md-6">
                      <label className="form-label text-muted">País</label>
                      <input 
                        type="text" 
                        className="form-control bg-light" 
                        value={profileData?.pais || 'No registrado'} 
                        readOnly 
                      />
                    </div>
                  </div>
                  <div className="mt-4 p-3 bg-light rounded border">
                    <i className="bi bi-info-circle me-2 text-primary"></i>
                    <span className="text-muted">Para modificar tus datos, por favor contacta a soporte.</span>
                  </div>
                </form>
              </div>
            </div>
          )}

          {/* Mis Reservas */}
          {activeTab === 'orders' && (
            <div className="card shadow-sm">
              <div className="card-header bg-white">
                <h5 className="mb-0"><i className="bi bi-receipt me-2"></i>Mis Reservas</h5>
              </div>
              <div className="card-body">
                {loading ? (
                  <div className="text-center py-5">
                    <div className="spinner-border text-primary" role="status">
                      <span className="visually-hidden">Cargando...</span>
                    </div>
                  </div>
                ) : orders.length === 0 && reservas.length === 0 ? (
                  <div className="text-center py-5">
                    <i className="bi bi-cart-x display-1 text-muted"></i>
                    <p className="text-muted mt-3">No tienes reservas todavía</p>
                    <button className="btn btn-primary" onClick={() => navigate('/')}>
                      Explorar Servicios
                    </button>
                  </div>
                ) : (
                  <>
                    {/* Reservas activas */}
                    {reservas.length > 0 && (
                      <div className="mb-4">
                        <h6 className="fw-bold text-primary mb-3">
                          <i className="bi bi-calendar-check me-2"></i>Reservas Activas ({reservas.length})
                        </h6>
                        <div className="row row-cols-1 row-cols-md-2 g-3">
                          {reservas.map((reserva) => (
                            <div key={reserva.id} className="col">
                              <div className="card border h-100">
                                <div className="card-body">
                                  <div className="d-flex justify-content-between align-items-start mb-2">
                                    <span className={`badge ${
                                      reserva.tipo === 'HOTEL' ? 'bg-success' :
                                      reserva.tipo === 'AUTO' ? 'bg-warning text-dark' :
                                      reserva.tipo === 'VUELO' ? 'bg-primary' :
                                      reserva.tipo === 'RESTAURANTE' ? 'bg-danger' :
                                      reserva.tipo === 'PAQUETE' ? 'bg-info' : 'bg-secondary'
                                    }`}>
                                      {reserva.tipo === 'HOTEL' && <i className="bi bi-building me-1"></i>}
                                      {reserva.tipo === 'AUTO' && <i className="bi bi-car-front me-1"></i>}
                                      {reserva.tipo === 'VUELO' && <i className="bi bi-airplane me-1"></i>}
                                      {reserva.tipo === 'RESTAURANTE' && <i className="bi bi-cup-hot me-1"></i>}
                                      {reserva.tipo === 'PAQUETE' && <i className="bi bi-box-seam me-1"></i>}
                                      {reserva.tipo}
                                    </span>
                                    <span className="badge bg-success">{reserva.estado}</span>
                                  </div>
                                  <h6 className="card-title fw-bold mb-2">{reserva.servicio}</h6>
                                  <div className="small text-muted mb-2">
                                    <div><i className="bi bi-calendar me-1"></i>Reservado: {reserva.fechaReserva ? new Date(reserva.fechaReserva).toLocaleDateString('es-ES') : 'N/A'}</div>
                                    {reserva.fechaInicio && (
                                      <div><i className="bi bi-calendar-event me-1"></i>Inicio: {new Date(reserva.fechaInicio).toLocaleDateString('es-ES')}</div>
                                    )}
                                  </div>
                                  <div className="d-flex justify-content-between align-items-center">
                                    <div>
                                      <code className="bg-light px-2 py-1 rounded small">{reserva.codigoConfirmacion || 'Sin código'}</code>
                                    </div>
                                    <div className="fw-bold text-success">${reserva.precioTotal?.toFixed(2) || '0.00'}</div>
                                  </div>
                                </div>
                                <div className="card-footer bg-white border-top">
                                  {/* Botones de Facturas */}
                                  <div className="d-flex gap-2 mb-2">
                                    <button 
                                      className="btn btn-outline-primary btn-sm flex-fill"
                                      onClick={() => handleVerFacturaTravelio(reserva.compraId)}
                                      title="Ver factura de Travelio"
                                    >
                                      <i className="bi bi-receipt me-1"></i>F. Travelio
                                    </button>
                                    <button 
                                      className={`btn btn-sm flex-fill ${reserva.facturaProveedorUrl ? 'btn-outline-info' : 'btn-outline-secondary'}`}
                                      onClick={() => handleVerFacturaProveedor(reserva.facturaProveedorUrl)}
                                      title="Ver factura del proveedor"
                                      disabled={!reserva.facturaProveedorUrl}
                                    >
                                      <i className="bi bi-file-earmark-text me-1"></i>F. Proveedor
                                    </button>
                                  </div>
                                  {/* Botón Cancelar */}
                                  <button 
                                    className="btn btn-outline-danger btn-sm w-100"
                                    onClick={() => handleCancelarReserva(reserva.id)}
                                    disabled={cancelando === reserva.id}
                                  >
                                    {cancelando === reserva.id ? (
                                      <><span className="spinner-border spinner-border-sm me-2"></span>Cancelando...</>
                                    ) : (
                                      <><i className="bi bi-x-circle me-1"></i>Cancelar Reserva</>
                                    )}
                                  </button>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Historial de compras */}
                    {orders.length > 0 && (
                      <div>
                        <h6 className="fw-bold text-secondary mb-3">
                          <i className="bi bi-clock-history me-2"></i>Historial de Compras
                        </h6>
                        <div className="table-responsive">
                          <table className="table table-hover">
                            <thead className="table-light">
                              <tr>
                                <th>#ID</th>
                                <th>Fecha</th>
                                <th>Total</th>
                                <th>Estado</th>
                              </tr>
                            </thead>
                            <tbody>
                              {orders.map((order) => (
                                <tr key={order.id}>
                                  <td><strong>#{order.id}</strong></td>
                                  <td>{order.fecha ? new Date(order.fecha).toLocaleDateString() : 'N/A'}</td>
                                  <td className="text-success fw-bold">${order.total?.toFixed(2)}</td>
                                  <td>
                                    <span className="badge bg-success">{order.estado}</span>
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </div>
                    )}
                  </>
                )}
              </div>
            </div>
          )}

          {/* Seguridad */}
          {activeTab === 'security' && (
            <div className="card shadow-sm">
              <div className="card-header bg-white">
                <h5 className="mb-0"><i className="bi bi-shield-lock me-2"></i>Seguridad</h5>
              </div>
              <div className="card-body">
                <form>
                  <div className="mb-3">
                    <label className="form-label">Contraseña Actual</label>
                    <input type="password" className="form-control" />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Nueva Contraseña</label>
                    <input type="password" className="form-control" />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Confirmar Nueva Contraseña</label>
                    <input type="password" className="form-control" />
                  </div>
                  <button type="button" className="btn btn-primary" disabled>
                    <i className="bi bi-lock me-2"></i>Cambiar Contraseña
                  </button>
                  <p className="text-muted small mt-2">
                    <i className="bi bi-info-circle me-1"></i>
                    Funcionalidad próximamente disponible
                  </p>
                </form>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Profile;
