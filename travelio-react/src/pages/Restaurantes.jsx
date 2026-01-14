import { useState, useEffect } from 'react';
import PageHeader from '../components/common/PageHeader';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Restaurantes = () => {
  const addItem = useCartStore((state) => state.addItem);
  const [loading, setLoading] = useState(false);
  const [mesas, setMesas] = useState([]);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    fecha: '',
    hora: '19:00',
    numeroPersonas: 2,
    capacidad: '',
    tipoMesa: '',
  });

  // Modal de reserva con datos obligatorios
  const [showReservaModal, setShowReservaModal] = useState(false);
  const [mesaParaReserva, setMesaParaReserva] = useState(null);
  const [reservaData, setReservaData] = useState({
    fecha: '',
    hora: '19:00',
    numeroPersonas: 2,
  });
  const [reservaErrors, setReservaErrors] = useState({});
  const [verificandoDisponibilidad, setVerificandoDisponibilidad] = useState(false);

  // Cargar mesas al montar el componente
  useEffect(() => {
    buscarMesas();
  }, []);

  const buscarMesas = async (filtros = {}) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.capacidad) params.append('capacidad', filtros.capacidad);
      if (filtros.tipoMesa) params.append('tipoMesa', filtros.tipoMesa);
      if (filtros.fecha) params.append('fecha', filtros.fecha);
      if (filtros.numeroPersonas) params.append('numeroPersonas', filtros.numeroPersonas);

      const url = `${ENDPOINTS.RESTAURANTES}${params.toString() ? '?' + params.toString() : ''}`;
      console.log('Fetching mesas from:', url);
      
      const response = await api.get(url);
      console.log('API Response:', response.data);
      
      if (response.data.success) {
        setMesas(response.data.data || []);
      } else {
        setError(response.data.message || 'Error al cargar mesas');
      }
    } catch (err) {
      console.error('Error fetching mesas:', err);
      setError(err.response?.data?.message || 'Error al conectar con el servidor');
    } finally {
      setLoading(false);
    }
  };

  const getRestaurantEmoji = (nombre) => {
    const n = nombre?.toLowerCase() || '';
    if (n.includes('cangrejito') || n.includes('7 mares') || n.includes('marisco')) return '🦀';
    if (n.includes('dragon') || n.includes('asiatico') || n.includes('rojo')) return '🥢';
    if (n.includes('sanctum') || n.includes('gourmet')) return '🍷';
    if (n.includes('andino') || n.includes('tipico')) return '🌿';
    if (n.includes('bar') || n.includes('sinson')) return '🍺';
    return '🍽️';
  };

  const getFallbackImage = (tipoMesa) => {
    const tipo = tipoMesa?.toLowerCase() || '';
    if (tipo.includes('vip')) return 'https://images.unsplash.com/photo-1552566626-52f8b828add9?w=400&h=250&fit=crop';
    if (tipo.includes('exterior') || tipo.includes('terraza')) return 'https://images.unsplash.com/photo-1514933651103-005eec06c04b?w=400&h=250&fit=crop';
    if (tipo.includes('privado')) return 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=400&h=250&fit=crop';
    return 'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=400&h=250&fit=crop';
  };

  const handleSearch = (e) => {
    e.preventDefault();
    buscarMesas(filters);
  };

  // Abrir modal de reserva con datos obligatorios
  const handleOpenReservaModal = (mesa) => {
    setMesaParaReserva(mesa);
    setReservaData({
      fecha: filters.fecha || '',
      hora: filters.hora || '19:00',
      numeroPersonas: filters.numeroPersonas || 2,
    });
    setReservaErrors({});
    setShowReservaModal(true);
  };

  // Validar y agregar al carrito
  const handleConfirmReserva = async () => {
    const errors = {};
    
    if (!reservaData.fecha) {
      errors.fecha = 'La fecha de reserva es obligatoria';
    } else {
      const fecha = new Date(reservaData.fecha);
      const hoy = new Date();
      hoy.setHours(0, 0, 0, 0);
      
      if (fecha < hoy) {
        errors.fecha = 'La fecha no puede ser en el pasado';
      }
    }
    
    if (!reservaData.hora) {
      errors.hora = 'La hora de reserva es obligatoria';
    }
    
    if (!reservaData.numeroPersonas || reservaData.numeroPersonas < 1) {
      errors.numeroPersonas = 'Debe indicar al menos 1 persona';
    }
    
    if (mesaParaReserva && reservaData.numeroPersonas > mesaParaReserva.capacidad) {
      errors.numeroPersonas = `La mesa tiene capacidad para ${mesaParaReserva.capacidad} personas`;
    }

    if (Object.keys(errors).length > 0) {
      setReservaErrors(errors);
      return;
    }

    // Verificar disponibilidad de la mesa antes de agregar
    setVerificandoDisponibilidad(true);
    try {
      const response = await api.post(`${ENDPOINTS.RESTAURANTES}/disponibilidad`, {
        servicioId: mesaParaReserva.servicioId,
        idMesa: mesaParaReserva.idMesa,
        fecha: reservaData.fecha,
        hora: reservaData.hora,
        numeroPersonas: parseInt(reservaData.numeroPersonas)
      });

      if (!response.data.disponible) {
        setReservaErrors({ general: 'La mesa no está disponible para la fecha y hora seleccionadas' });
        Swal.fire({
          icon: 'warning',
          title: 'No Disponible',
          text: 'La mesa no está disponible para la fecha y hora seleccionadas. Por favor, intenta con otra hora o fecha.',
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
    const fechaReserva = new Date(reservaData.fecha);
    addItem({
      tipo: 'RESTAURANT',
      titulo: `Mesa #${mesaParaReserva.idMesa} - ${mesaParaReserva.nombreProveedor}`,
      detalle: `${mesaParaReserva.tipoMesa} - ${fechaReserva.toLocaleDateString()} ${reservaData.hora} - ${reservaData.numeroPersonas} persona(s)`,
      precioOriginal: mesaParaReserva.precio,
      precioFinal: mesaParaReserva.precio,
      unidadPrecio: '/reserva',
      servicioId: mesaParaReserva.servicioId,
      idProducto: mesaParaReserva.idMesa?.toString(),
      imagenUrl: mesaParaReserva.imagenURL || getFallbackImage(mesaParaReserva.tipoMesa),
      fechaInicio: fechaReserva,
      horaReserva: reservaData.hora,
      numeroPersonas: parseInt(reservaData.numeroPersonas),
    });

    setShowReservaModal(false);
    setMesaParaReserva(null);

    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: `Mesa reservada en ${mesaParaReserva.nombreProveedor}`,
      showConfirmButton: false,
      timer: 2000,
    });
  };

  // Obtener fecha mínima (hoy)
  const getFechaMinima = () => {
    return new Date().toISOString().split('T')[0];
  };

  // Generar opciones de hora
  const generarOpcionesHora = () => {
    const horas = [];
    for (let h = 11; h <= 22; h++) {
      horas.push(`${h.toString().padStart(2, '0')}:00`);
      horas.push(`${h.toString().padStart(2, '0')}:30`);
    }
    return horas;
  };

  return (
    <div className="container py-4">
      <PageHeader
        title="Restaurantes"
        subtitle="7 restaurantes integrados - Mariscos, Asiático, Gourmet y más"
        icon="bi-cup-straw"
        gradient="warning"
      />

      {/* Formulario de búsqueda mejorado */}
      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <form onSubmit={handleSearch}>
            <div className="row g-3">
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-calendar-event me-1"></i>Fecha
                </label>
                <input
                  type="date"
                  className="form-control"
                  min={getFechaMinima()}
                  value={filters.fecha}
                  onChange={(e) => setFilters({ ...filters, fecha: e.target.value })}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-clock me-1"></i>Hora
                </label>
                <select
                  className="form-select"
                  value={filters.hora}
                  onChange={(e) => setFilters({ ...filters, hora: e.target.value })}
                >
                  {generarOpcionesHora().map((hora) => (
                    <option key={hora} value={hora}>{hora}</option>
                  ))}
                </select>
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
                  value={filters.numeroPersonas}
                  onChange={(e) => {
                    const val = parseInt(e.target.value) || 1;
                    setFilters({ ...filters, numeroPersonas: Math.max(1, Math.min(20, val)) });
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-diagram-3 me-1"></i>Capacidad mín.
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="1"
                  max="20"
                  placeholder="Cualquiera"
                  value={filters.capacidad}
                  onChange={(e) => {
                    const val = e.target.value ? parseInt(e.target.value) : '';
                    setFilters({ ...filters, capacidad: val });
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label">
                  <i className="bi bi-grid me-1"></i>Tipo de Mesa
                </label>
                <select
                  className="form-select"
                  value={filters.tipoMesa}
                  onChange={(e) => setFilters({ ...filters, tipoMesa: e.target.value })}
                >
                  <option value="">Todas</option>
                  <option value="Interior">Interior</option>
                  <option value="Exterior">Exterior/Terraza</option>
                  <option value="VIP">VIP</option>
                  <option value="Privado">Privado</option>
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
            <button className="btn btn-sm btn-outline-warning ms-3" onClick={() => buscarMesas()}>
              Reintentar
            </button>
          </div>
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="text-center py-5">
          <div className="spinner-border text-danger" role="status">
            <span className="visually-hidden">Cargando...</span>
          </div>
          <p className="mt-3 text-muted">Conectando con restaurantes...</p>
        </div>
      )}

      {/* Resultados */}
      {!loading && (
        <>
          <p className="text-muted mb-3">{mesas.length} mesas encontradas</p>

          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            {mesas.map((mesa, index) => {
              const imagenFinal = mesa.imagenURL || getFallbackImage(mesa.tipoMesa);
              
              return (
                <div key={`${mesa.servicioId}-${mesa.idMesa}-${index}`} className="col">
                  <div className="card h-100 shadow-sm service-card">
                    <div className="position-relative">
                      <img
                        src={imagenFinal}
                        className="card-img-top"
                        alt={`Mesa ${mesa.idMesa}`}
                        style={{ height: '200px', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = getFallbackImage(mesa.tipoMesa); }}
                      />
                      <span className="position-absolute bottom-0 start-0 m-2 badge bg-danger fs-6">
                        {getRestaurantEmoji(mesa.nombreProveedor)} {mesa.nombreProveedor}
                      </span>
                      <span className={`position-absolute top-0 end-0 m-2 badge bg-${mesa.estado === 'Disponible' ? 'success' : 'warning'}`}>
                        {mesa.estado}
                      </span>
                    </div>

                    <div className="card-body">
                      <h5 className="card-title fw-bold">Mesa #{mesa.idMesa}</h5>
                      <div className="d-flex gap-2 mb-3">
                        <span className="badge bg-info text-white">{mesa.tipoMesa}</span>
                        <span className="badge bg-light text-dark border">👥 {mesa.capacidad} personas</span>
                      </div>

                      <div className="d-flex justify-content-between align-items-end mt-3">
                        <div>
                          <div className="fw-bold text-danger fs-4">${mesa.precio?.toFixed(2)}</div>
                          <small className="text-muted">por reserva</small>
                        </div>
                        <button
                          className="btn btn-danger"
                          onClick={() => handleOpenReservaModal(mesa)}
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

      {/* Modal de Reserva de Mesa */}
      {showReservaModal && mesaParaReserva && (
        <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }} tabIndex="-1">
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-danger text-white">
                <h5 className="modal-title">
                  <i className="bi bi-calendar-check me-2"></i>
                  Reservar Mesa
                </h5>
                <button 
                  type="button" 
                  className="btn-close btn-close-white" 
                  onClick={() => setShowReservaModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                {/* Info de la mesa */}
                <div className="alert alert-light border mb-4">
                  <div className="d-flex align-items-center gap-3">
                    <div className="flex-shrink-0">
                      <img 
                        src={mesaParaReserva.imagenURL || getFallbackImage(mesaParaReserva.tipoMesa)}
                        alt="Mesa"
                        className="rounded"
                        style={{ width: '80px', height: '60px', objectFit: 'cover' }}
                      />
                    </div>
                    <div>
                      <h6 className="fw-bold mb-1">
                        {getRestaurantEmoji(mesaParaReserva.nombreProveedor)} {mesaParaReserva.nombreProveedor}
                      </h6>
                      <p className="mb-0 text-muted">
                        Mesa #{mesaParaReserva.idMesa} - {mesaParaReserva.tipoMesa}
                      </p>
                      <span className="badge bg-secondary">Capacidad: {mesaParaReserva.capacidad} personas</span>
                    </div>
                  </div>
                </div>

                {/* Formulario de reserva */}
                <div className="row g-3">
                  <div className="col-md-6">
                    <label className="form-label fw-bold">
                      <i className="bi bi-calendar-event me-1 text-danger"></i>
                      Fecha <span className="text-danger">*</span>
                    </label>
                    <input
                      type="date"
                      className={`form-control ${reservaErrors.fecha ? 'is-invalid' : ''}`}
                      min={getFechaMinima()}
                      value={reservaData.fecha}
                      onChange={(e) => setReservaData({ ...reservaData, fecha: e.target.value })}
                    />
                    {reservaErrors.fecha && (
                      <div className="invalid-feedback">{reservaErrors.fecha}</div>
                    )}
                  </div>

                  <div className="col-md-6">
                    <label className="form-label fw-bold">
                      <i className="bi bi-clock me-1 text-danger"></i>
                      Hora <span className="text-danger">*</span>
                    </label>
                    <select
                      className={`form-select ${reservaErrors.hora ? 'is-invalid' : ''}`}
                      value={reservaData.hora}
                      onChange={(e) => setReservaData({ ...reservaData, hora: e.target.value })}
                    >
                      {generarOpcionesHora().map((hora) => (
                        <option key={hora} value={hora}>{hora}</option>
                      ))}
                    </select>
                    {reservaErrors.hora && (
                      <div className="invalid-feedback">{reservaErrors.hora}</div>
                    )}
                  </div>
                </div>

                <div className="mt-3">
                  <label className="form-label fw-bold">
                    <i className="bi bi-people me-1 text-danger"></i>
                    Número de Personas <span className="text-danger">*</span>
                  </label>
                  <input
                    type="number"
                    className={`form-control ${reservaErrors.numeroPersonas ? 'is-invalid' : ''}`}
                    min="1"
                    max={mesaParaReserva.capacidad}
                    value={reservaData.numeroPersonas}
                    onChange={(e) => {
                      const val = parseInt(e.target.value) || 1;
                      setReservaData({ ...reservaData, numeroPersonas: Math.max(1, val) });
                    }}
                  />
                  {reservaErrors.numeroPersonas && (
                    <div className="invalid-feedback">{reservaErrors.numeroPersonas}</div>
                  )}
                  <small className="text-muted">
                    Máximo {mesaParaReserva.capacidad} personas en esta mesa
                  </small>
                </div>

                {/* Resumen de precio */}
                <div className="bg-light rounded-3 p-3 mt-4">
                  <h6 className="fw-bold mb-3">
                    <i className="bi bi-receipt me-2"></i>Resumen de Reserva
                  </h6>
                  {reservaData.fecha && (
                    <div className="d-flex justify-content-between mb-2">
                      <span>Fecha:</span>
                      <span>{new Date(reservaData.fecha).toLocaleDateString()}</span>
                    </div>
                  )}
                  <div className="d-flex justify-content-between mb-2">
                    <span>Hora:</span>
                    <span>{reservaData.hora}</span>
                  </div>
                  <div className="d-flex justify-content-between mb-2">
                    <span>Personas:</span>
                    <span>{reservaData.numeroPersonas}</span>
                  </div>
                  <hr />
                  <div className="d-flex justify-content-between">
                    <span className="fw-bold">Total:</span>
                    <span className="fw-bold text-danger fs-5">
                      ${mesaParaReserva.precio?.toFixed(2)}
                    </span>
                  </div>
                </div>

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
                  className="btn btn-danger fw-bold"
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

export default Restaurantes;
