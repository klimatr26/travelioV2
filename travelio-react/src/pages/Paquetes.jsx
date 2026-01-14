import { useState, useEffect } from 'react';
import PageHeader from '../components/common/PageHeader';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Paquetes = () => {
  const addItem = useCartStore((state) => state.addItem);
  const [loading, setLoading] = useState(false);
  const [paquetes, setPaquetes] = useState([]);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    ciudad: '',
    fechaInicio: '',
    personas: 2,
    tipoActividad: '',
    precioMax: '',
  });

  // Modal de reserva con datos obligatorios
  const [showReservaModal, setShowReservaModal] = useState(false);
  const [paqueteParaReserva, setPaqueteParaReserva] = useState(null);
  const [reservaData, setReservaData] = useState({
    fechaInicio: '',
    personas: 2,
  });
  const [reservaErrors, setReservaErrors] = useState({});
  const [verificandoDisponibilidad, setVerificandoDisponibilidad] = useState(false);

  // Cargar paquetes al montar
  useEffect(() => {
    buscarPaquetes();
  }, []);

  const buscarPaquetes = async (filtros = {}) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.ciudad) params.append('ciudad', filtros.ciudad);
      if (filtros.fechaInicio) params.append('fechaInicio', filtros.fechaInicio);
      if (filtros.tipoActividad) params.append('tipoActividad', filtros.tipoActividad);
      if (filtros.precioMax) params.append('precioMax', filtros.precioMax);
      if (filtros.personas) params.append('personas', filtros.personas);

      const url = `${ENDPOINTS.PAQUETES}${params.toString() ? '?' + params.toString() : ''}`;
      console.log('Fetching paquetes from:', url);
      
      const response = await api.get(url);
      console.log('API Response:', response.data);
      
      if (response.data.success) {
        setPaquetes(response.data.data || []);
      } else {
        setError(response.data.message || 'Error al cargar paquetes');
      }
    } catch (err) {
      console.error('Error fetching paquetes:', err);
      setError(err.response?.data?.message || 'Error al conectar con el servidor');
    } finally {
      setLoading(false);
    }
  };

  const getActivityEmoji = (tipo) => {
    const t = tipo?.toLowerCase() || '';
    if (t.includes('aventura')) return '⛰️';
    if (t.includes('cultural')) return '🏛️';
    if (t.includes('gastronom')) return '🍽️';
    if (t.includes('relax')) return '🧘';
    if (t.includes('familiar')) return '👨‍👩‍👧‍👦';
    return '🎒';
  };

  const getFallbackImage = (tipo) => {
    const t = tipo?.toLowerCase() || '';
    if (t.includes('aventura')) return 'https://images.unsplash.com/photo-1530866495561-507c9faab2ed?w=400&h=250&fit=crop';
    if (t.includes('cultural')) return 'https://images.unsplash.com/photo-1467269204594-9661b134dd2b?w=400&h=250&fit=crop';
    if (t.includes('gastronom')) return 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=400&h=250&fit=crop';
    if (t.includes('relax')) return 'https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=400&h=250&fit=crop';
    return 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=250&fit=crop';
  };

  const handleSearch = (e) => {
    e.preventDefault();
    buscarPaquetes(filters);
  };

  // Abrir modal de reserva con datos obligatorios
  const handleOpenReservaModal = (paquete) => {
    setPaqueteParaReserva(paquete);
    setReservaData({
      fechaInicio: filters.fechaInicio || '',
      personas: filters.personas || 2,
    });
    setReservaErrors({});
    setShowReservaModal(true);
  };

  // Validar y agregar al carrito
  const handleConfirmReserva = async () => {
    const errors = {};
    
    if (!reservaData.fechaInicio) {
      errors.fechaInicio = 'La fecha de inicio es obligatoria';
    } else {
      const fecha = new Date(reservaData.fechaInicio);
      const hoy = new Date();
      hoy.setHours(0, 0, 0, 0);
      
      if (fecha < hoy) {
        errors.fechaInicio = 'La fecha no puede ser en el pasado';
      }
    }
    
    if (!reservaData.personas || reservaData.personas < 1) {
      errors.personas = 'Debe indicar al menos 1 persona';
    }
    
    if (paqueteParaReserva && reservaData.personas > paqueteParaReserva.capacidad) {
      errors.personas = `El paquete tiene capacidad máxima de ${paqueteParaReserva.capacidad} personas`;
    }

    if (Object.keys(errors).length > 0) {
      setReservaErrors(errors);
      return;
    }

    // Verificar disponibilidad del paquete antes de agregar
    setVerificandoDisponibilidad(true);
    try {
      const response = await api.post(`${ENDPOINTS.PAQUETES}/disponibilidad`, {
        servicioId: paqueteParaReserva.servicioId,
        idPaquete: paqueteParaReserva.idPaquete,
        fechaInicio: reservaData.fechaInicio,
        numeroPersonas: parseInt(reservaData.personas)
      });

      if (!response.data.disponible) {
        setReservaErrors({ general: 'El paquete no está disponible para las fechas seleccionadas' });
        Swal.fire({
          icon: 'warning',
          title: 'No Disponible',
          text: 'El paquete turístico no está disponible para la fecha y número de personas seleccionados.',
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

    // Obtener precio correcto (precioActual o precioNormal)
    const precioFinal = paqueteParaReserva.precioActual || paqueteParaReserva.precio || 0;
    const precioOriginal = paqueteParaReserva.precioNormal || precioFinal;
    
    // Agregar al carrito con datos validados
    const fechaInicio = new Date(reservaData.fechaInicio);
    addItem({
      tipo: 'PACKAGE',
      titulo: paqueteParaReserva.nombre,
      detalle: `${paqueteParaReserva.ciudad} - ${paqueteParaReserva.duracion || paqueteParaReserva.duracionDias} días - ${reservaData.personas} persona(s)`,
      precioOriginal: precioOriginal,
      precioFinal: precioFinal,
      unidadPrecio: '/persona',
      servicioId: paqueteParaReserva.servicioId,
      idProducto: paqueteParaReserva.idPaquete,
      imagenUrl: paqueteParaReserva.imagenUrl || getFallbackImage(paqueteParaReserva.tipoActividad),
      fechaInicio: fechaInicio,
      numeroPersonas: parseInt(reservaData.personas),
    });

    setShowReservaModal(false);
    setPaqueteParaReserva(null);

    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: `${paqueteParaReserva.nombre} agregado al carrito`,
      showConfirmButton: false,
      timer: 2000,
    });
  };

  // Calcular precio total
  const calcularPrecioTotal = () => {
    if (paqueteParaReserva && reservaData.personas > 0) {
      const precio = paqueteParaReserva.precioActual || paqueteParaReserva.precio || 0;
      return precio * parseInt(reservaData.personas);
    }
    return 0;
  };

  // Obtener fecha mínima (hoy)
  const getFechaMinima = () => {
    return new Date().toISOString().split('T')[0];
  };

  // Obtener precio para mostrar (maneja diferentes nombres de campo)
  const getPrecio = (paquete) => {
    return paquete.precioActual || paquete.precio || 0;
  };

  const getPrecioNormal = (paquete) => {
    return paquete.precioNormal || paquete.precioOriginal || getPrecio(paquete);
  };

  const tieneDescuento = (paquete) => {
    const actual = getPrecio(paquete);
    const normal = getPrecioNormal(paquete);
    return actual < normal && normal > 0;
  };

  return (
    <div className="container py-4">
      <PageHeader
        title="Paquetes Turísticos"
        subtitle="3 agencias integradas - Aventura, Cultural, Gastronómico y más"
        icon="bi-backpack2"
        gradient="light"
      />

      {/* Formulario de búsqueda mejorado */}
      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <form onSubmit={handleSearch}>
            <div className="row g-3">
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-geo-alt me-1"></i>Ciudad
                </label>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Ej: Cuenca"
                  value={filters.ciudad}
                  onChange={(e) => setFilters({ ...filters, ciudad: e.target.value })}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-calendar-event me-1"></i>Fecha inicio
                </label>
                <input
                  type="date"
                  className="form-control"
                  min={getFechaMinima()}
                  value={filters.fechaInicio}
                  onChange={(e) => setFilters({ ...filters, fechaInicio: e.target.value })}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-people me-1"></i>Personas
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="1"
                  max="20"
                  value={filters.personas}
                  onChange={(e) => {
                    const val = parseInt(e.target.value) || 1;
                    setFilters({ ...filters, personas: Math.max(1, Math.min(20, val)) });
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-tag me-1"></i>Tipo
                </label>
                <select
                  className="form-select"
                  value={filters.tipoActividad}
                  onChange={(e) => setFilters({ ...filters, tipoActividad: e.target.value })}
                >
                  <option value="">Todos</option>
                  <option value="Aventura">⛰️ Aventura</option>
                  <option value="Cultural">🏛️ Cultural</option>
                  <option value="Gastronomico">🍽️ Gastronómico</option>
                  <option value="Relax">🧘 Relax</option>
                  <option value="Familiar">👨‍👩‍👧‍👦 Familiar</option>
                </select>
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-currency-dollar me-1"></i>Precio máx.
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="0"
                  step="50"
                  placeholder="Sin límite"
                  value={filters.precioMax}
                  onChange={(e) => setFilters({ ...filters, precioMax: e.target.value })}
                />
              </div>
              <div className="col-md-2 d-flex align-items-end">
                <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                  {loading ? <span className="spinner-border spinner-border-sm"></span> : '🔍 Buscar'}
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
            <button className="btn btn-sm btn-outline-warning ms-3" onClick={() => buscarPaquetes()}>
              Reintentar
            </button>
          </div>
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="text-center py-5">
          <div className="spinner-border text-success" role="status">
            <span className="visually-hidden">Cargando...</span>
          </div>
          <p className="mt-3 text-muted">Conectando con agencias de turismo...</p>
        </div>
      )}

      {/* Resultados */}
      {!loading && (
        <>
          <p className="text-muted mb-3">{paquetes.length} paquetes encontrados</p>

          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            {paquetes.map((paquete, index) => {
              const imagenFinal = paquete.imagenUrl || getFallbackImage(paquete.tipoActividad);
              const precio = getPrecio(paquete);
              const precioNormal = getPrecioNormal(paquete);
              const descuento = tieneDescuento(paquete);
              
              return (
                <div key={`${paquete.servicioId}-${paquete.idPaquete}-${index}`} className="col">
                  <div className="card h-100 shadow-sm service-card">
                    <div className="position-relative">
                      <img
                        src={imagenFinal}
                        className="card-img-top"
                        alt={paquete.nombre}
                        style={{ height: '200px', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = getFallbackImage(paquete.tipoActividad); }}
                      />
                      <span className="position-absolute bottom-0 start-0 m-2 badge bg-success fs-6">
                        {getActivityEmoji(paquete.tipoActividad)} {paquete.tipoActividad}
                      </span>
                      {descuento && (
                        <span className="position-absolute top-0 end-0 m-2 badge bg-danger">
                          -{Math.round((1 - precio / precioNormal) * 100)}%
                        </span>
                      )}
                    </div>

                    <div className="card-body">
                      <h5 className="card-title fw-bold mb-1">{paquete.nombre}</h5>
                      <p className="text-muted small mb-2">
                        <i className="bi bi-geo-alt"></i> {paquete.ciudad}, {paquete.pais}
                      </p>
                      <p className="text-muted small mb-2">
                        <i className="bi bi-shop"></i> {paquete.nombreProveedor}
                      </p>

                      <div className="d-flex flex-wrap gap-1 mb-3">
                        <span className="badge bg-light text-dark border">
                          <i className="bi bi-calendar3"></i> {paquete.duracion || paquete.duracionDias} días
                        </span>
                        <span className="badge bg-light text-dark border">
                          <i className="bi bi-people"></i> máx {paquete.capacidad}
                        </span>
                      </div>

                      <div className="d-flex justify-content-between align-items-end">
                        <div>
                          {descuento && (
                            <div className="text-muted text-decoration-line-through small">
                              ${precioNormal.toFixed(2)}
                            </div>
                          )}
                          <div className="fw-bold text-success fs-4">
                            ${precio.toFixed(2)}
                          </div>
                          <small className="text-muted">por persona</small>
                        </div>

                        <button
                          className="btn btn-success"
                          onClick={() => handleOpenReservaModal(paquete)}
                        >
                          Reservar →
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </>
      )}

      {/* Modal de Reserva de Paquete */}
      {showReservaModal && paqueteParaReserva && (
        <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-success text-white">
                <h5 className="modal-title">
                  <i className="bi bi-backpack2 me-2"></i>
                  Reservar Paquete Turístico
                </h5>
                <button 
                  type="button" 
                  className="btn-close btn-close-white" 
                  onClick={() => setShowReservaModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                {/* Info del paquete */}
                <div className="alert alert-light border mb-4">
                  <div className="d-flex align-items-center gap-3">
                    <div className="flex-shrink-0">
                      <img 
                        src={paqueteParaReserva.imagenUrl || getFallbackImage(paqueteParaReserva.tipoActividad)}
                        alt={paqueteParaReserva.nombre}
                        className="rounded"
                        style={{ width: '80px', height: '60px', objectFit: 'cover' }}
                      />
                    </div>
                    <div>
                      <h6 className="fw-bold mb-1">
                        {getActivityEmoji(paqueteParaReserva.tipoActividad)} {paqueteParaReserva.nombre}
                      </h6>
                      <p className="mb-0 text-muted small">
                        <i className="bi bi-geo-alt me-1"></i>
                        {paqueteParaReserva.ciudad}, {paqueteParaReserva.pais}
                      </p>
                      <div className="d-flex gap-2 mt-1">
                        <span className="badge bg-info">{paqueteParaReserva.duracion || paqueteParaReserva.duracionDias} días</span>
                        <span className="badge bg-secondary">máx {paqueteParaReserva.capacidad} pers.</span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Formulario de reserva */}
                <div className="mb-3">
                  <label className="form-label fw-bold">
                    <i className="bi bi-calendar-event me-1 text-success"></i>
                    Fecha de Inicio <span className="text-danger">*</span>
                  </label>
                  <input
                    type="date"
                    className={`form-control ${reservaErrors.fechaInicio ? 'is-invalid' : ''}`}
                    min={getFechaMinima()}
                    value={reservaData.fechaInicio}
                    onChange={(e) => setReservaData({ ...reservaData, fechaInicio: e.target.value })}
                  />
                  {reservaErrors.fechaInicio && (
                    <div className="invalid-feedback">{reservaErrors.fechaInicio}</div>
                  )}
                </div>

                <div className="mb-3">
                  <label className="form-label fw-bold">
                    <i className="bi bi-people me-1 text-success"></i>
                    Número de Personas <span className="text-danger">*</span>
                  </label>
                  <input
                    type="number"
                    className={`form-control ${reservaErrors.personas ? 'is-invalid' : ''}`}
                    min="1"
                    max={paqueteParaReserva.capacidad}
                    value={reservaData.personas}
                    onChange={(e) => {
                      const val = parseInt(e.target.value) || 1;
                      setReservaData({ ...reservaData, personas: Math.max(1, val) });
                    }}
                  />
                  {reservaErrors.personas && (
                    <div className="invalid-feedback">{reservaErrors.personas}</div>
                  )}
                  <small className="text-muted">
                    Capacidad máxima: {paqueteParaReserva.capacidad} personas
                  </small>
                </div>

                {/* Resumen de precio */}
                {reservaData.personas > 0 && (
                  <div className="bg-light rounded-3 p-3 mt-4">
                    <h6 className="fw-bold mb-3">
                      <i className="bi bi-receipt me-2"></i>Resumen de Precio
                    </h6>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Precio por persona:</span>
                      <span>${getPrecio(paqueteParaReserva).toFixed(2)}</span>
                    </div>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Personas:</span>
                      <span>x {reservaData.personas}</span>
                    </div>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Duración:</span>
                      <span>{paqueteParaReserva.duracion || paqueteParaReserva.duracionDias} días</span>
                    </div>
                    <hr />
                    <div className="d-flex justify-content-between">
                      <span className="fw-bold">Total Estimado:</span>
                      <span className="fw-bold text-success fs-5">
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
                  className="btn btn-success fw-bold"
                  onClick={handleConfirmReserva}
                  disabled={verificandoDisponibilidad}
                >
                  {verificandoDisponibilidad ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Verificando disponibilidad...
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

export default Paquetes;
