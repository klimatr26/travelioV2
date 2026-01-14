import { useState, useEffect } from 'react';
import PageHeader from '../components/common/PageHeader';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Vuelos = () => {
  const addItem = useCartStore((state) => state.addItem);
  const [loading, setLoading] = useState(false);
  const [vuelos, setVuelos] = useState([]);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    origen: '',
    destino: '',
    fechaSalida: '',
    pasajeros: 1,
    tipoCabina: '',
  });

  // Modal de reserva con datos obligatorios
  const [showReservaModal, setShowReservaModal] = useState(false);
  const [vueloParaReserva, setVueloParaReserva] = useState(null);
  const [reservaData, setReservaData] = useState({
    fechaVuelo: '',
    pasajeros: 1,
  });
  const [reservaErrors, setReservaErrors] = useState({});
  const [verificandoDisponibilidad, setVerificandoDisponibilidad] = useState(false);

  // Cargar vuelos al montar
  useEffect(() => {
    buscarVuelos();
  }, []);

  const buscarVuelos = async (filtros = {}) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.origen) params.append('origen', filtros.origen);
      if (filtros.destino) params.append('destino', filtros.destino);
      if (filtros.fechaSalida) params.append('fechaSalida', filtros.fechaSalida);
      if (filtros.tipoCabina) params.append('tipoCabina', filtros.tipoCabina);
      if (filtros.pasajeros) params.append('pasajeros', filtros.pasajeros);

      const url = `${ENDPOINTS.VUELOS}${params.toString() ? '?' + params.toString() : ''}`;
      console.log('Fetching vuelos from:', url);
      
      const response = await api.get(url);
      console.log('API Response:', response.data);
      
      if (response.data.success) {
        setVuelos(response.data.data || []);
      } else {
        setError(response.data.message || 'Error al cargar vuelos');
      }
    } catch (err) {
      console.error('Error fetching vuelos:', err);
      setError(err.response?.data?.message || 'Error al conectar con el servidor');
    } finally {
      setLoading(false);
    }
  };

  const getAirlineColor = (aerolinea) => {
    const nombre = aerolinea?.toLowerCase() || '';
    if (nombre.includes('withfly')) return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
    if (nombre.includes('astra')) return 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)';
    if (nombre.includes('skayward')) return 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)';
    if (nombre.includes('skyandes')) return 'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)';
    if (nombre.includes('caribbean')) return 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)';
    return 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)';
  };

  const handleSearch = (e) => {
    e.preventDefault();
    buscarVuelos(filters);
  };

  // Abrir modal de reserva con datos obligatorios
  const handleOpenReservaModal = (vuelo) => {
    setVueloParaReserva(vuelo);
    setReservaData({
      fechaVuelo: filters.fechaSalida || (vuelo.fecha ? vuelo.fecha.split('T')[0] : ''),
      pasajeros: filters.pasajeros || 1,
    });
    setReservaErrors({});
    setShowReservaModal(true);
  };

  // Validar y agregar al carrito
  const handleConfirmReserva = async () => {
    const errors = {};
    
    if (!reservaData.fechaVuelo) {
      errors.fechaVuelo = 'La fecha del vuelo es obligatoria';
    } else {
      const fecha = new Date(reservaData.fechaVuelo);
      const hoy = new Date();
      hoy.setHours(0, 0, 0, 0);
      
      if (fecha < hoy) {
        errors.fechaVuelo = 'La fecha no puede ser en el pasado';
      }
    }
    
    if (!reservaData.pasajeros || reservaData.pasajeros < 1) {
      errors.pasajeros = 'Debe indicar al menos 1 pasajero';
    }
    
    if (vueloParaReserva && reservaData.pasajeros > vueloParaReserva.asientosDisponibles) {
      errors.pasajeros = `Solo hay ${vueloParaReserva.asientosDisponibles} asientos disponibles`;
    }

    if (Object.keys(errors).length > 0) {
      setReservaErrors(errors);
      return;
    }

    // Verificar disponibilidad de asientos antes de agregar
    setVerificandoDisponibilidad(true);
    try {
      const response = await api.post(`${ENDPOINTS.VUELOS}/disponibilidad`, {
        servicioId: vueloParaReserva.servicioId,
        idVuelo: vueloParaReserva.idVuelo,
        fechaVuelo: reservaData.fechaVuelo,
        asientosRequeridos: parseInt(reservaData.pasajeros)
      });

      if (!response.data.disponible) {
        setReservaErrors({ general: 'No hay suficientes asientos disponibles para este vuelo' });
        Swal.fire({
          icon: 'warning',
          title: 'No Disponible',
          text: 'No hay suficientes asientos disponibles. Por favor, intenta con menos pasajeros u otro vuelo.',
        });
        setVerificandoDisponibilidad(false);
        return;
      }
    } catch (err) {
      console.warn('No se pudo verificar disponibilidad:', err);
      // Continuar si falla la verificación
    } finally {
      setVerificandoDisponibilidad(false);
    }

    // Agregar al carrito con datos validados
    const fechaVuelo = new Date(reservaData.fechaVuelo);
    addItem({
      tipo: 'FLIGHT',
      titulo: `${vueloParaReserva.nombreAerolinea} - ${vueloParaReserva.origen} → ${vueloParaReserva.destino}`,
      detalle: `${vueloParaReserva.tipoCabina} - ${fechaVuelo.toLocaleDateString()} - ${reservaData.pasajeros} pasajero(s)`,
      precioOriginal: vueloParaReserva.precioNormal,
      precioFinal: vueloParaReserva.precioActual,
      unidadPrecio: '/persona',
      servicioId: vueloParaReserva.servicioId,
      idProducto: vueloParaReserva.idVuelo,
      imagenUrl: null,
      fechaInicio: fechaVuelo,
      numeroPersonas: parseInt(reservaData.pasajeros),
    });

    setShowReservaModal(false);
    setVueloParaReserva(null);

    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: `Vuelo ${vueloParaReserva.nombreAerolinea} agregado al carrito`,
      showConfirmButton: false,
      timer: 2000,
    });
  };

  // Calcular precio total
  const calcularPrecioTotal = () => {
    if (vueloParaReserva && reservaData.pasajeros > 0) {
      return vueloParaReserva.precioActual * parseInt(reservaData.pasajeros);
    }
    return 0;
  };

  // Obtener fecha mínima (hoy)
  const getFechaMinima = () => {
    return new Date().toISOString().split('T')[0];
  };

  return (
    <div className="container py-4">
      <PageHeader
        title="Vuelos"
        subtitle="5 aerolíneas integradas - Económica, Ejecutiva y Primera Clase"
        icon="bi-airplane"
        gradient="danger"
      />

      {/* Formulario de búsqueda mejorado */}
      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <form onSubmit={handleSearch}>
            <div className="row g-3">
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-geo-alt me-1"></i>Origen
                </label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Ciudad origen"
                  value={filters.origen}
                  onChange={(e) => setFilters({ ...filters, origen: e.target.value })}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-geo-alt-fill me-1"></i>Destino
                </label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Ciudad destino"
                  value={filters.destino}
                  onChange={(e) => setFilters({ ...filters, destino: e.target.value })}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-calendar-event me-1"></i>Fecha
                </label>
                <input
                  type="date"
                  className="form-control"
                  min={getFechaMinima()}
                  value={filters.fechaSalida}
                  onChange={(e) => setFilters({ ...filters, fechaSalida: e.target.value })}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-people me-1"></i>Pasajeros
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="1"
                  max="10"
                  value={filters.pasajeros}
                  onChange={(e) => {
                    const val = parseInt(e.target.value) || 1;
                    setFilters({ ...filters, pasajeros: Math.max(1, Math.min(10, val)) });
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-star me-1"></i>Cabina
                </label>
                <select
                  className="form-select"
                  value={filters.tipoCabina}
                  onChange={(e) => setFilters({ ...filters, tipoCabina: e.target.value })}
                >
                  <option value="">Todas</option>
                  <option value="Economica">Económica</option>
                  <option value="Ejecutiva">Ejecutiva</option>
                  <option value="Primera">Primera Clase</option>
                </select>
              </div>
              <div className="col-md-2 d-flex align-items-end">
                <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                  {loading ? <span className="spinner-border spinner-border-sm"></span> : '✈️ Buscar'}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>

      {/* Error message */}
      {error && (
        <div className="alert alert-warning d-flex align-items-center mb-4">
          <i className="bi bi-exclamation-triangle me-2"></i>
          <div>
            <strong>Nota:</strong> {error}
            <button className="btn btn-sm btn-outline-warning ms-3" onClick={() => buscarVuelos()}>
              Reintentar
            </button>
          </div>
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="text-center py-5">
          <div className="spinner-border text-warning" role="status">
            <span className="visually-hidden">Cargando...</span>
          </div>
          <p className="mt-3 text-muted">Conectando con aerolíneas...</p>
        </div>
      )}

      {/* Resultados */}
      {!loading && (
        <>
          <p className="text-muted mb-3">{vuelos.length} vuelos encontrados</p>

          <div className="row g-3">
            {vuelos.map((vuelo, index) => {
              const fechaVuelo = vuelo.fecha ? new Date(vuelo.fecha) : null;
              const tieneDescuento = vuelo.precioActual < vuelo.precioNormal && vuelo.precioNormal > 0;
              
              return (
                <div key={`${vuelo.servicioId}-${vuelo.idVuelo}-${index}`} className="col-12">
                  <div className="card shadow-sm flight-card border-0">
                    <div className="card-body p-4">
                      <div className="row align-items-center">
                        <div className="col-md-3">
                          <div className="d-flex align-items-center gap-3">
                            <div
                              className="rounded-circle d-flex align-items-center justify-content-center"
                              style={{ width: '60px', height: '60px', background: getAirlineColor(vuelo.nombreAerolinea) }}
                            >
                              <span style={{ fontSize: '1.8rem' }}>✈️</span>
                            </div>
                            <div>
                              <h6 className="fw-bold mb-0">{vuelo.nombreAerolinea}</h6>
                              <span className="badge bg-info">{vuelo.tipoCabina}</span>
                            </div>
                          </div>
                        </div>

                        <div className="col-md-4">
                          <div className="d-flex align-items-center justify-content-center gap-3">
                            <div className="text-center">
                              <div className="fw-bold fs-4 text-primary">{vuelo.origen}</div>
                              <small className="text-muted">Origen</small>
                            </div>
                            <div className="text-center px-4">
                              <div className="border-top border-2 border-primary position-relative" style={{ width: '80px' }}>
                                <span className="position-absolute top-50 start-50 translate-middle bg-white px-2">
                                  <i className="bi bi-airplane"></i>
                                </span>
                              </div>
                              {fechaVuelo && (
                                <small className="text-muted d-block mt-2">
                                  {fechaVuelo.toLocaleDateString('es-ES', { day: '2-digit', month: 'short' })}
                                </small>
                              )}
                            </div>
                            <div className="text-center">
                              <div className="fw-bold fs-4 text-success">{vuelo.destino}</div>
                              <small className="text-muted">Destino</small>
                            </div>
                          </div>
                        </div>

                        <div className="col-md-2 text-center">
                          <div className="bg-light rounded-3 p-2">
                            <small className="text-muted d-block">Asientos</small>
                            <div className={`fw-bold fs-5 text-${vuelo.asientosDisponibles < 5 ? 'danger' : 'success'}`}>
                              {vuelo.asientosDisponibles || 'N/A'}
                            </div>
                            <small className="text-muted">disponibles</small>
                          </div>
                        </div>

                        <div className="col-md-3 text-end">
                          {tieneDescuento && (
                            <div className="text-muted text-decoration-line-through small">
                              ${vuelo.precioNormal?.toFixed(2)}
                            </div>
                          )}
                          <div className="fw-bold text-warning fs-3">${vuelo.precioActual?.toFixed(2)}</div>
                          <small className="text-muted">por persona</small>
                          {tieneDescuento && (
                            <span className="badge bg-danger ms-2">-{Math.round((1 - vuelo.precioActual / vuelo.precioNormal) * 100)}%</span>
                          )}
                          <div className="mt-2">
                            <button
                              className="btn btn-warning fw-bold"
                              onClick={() => handleOpenReservaModal(vuelo)}
                            >
                              Reservar ✈
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </>
      )}

      {/* Modal de Reserva de Vuelo */}
      {showReservaModal && vueloParaReserva && (
        <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-warning text-dark">
                <h5 className="modal-title">
                  <i className="bi bi-airplane me-2"></i>
                  Reservar Vuelo
                </h5>
                <button 
                  type="button" 
                  className="btn-close" 
                  onClick={() => setShowReservaModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                {/* Info del vuelo */}
                <div className="alert alert-light border mb-4">
                  <div className="d-flex align-items-center gap-3">
                    <div
                      className="rounded-circle d-flex align-items-center justify-content-center flex-shrink-0"
                      style={{ width: '50px', height: '50px', background: getAirlineColor(vueloParaReserva.nombreAerolinea) }}
                    >
                      <span style={{ fontSize: '1.5rem' }}>✈️</span>
                    </div>
                    <div>
                      <h6 className="fw-bold mb-1">{vueloParaReserva.nombreAerolinea}</h6>
                      <p className="mb-0 text-muted">
                        {vueloParaReserva.origen} → {vueloParaReserva.destino}
                      </p>
                      <span className="badge bg-info">{vueloParaReserva.tipoCabina}</span>
                    </div>
                  </div>
                </div>

                {/* Formulario de reserva */}
                <div className="mb-3">
                  <label className="form-label fw-bold">
                    <i className="bi bi-calendar-event me-1 text-warning"></i>
                    Fecha del Vuelo <span className="text-danger">*</span>
                  </label>
                  <input
                    type="date"
                    className={`form-control ${reservaErrors.fechaVuelo ? 'is-invalid' : ''}`}
                    min={getFechaMinima()}
                    value={reservaData.fechaVuelo}
                    onChange={(e) => setReservaData({ ...reservaData, fechaVuelo: e.target.value })}
                  />
                  {reservaErrors.fechaVuelo && (
                    <div className="invalid-feedback">{reservaErrors.fechaVuelo}</div>
                  )}
                </div>

                <div className="mb-3">
                  <label className="form-label fw-bold">
                    <i className="bi bi-people me-1 text-warning"></i>
                    Número de Pasajeros <span className="text-danger">*</span>
                  </label>
                  <input
                    type="number"
                    className={`form-control ${reservaErrors.pasajeros ? 'is-invalid' : ''}`}
                    min="1"
                    max={vueloParaReserva.asientosDisponibles || 10}
                    value={reservaData.pasajeros}
                    onChange={(e) => {
                      const val = parseInt(e.target.value) || 1;
                      setReservaData({ ...reservaData, pasajeros: Math.max(1, val) });
                    }}
                  />
                  {reservaErrors.pasajeros && (
                    <div className="invalid-feedback">{reservaErrors.pasajeros}</div>
                  )}
                  <small className="text-muted">
                    Máximo {vueloParaReserva.asientosDisponibles} asientos disponibles
                  </small>
                </div>

                {/* Resumen de precio */}
                {reservaData.pasajeros > 0 && (
                  <div className="bg-light rounded-3 p-3 mt-4">
                    <h6 className="fw-bold mb-3">
                      <i className="bi bi-receipt me-2"></i>Resumen de Precio
                    </h6>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Precio por persona:</span>
                      <span>${vueloParaReserva.precioActual?.toFixed(2)}</span>
                    </div>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Pasajeros:</span>
                      <span>x {reservaData.pasajeros}</span>
                    </div>
                    <hr />
                    <div className="d-flex justify-content-between">
                      <span className="fw-bold">Total Estimado:</span>
                      <span className="fw-bold text-warning fs-5">
                        ${calcularPrecioTotal().toFixed(2)}
                      </span>
                    </div>
                  </div>
                )}

                {/* Mensaje de error general */}
                {reservaErrors.general && (
                  <div className="alert alert-danger mt-3 mb-0">
                    <i className="bi bi-exclamation-triangle me-2"></i>
                    {reservaErrors.general}
                  </div>
                )}
              </div>
              <div className="modal-footer">
                <button 
                  type="button" 
                  className="btn btn-secondary" 
                  onClick={() => setShowReservaModal(false)}
                  disabled={verificandoDisponibilidad}
                >
                  Cancelar
                </button>
                <button 
                  type="button" 
                  className="btn btn-warning fw-bold"
                  onClick={handleConfirmReserva}
                  disabled={verificandoDisponibilidad}
                >
                  {verificandoDisponibilidad ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Verificando...
                    </>
                  ) : (
                    <>
                      <i className="bi bi-cart-plus me-1"></i>
                      Agregar al Carrito
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Vuelos;
