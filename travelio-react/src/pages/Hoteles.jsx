import { useState, useEffect } from 'react';
import PageHeader from '../components/common/PageHeader';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Hoteles = () => {
  const addItem = useCartStore((state) => state.addItem);
  const [loading, setLoading] = useState(false);
  const [habitaciones, setHabitaciones] = useState([]);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    ciudad: '',
    fechaInicio: '',
    fechaFin: '',
    numeroHuespedes: 2,
    tipoHabitacion: '',
  });

  // Modal de reserva
  const [showReservaModal, setShowReservaModal] = useState(false);
  const [selectedHab, setSelectedHab] = useState(null);
  const [reservaData, setReservaData] = useState({
    fechaInicio: '',
    fechaFin: '',
    numeroHuespedes: 2,
  });
  const [reservaErrors, setReservaErrors] = useState({});

  // Cargar habitaciones al montar
  useEffect(() => {
    buscarHabitaciones();
  }, []);

  const buscarHabitaciones = async (filtros = {}) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.ciudad) params.append('ciudad', filtros.ciudad);
      if (filtros.tipoHabitacion) params.append('tipoHabitacion', filtros.tipoHabitacion);
      if (filtros.fechaInicio) params.append('fechaInicio', filtros.fechaInicio);
      if (filtros.fechaFin) params.append('fechaFin', filtros.fechaFin);
      if (filtros.numeroHuespedes) params.append('numeroHuespedes', filtros.numeroHuespedes);

      const url = `${ENDPOINTS.HOTELES}${params.toString() ? '?' + params.toString() : ''}`;
      console.log('Fetching habitaciones from:', url);
      
      const response = await api.get(url);
      console.log('API Response:', response.data);
      
      if (response.data.success) {
        setHabitaciones(response.data.data || []);
      } else {
        setError(response.data.message || 'Error al cargar habitaciones');
      }
    } catch (err) {
      console.error('Error fetching habitaciones:', err);
      setError(err.response?.data?.message || 'Error al conectar con el servidor');
    } finally {
      setLoading(false);
    }
  };

  const getFallbackImage = (tipo, hotel) => {
    const tipoLower = (tipo?.toLowerCase() || '') + ' ' + (hotel?.toLowerCase() || '');
    if (tipoLower.includes('suite') || tipoLower.includes('premium'))
      return 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=400&h=250&fit=crop';
    if (tipoLower.includes('doble') || tipoLower.includes('matrimonial'))
      return 'https://images.unsplash.com/photo-1590490360182-c33d57733427?w=400&h=250&fit=crop';
    if (tipoLower.includes('familiar'))
      return 'https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=400&h=250&fit=crop';
    return 'https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=400&h=250&fit=crop';
  };

  const handleSearch = (e) => {
    e.preventDefault();
    buscarHabitaciones(filters);
  };

  // Abrir modal de reserva
  const handleOpenReservaModal = (hab) => {
    setSelectedHab(hab);
    // Pre-cargar fechas de los filtros si existen
    setReservaData({
      fechaInicio: filters.fechaInicio || '',
      fechaFin: filters.fechaFin || '',
      numeroHuespedes: filters.numeroHuespedes || 2,
    });
    setReservaErrors({});
    setShowReservaModal(true);
  };

  // Estado para verificar disponibilidad
  const [verificandoDisponibilidad, setVerificandoDisponibilidad] = useState(false);

  // Validar y agregar al carrito
  const handleConfirmReserva = async () => {
    const errors = {};
    
    if (!reservaData.fechaInicio) {
      errors.fechaInicio = 'La fecha de entrada es obligatoria';
    }
    if (!reservaData.fechaFin) {
      errors.fechaFin = 'La fecha de salida es obligatoria';
    }
    if (reservaData.fechaInicio && reservaData.fechaFin) {
      const inicio = new Date(reservaData.fechaInicio);
      const fin = new Date(reservaData.fechaFin);
      const hoy = new Date();
      hoy.setHours(0, 0, 0, 0);
      
      if (inicio < hoy) {
        errors.fechaInicio = 'La fecha de entrada no puede ser en el pasado';
      }
      if (fin <= inicio) {
        errors.fechaFin = 'La fecha de salida debe ser posterior a la entrada';
      }
    }
    if (!reservaData.numeroHuespedes || reservaData.numeroHuespedes < 1) {
      errors.numeroHuespedes = 'Debe haber al menos 1 huésped';
    }

    if (Object.keys(errors).length > 0) {
      setReservaErrors(errors);
      return;
    }

    // Verificar disponibilidad antes de agregar
    setVerificandoDisponibilidad(true);
    try {
      const response = await api.post(`${ENDPOINTS.HOTELES}/disponibilidad`, {
        servicioId: selectedHab.servicioId,
        idHabitacion: selectedHab.idHabitacion,
        fechaInicio: reservaData.fechaInicio,
        fechaFin: reservaData.fechaFin
      });

      if (!response.data.disponible) {
        setReservaErrors({ general: 'La habitación no está disponible para las fechas seleccionadas' });
        Swal.fire({
          icon: 'warning',
          title: 'No Disponible',
          text: 'La habitación no está disponible para las fechas seleccionadas. Por favor, intenta con otras fechas.',
        });
        setVerificandoDisponibilidad(false);
        return;
      }
    } catch (err) {
      console.warn('No se pudo verificar disponibilidad:', err);
      // Continuar si falla la verificación (el backend verificará al procesar)
    } finally {
      setVerificandoDisponibilidad(false);
    }

    // Agregar al carrito con las fechas validadas
    addItem({
      tipo: 'HOTEL',
      titulo: selectedHab.nombreHabitacion,
      detalle: `${selectedHab.hotel} - ${selectedHab.ciudad}`,
      precioOriginal: selectedHab.precioNormal,
      precioFinal: selectedHab.precioActual,
      unidadPrecio: '/noche',
      servicioId: selectedHab.servicioId,
      idProducto: selectedHab.idHabitacion,
      imagenUrl: selectedHab.imagenes?.[0] || getFallbackImage(selectedHab.tipoHabitacion, selectedHab.hotel),
      fechaInicio: new Date(reservaData.fechaInicio),
      fechaFin: new Date(reservaData.fechaFin),
      numeroPersonas: reservaData.numeroHuespedes,
    });

    setShowReservaModal(false);
    setSelectedHab(null);

    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: `${selectedHab.nombreHabitacion} agregado al carrito`,
      showConfirmButton: false,
      timer: 2000,
    });
  };

  // Calcular noches
  const calcularNoches = () => {
    if (reservaData.fechaInicio && reservaData.fechaFin) {
      const inicio = new Date(reservaData.fechaInicio);
      const fin = new Date(reservaData.fechaFin);
      const diff = Math.ceil((fin - inicio) / (1000 * 60 * 60 * 24));
      return diff > 0 ? diff : 0;
    }
    return 0;
  };

  return (
    <div className="container py-4">
      <PageHeader
        title="Hoteles y Habitaciones"
        subtitle="6 hoteles integrados - Suites, Dobles, Familiares y más"
        icon="bi-building"
        gradient="success"
      />

      {/* Formulario de búsqueda */}
      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <form onSubmit={handleSearch}>
            <div className="row g-3">
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-geo-alt me-1 text-primary"></i>Ciudad
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
                <label className="form-label fw-bold">
                  <i className="bi bi-calendar-event me-1 text-success"></i>Check-in
                </label>
                <input
                  type="date"
                  className="form-control"
                  min={new Date().toISOString().split('T')[0]}
                  value={filters.fechaInicio}
                  onChange={(e) => {
                    const newFechaInicio = e.target.value;
                    setFilters(prev => ({
                      ...prev, 
                      fechaInicio: newFechaInicio,
                      // Si check-out es menor que check-in, limpiar check-out
                      fechaFin: prev.fechaFin && prev.fechaFin <= newFechaInicio ? '' : prev.fechaFin
                    }));
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-calendar-event me-1 text-danger"></i>Check-out
                </label>
                <input
                  type="date"
                  className="form-control"
                  min={filters.fechaInicio || new Date().toISOString().split('T')[0]}
                  value={filters.fechaFin}
                  onChange={(e) => setFilters({ ...filters, fechaFin: e.target.value })}
                  disabled={!filters.fechaInicio}
                />
                {!filters.fechaInicio && (
                  <small className="text-muted">Selecciona check-in primero</small>
                )}
              </div>
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-people me-1 text-info"></i>Huéspedes
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="1"
                  max="10"
                  value={filters.numeroHuespedes}
                  onChange={(e) => {
                    const val = parseInt(e.target.value) || 1;
                    setFilters({ ...filters, numeroHuespedes: Math.max(1, Math.min(10, val)) });
                  }}
                  onBlur={(e) => {
                    // Asegurar que el valor esté en rango al perder foco
                    const val = parseInt(e.target.value) || 1;
                    if (val < 1 || val > 10) {
                      setFilters({ ...filters, numeroHuespedes: Math.max(1, Math.min(10, val)) });
                    }
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-door-open me-1 text-warning"></i>Tipo
                </label>
                <select
                  className="form-select"
                  value={filters.tipoHabitacion}
                  onChange={(e) => setFilters({ ...filters, tipoHabitacion: e.target.value })}
                >
                  <option value="">Todos</option>
                  <option value="Individual">Individual</option>
                  <option value="Doble">Doble</option>
                  <option value="Suite">Suite</option>
                  <option value="Familiar">Familiar</option>
                </select>
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
            <button className="btn btn-sm btn-outline-warning ms-3" onClick={() => buscarHabitaciones()}>
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
          <p className="mt-3 text-muted">Conectando con hoteles...</p>
        </div>
      )}

      {/* Resultados */}
      {!loading && (
        <>
          <p className="text-muted mb-3">{habitaciones.length} habitaciones encontradas</p>

          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            {habitaciones.map((hab, index) => {
              const imagenFinal = hab.imagenes?.[0] || getFallbackImage(hab.tipoHabitacion, hab.hotel);
              const tieneDescuento = hab.precioActual < hab.precioNormal && hab.precioNormal > 0;
              
              return (
                <div key={`${hab.servicioId}-${hab.idHabitacion}-${index}`} className="col">
                  <div className="card h-100 shadow-sm service-card">
                    <div className="position-relative">
                      <img
                        src={imagenFinal}
                        className="card-img-top"
                        alt={hab.nombreHabitacion}
                        style={{ height: '200px', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = getFallbackImage(hab.tipoHabitacion, hab.hotel); }}
                      />
                      <span className="position-absolute top-0 end-0 m-2 badge bg-primary">
                        {hab.nombreProveedor}
                      </span>
                      <span className="position-absolute bottom-0 start-0 m-2 badge bg-success">
                        {hab.hotel}
                      </span>
                      {tieneDescuento && (
                        <span className="position-absolute top-0 start-0 m-2 badge bg-danger">
                          -{Math.round((1 - hab.precioActual / hab.precioNormal) * 100)}%
                        </span>
                      )}
                    </div>

                    <div className="card-body">
                      <h5 className="card-title fw-bold mb-1">{hab.nombreHabitacion}</h5>
                      <p className="text-muted small mb-2">
                        <i className="bi bi-geo-alt"></i> {hab.ciudad}, {hab.pais}
                      </p>

                      <div className="d-flex flex-wrap gap-1 mb-3">
                        <span className="badge bg-light text-dark border">
                          <i className="bi bi-people"></i> {hab.capacidad} personas
                        </span>
                        <span className="badge bg-info text-white">{hab.tipoHabitacion}</span>
                      </div>

                      {hab.amenidades && (
                        <p className="small text-muted mb-2">
                          ✨ {hab.amenidades.length > 50 ? hab.amenidades.substring(0, 50) + '...' : hab.amenidades}
                        </p>
                      )}

                      <div className="d-flex justify-content-between align-items-end">
                        <div>
                          {tieneDescuento && (
                            <div className="text-muted text-decoration-line-through small">
                              ${hab.precioNormal?.toFixed(2)}
                            </div>
                          )}
                          <div className="fw-bold text-success fs-4">
                            ${hab.precioActual?.toFixed(2)} 
                            <small className="text-muted fw-normal fs-6">/noche</small>
                          </div>
                        </div>

                        <button
                          className="btn btn-success"
                          onClick={() => handleOpenReservaModal(hab)}
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

      {/* Modal de Reserva */}
      {showReservaModal && selectedHab && (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-success text-white">
                <h5 className="modal-title">
                  <i className="bi bi-calendar-check me-2"></i>
                  Confirmar Reserva
                </h5>
                <button 
                  type="button" 
                  className="btn-close btn-close-white" 
                  onClick={() => setShowReservaModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                {/* Info de la habitación */}
                <div className="d-flex mb-4 p-3 bg-light rounded">
                  <img 
                    src={selectedHab.imagenes?.[0] || getFallbackImage(selectedHab.tipoHabitacion, selectedHab.hotel)}
                    alt={selectedHab.nombreHabitacion}
                    style={{ width: '100px', height: '80px', objectFit: 'cover', borderRadius: '8px' }}
                  />
                  <div className="ms-3">
                    <h6 className="mb-1">{selectedHab.nombreHabitacion}</h6>
                    <p className="text-muted small mb-1">
                      <i className="bi bi-building me-1"></i>{selectedHab.hotel}
                    </p>
                    <p className="text-muted small mb-0">
                      <i className="bi bi-geo-alt me-1"></i>{selectedHab.ciudad}
                    </p>
                  </div>
                </div>

                {/* Formulario de fechas */}
                <div className="row g-3">
                  <div className="col-md-6">
                    <label className="form-label fw-bold">
                      <i className="bi bi-calendar-event me-1 text-success"></i>
                      Fecha de Entrada *
                    </label>
                    <input
                      type="date"
                      className={`form-control ${reservaErrors.fechaInicio ? 'is-invalid' : ''}`}
                      min={new Date().toISOString().split('T')[0]}
                      value={reservaData.fechaInicio}
                      onChange={(e) => setReservaData({ ...reservaData, fechaInicio: e.target.value })}
                    />
                    {reservaErrors.fechaInicio && (
                      <div className="invalid-feedback">{reservaErrors.fechaInicio}</div>
                    )}
                  </div>
                  <div className="col-md-6">
                    <label className="form-label fw-bold">
                      <i className="bi bi-calendar-event me-1 text-danger"></i>
                      Fecha de Salida *
                    </label>
                    <input
                      type="date"
                      className={`form-control ${reservaErrors.fechaFin ? 'is-invalid' : ''}`}
                      min={reservaData.fechaInicio || new Date().toISOString().split('T')[0]}
                      value={reservaData.fechaFin}
                      onChange={(e) => setReservaData({ ...reservaData, fechaFin: e.target.value })}
                    />
                    {reservaErrors.fechaFin && (
                      <div className="invalid-feedback">{reservaErrors.fechaFin}</div>
                    )}
                  </div>
                  <div className="col-12">
                    <label className="form-label fw-bold">
                      <i className="bi bi-people me-1 text-primary"></i>
                      Número de Huéspedes *
                    </label>
                    <input
                      type="number"
                      className={`form-control ${reservaErrors.numeroHuespedes ? 'is-invalid' : ''}`}
                      min="1"
                      max={selectedHab.capacidad || 10}
                      value={reservaData.numeroHuespedes}
                      onChange={(e) => setReservaData({ ...reservaData, numeroHuespedes: parseInt(e.target.value) || 1 })}
                    />
                    {reservaErrors.numeroHuespedes && (
                      <div className="invalid-feedback">{reservaErrors.numeroHuespedes}</div>
                    )}
                    <small className="text-muted">Capacidad máxima: {selectedHab.capacidad || 'N/A'} personas</small>
                  </div>
                </div>

                {/* Resumen de precios */}
                {calcularNoches() > 0 && (
                  <div className="mt-4 p-3 bg-success bg-opacity-10 rounded border border-success">
                    <div className="d-flex justify-content-between mb-2">
                      <span>Precio por noche:</span>
                      <span className="fw-bold">${selectedHab.precioActual?.toFixed(2)}</span>
                    </div>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Número de noches:</span>
                      <span className="fw-bold">{calcularNoches()}</span>
                    </div>
                    <hr />
                    <div className="d-flex justify-content-between">
                      <span className="fw-bold fs-5">Total estimado:</span>
                      <span className="fw-bold fs-5 text-success">
                        ${(selectedHab.precioActual * calcularNoches()).toFixed(2)}
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
                  className="btn btn-success"
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
                      <i className="bi bi-cart-plus me-2"></i>
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

export default Hoteles;
