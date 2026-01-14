import { useState, useEffect } from 'react';
import PageHeader from '../components/common/PageHeader';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Autos = () => {
  const addItem = useCartStore((state) => state.addItem);
  const [loading, setLoading] = useState(false);
  const [autos, setAutos] = useState([]);
  const [error, setError] = useState(null);
  const [selectedAuto, setSelectedAuto] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [checkingAvailability, setCheckingAvailability] = useState(null);
  const [filters, setFilters] = useState({
    ciudad: '',
    fechaInicio: '',
    fechaFin: '',
    capacidad: '',
    precioMax: '',
  });

  // Modal de reserva con fechas obligatorias
  const [showReservaModal, setShowReservaModal] = useState(false);
  const [autoParaReserva, setAutoParaReserva] = useState(null);
  const [reservaData, setReservaData] = useState({
    fechaInicio: '',
    fechaFin: '',
  });
  const [reservaErrors, setReservaErrors] = useState({});

  // Cargar autos al montar el componente
  useEffect(() => {
    buscarAutos();
  }, []);

  const buscarAutos = async (filtros = {}) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filtros.ciudad) params.append('ciudad', filtros.ciudad);
      if (filtros.capacidad) params.append('capacidad', filtros.capacidad);
      if (filtros.precioMax) params.append('precioMax', filtros.precioMax);
      if (filtros.fechaInicio) params.append('fechaInicio', filtros.fechaInicio);
      if (filtros.fechaFin) params.append('fechaFin', filtros.fechaFin);

      const url = `${ENDPOINTS.AUTOS}${params.toString() ? '?' + params.toString() : ''}`;
      console.log('Fetching autos from:', url);
      
      const response = await api.get(url);
      console.log('API Response:', response.data);
      
      if (response.data.success) {
        setAutos(response.data.data || []);
      } else {
        setError(response.data.message || 'Error al cargar vehículos');
      }
    } catch (err) {
      console.error('Error fetching autos:', err);
      setError(err.response?.data?.message || 'Error al conectar con el servidor');
    } finally {
      setLoading(false);
    }
  };

  const getFallbackImage = (tipo) => {
    const tipoLower = tipo?.toLowerCase() || '';
    if (tipoLower.includes('suv') || tipoLower.includes('sportage'))
      return 'https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=400&h=250&fit=crop';
    if (tipoLower.includes('sedan') || tipoLower.includes('corolla'))
      return 'https://images.unsplash.com/photo-1550355291-bbee04a92027?w=400&h=250&fit=crop';
    if (tipoLower.includes('pickup') || tipoLower.includes('hilux'))
      return 'https://images.unsplash.com/photo-1559416523-140ddc3d238c?w=400&h=250&fit=crop';
    if (tipoLower.includes('yaris') || tipoLower.includes('economico'))
      return 'https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=400&h=250&fit=crop';
    return 'https://images.unsplash.com/photo-1502877338535-766e1452684a?w=400&h=250&fit=crop';
  };

  const handleSearch = (e) => {
    e.preventDefault();
    buscarAutos(filters);
  };

  // Abrir modal de reserva con fechas obligatorias
  const handleOpenReservaModal = (auto) => {
    setAutoParaReserva(auto);
    setReservaData({
      fechaInicio: filters.fechaInicio || '',
      fechaFin: filters.fechaFin || '',
    });
    setReservaErrors({});
    setShowReservaModal(true);
  };

  // Validar y agregar al carrito
  const handleConfirmReserva = () => {
    const errors = {};
    
    if (!reservaData.fechaInicio) {
      errors.fechaInicio = 'La fecha de recogida es obligatoria';
    }
    if (!reservaData.fechaFin) {
      errors.fechaFin = 'La fecha de entrega es obligatoria';
    }
    if (reservaData.fechaInicio && reservaData.fechaFin) {
      const inicio = new Date(reservaData.fechaInicio);
      const fin = new Date(reservaData.fechaFin);
      const hoy = new Date();
      hoy.setHours(0, 0, 0, 0);
      
      if (inicio < hoy) {
        errors.fechaInicio = 'La fecha de recogida no puede ser en el pasado';
      }
      if (fin <= inicio) {
        errors.fechaFin = 'La fecha de entrega debe ser posterior a la recogida';
      }
    }

    if (Object.keys(errors).length > 0) {
      setReservaErrors(errors);
      return;
    }

    // Agregar al carrito con fechas validadas
    addItem({
      tipo: 'CAR',
      titulo: autoParaReserva.tipo,
      detalle: `${autoParaReserva.nombreProveedor} - ${autoParaReserva.ciudad}`,
      precioOriginal: autoParaReserva.precioNormalPorDia,
      precioFinal: autoParaReserva.precioActualPorDia,
      unidadPrecio: '/día',
      servicioId: autoParaReserva.servicioId,
      idProducto: autoParaReserva.idAuto,
      imagenUrl: autoParaReserva.uriImagen || getFallbackImage(autoParaReserva.tipo),
      fechaInicio: new Date(reservaData.fechaInicio),
      fechaFin: new Date(reservaData.fechaFin),
    });

    setShowReservaModal(false);
    setAutoParaReserva(null);

    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: `${autoParaReserva.tipo} agregado al carrito`,
      showConfirmButton: false,
      timer: 2000,
    });
  };

  // Calcular días de alquiler
  const calcularDias = () => {
    if (reservaData.fechaInicio && reservaData.fechaFin) {
      const inicio = new Date(reservaData.fechaInicio);
      const fin = new Date(reservaData.fechaFin);
      const diff = Math.ceil((fin - inicio) / (1000 * 60 * 60 * 24));
      return diff > 0 ? diff : 0;
    }
    return 0;
  };

  // Función legacy para cuando se usa con check de disponibilidad
  const handleAddToCart = (auto) => {
    addItem({
      tipo: 'CAR',
      titulo: auto.tipo,
      detalle: `${auto.nombreProveedor} - ${auto.ciudad}`,
      precioOriginal: auto.precioNormalPorDia,
      precioFinal: auto.precioActualPorDia,
      unidadPrecio: '/día',
      servicioId: auto.servicioId,
      idProducto: auto.idAuto,
      imagenUrl: auto.uriImagen || getFallbackImage(auto.tipo),
      fechaInicio: filters.fechaInicio ? new Date(filters.fechaInicio) : null,
      fechaFin: filters.fechaFin ? new Date(filters.fechaFin) : null,
    });

    Swal.fire({
      toast: true,
      position: 'top-end',
      icon: 'success',
      title: `${auto.tipo} agregado al carrito`,
      showConfirmButton: false,
      timer: 2000,
    });
  };

  // Ver detalle del auto
  const handleViewDetail = async (auto) => {
    setLoadingDetail(true);
    setShowModal(true);
    try {
      const response = await api.get(`${ENDPOINTS.AUTOS}/${auto.servicioId}/${auto.idAuto}`);
      if (response.data.success) {
        setSelectedAuto({ ...auto, ...response.data.data });
      } else {
        setSelectedAuto(auto); // Usar datos básicos si no hay detalle
      }
    } catch (err) {
      console.error('Error fetching auto detail:', err);
      setSelectedAuto(auto); // Usar datos básicos en caso de error
    } finally {
      setLoadingDetail(false);
    }
  };

  // Verificar disponibilidad
  const handleCheckAvailability = async (auto) => {
    if (!filters.fechaInicio || !filters.fechaFin) {
      Swal.fire({
        icon: 'warning',
        title: 'Fechas requeridas',
        text: 'Por favor selecciona las fechas de recogida y entrega para verificar disponibilidad',
        confirmButtonColor: '#0d6efd',
      });
      return;
    }

    setCheckingAvailability(auto.idAuto);
    try {
      const response = await api.post(`${ENDPOINTS.AUTOS}/disponibilidad`, {
        servicioId: auto.servicioId,
        idAuto: auto.idAuto,
        fechaInicio: filters.fechaInicio,
        fechaFin: filters.fechaFin,
      });

      if (response.data.success && response.data.disponible) {
        Swal.fire({
          icon: 'success',
          title: '¡Disponible!',
          html: `<p><strong>${auto.tipo}</strong> está disponible para las fechas seleccionadas.</p>
                 <p class="text-muted small">Del ${filters.fechaInicio} al ${filters.fechaFin}</p>`,
          confirmButtonText: 'Agregar al carrito',
          showCancelButton: true,
          cancelButtonText: 'Cerrar',
          confirmButtonColor: '#198754',
        }).then((result) => {
          if (result.isConfirmed) {
            handleAddToCart(auto);
          }
        });
      } else {
        Swal.fire({
          icon: 'error',
          title: 'No disponible',
          text: `${auto.tipo} no está disponible para las fechas seleccionadas. Intenta con otras fechas.`,
          confirmButtonColor: '#0d6efd',
        });
      }
    } catch (err) {
      console.error('Error checking availability:', err);
      Swal.fire({
        icon: 'info',
        title: 'Verificación no disponible',
        text: 'No se pudo verificar la disponibilidad. El auto se agregará al carrito y se validará al procesar.',
        confirmButtonColor: '#0d6efd',
      });
    } finally {
      setCheckingAvailability(null);
    }
  };

  const closeModal = () => {
    setShowModal(false);
    setSelectedAuto(null);
  };

  return (
    <div className="container py-4">
      <PageHeader
        title="Renta de Vehículos"
        subtitle="6 proveedores integrados - SUVs, Sedanes, Pickups y más"
        icon="bi-car-front"
        gradient="primary"
      />

      {/* Formulario de búsqueda */}
      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <form onSubmit={handleSearch}>
            <div className="row g-3">
              <div className="col-md-3">
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
                  <i className="bi bi-calendar-event me-1 text-success"></i>Recogida
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
                      fechaFin: prev.fechaFin && prev.fechaFin <= newFechaInicio ? '' : prev.fechaFin
                    }));
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-calendar-event me-1 text-danger"></i>Entrega
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
                  <small className="text-muted">Selecciona recogida primero</small>
                )}
              </div>
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-people me-1 text-info"></i>Capacidad mín.
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="1"
                  max="15"
                  placeholder="Ej: 4"
                  value={filters.capacidad}
                  onChange={(e) => {
                    const val = parseInt(e.target.value) || '';
                    setFilters({ ...filters, capacidad: val ? Math.max(1, Math.min(15, val)) : '' });
                  }}
                />
              </div>
              <div className="col-md-2">
                <label className="form-label fw-bold">
                  <i className="bi bi-currency-dollar me-1 text-warning"></i>Precio máx.
                </label>
                <input
                  type="number"
                  className="form-control"
                  min="0"
                  step="10"
                  placeholder="Ej: 100"
                  value={filters.precioMax}
                  onChange={(e) => {
                    const val = parseFloat(e.target.value) || '';
                    setFilters({ ...filters, precioMax: val >= 0 ? val : '' });
                  }}
                />
              </div>
              <div className="col-md-1 d-flex align-items-end">
                <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                  {loading ? <span className="spinner-border spinner-border-sm"></span> : '🔍'}
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
            <button className="btn btn-sm btn-outline-warning ms-3" onClick={() => buscarAutos()}>
              Reintentar
            </button>
          </div>
        </div>
      )}

      {/* Loading */}
      {loading && (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Cargando...</span>
          </div>
          <p className="mt-3 text-muted">Conectando con proveedores de vehículos...</p>
        </div>
      )}

      {/* Resultados */}
      {!loading && (
        <>
          <p className="text-muted mb-3">{autos.length} vehículos encontrados</p>

          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
            {autos.map((auto, index) => {
              const imagenFinal = auto.uriImagen || getFallbackImage(auto.tipo);
              const tieneDescuento = auto.descuentoPorcentaje > 0;
              
              return (
                <div key={`${auto.servicioId}-${auto.idAuto}-${index}`} className="col">
                  <div className="card h-100 shadow-sm service-card">
                    <div className="position-relative">
                      <img
                        src={imagenFinal}
                        className="card-img-top"
                        alt={auto.tipo}
                        style={{ height: '200px', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = getFallbackImage(auto.tipo); }}
                      />
                      
                      {tieneDescuento && (
                        <span className="position-absolute top-0 end-0 m-2 badge bg-danger fs-6">
                          <i className="bi bi-fire"></i> -{auto.descuentoPorcentaje}%
                        </span>
                      )}
                      <span className="position-absolute bottom-0 start-0 m-2 badge bg-primary">
                        {auto.nombreProveedor}
                      </span>
                    </div>

                    <div className="card-body">
                      <h5 className="card-title fw-bold mb-1">{auto.tipo}</h5>
                      <p className="text-muted small mb-2">
                        <i className="bi bi-geo-alt"></i> {auto.ciudad}, {auto.pais}
                      </p>

                      <div className="d-flex gap-2 mb-3">
                        <span className="badge bg-light text-dark border">
                          <i className="bi bi-people"></i> {auto.capacidadPasajeros} pasajeros
                        </span>
                      </div>

                      <div className="d-flex justify-content-between align-items-end">
                        <div>
                          {tieneDescuento && (
                            <small className="text-muted text-decoration-line-through">
                              ${auto.precioNormalPorDia?.toFixed(2)}
                            </small>
                          )}
                          <div className="fw-bold text-primary fs-4">
                            ${auto.precioActualPorDia?.toFixed(2)} 
                            <small className="text-muted fw-normal fs-6">/día</small>
                          </div>
                        </div>

                        <div className="d-flex gap-1">
                          <button
                            className="btn btn-outline-primary btn-sm"
                            onClick={() => handleViewDetail(auto)}
                            title="Ver detalle"
                          >
                            <i className="bi bi-eye"></i>
                          </button>
                          <button
                            className="btn btn-outline-success btn-sm"
                            onClick={() => handleCheckAvailability(auto)}
                            disabled={checkingAvailability === auto.idAuto}
                            title="Verificar disponibilidad"
                          >
                            {checkingAvailability === auto.idAuto ? (
                              <span className="spinner-border spinner-border-sm"></span>
                            ) : (
                              <i className="bi bi-calendar-check"></i>
                            )}
                          </button>
                          <button
                            className="btn btn-primary btn-sm"
                            onClick={() => handleOpenReservaModal(auto)}
                            title="Reservar"
                          >
                            <i className="bi bi-cart-plus"></i>
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          {autos.length === 0 && !error && (
            <div className="text-center py-5">
              <img
                src="https://images.unsplash.com/photo-1449965408869-eaa3f722e40d?w=300&h=200&fit=crop"
                className="rounded-4 mb-4 shadow"
                alt="Autos"
              />
              <h4 className="fw-bold">
                <i className="bi bi-search"></i> Busca tu vehículo ideal
              </h4>
              <p className="text-muted">Selecciona fechas y ciudad para ver disponibilidad en tiempo real</p>
            </div>
          )}
        </>
      )}

      {/* Modal de Detalle */}
      {showModal && (
        <div className="modal fade show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-lg modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  <i className="bi bi-car-front me-2"></i>
                  {selectedAuto?.tipo || 'Detalle del Vehículo'}
                </h5>
                <button type="button" className="btn-close" onClick={closeModal}></button>
              </div>
              <div className="modal-body">
                {loadingDetail ? (
                  <div className="text-center py-5">
                    <div className="spinner-border text-primary" role="status">
                      <span className="visually-hidden">Cargando...</span>
                    </div>
                    <p className="mt-2 text-muted">Cargando información del vehículo...</p>
                  </div>
                ) : selectedAuto ? (
                  <div className="row">
                    <div className="col-md-6">
                      <img
                        src={selectedAuto.uriImagen || getFallbackImage(selectedAuto.tipo)}
                        className="img-fluid rounded shadow-sm"
                        alt={selectedAuto.tipo}
                        style={{ maxHeight: '300px', width: '100%', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = getFallbackImage(selectedAuto.tipo); }}
                      />
                      
                      {selectedAuto.descuentoPorcentaje > 0 && (
                        <div className="mt-2">
                          <span className="badge bg-danger fs-6">
                            <i className="bi bi-fire"></i> ¡{selectedAuto.descuentoPorcentaje}% de descuento!
                          </span>
                        </div>
                      )}
                    </div>
                    <div className="col-md-6">
                      <h4 className="fw-bold">{selectedAuto.tipo}</h4>
                      <p className="text-muted mb-3">
                        <i className="bi bi-building me-1"></i> {selectedAuto.nombreProveedor}
                      </p>
                      
                      <div className="mb-3">
                        <span className="badge bg-primary me-2">
                          <i className="bi bi-geo-alt"></i> {selectedAuto.ciudad}, {selectedAuto.pais}
                        </span>
                        <span className="badge bg-secondary">
                          <i className="bi bi-people"></i> {selectedAuto.capacidadPasajeros} pasajeros
                        </span>
                      </div>

                      <div className="border rounded p-3 mb-3 bg-light">
                        <h6 className="fw-bold mb-2">Características</h6>
                        <ul className="list-unstyled mb-0">
                          <li><i className="bi bi-check-circle text-success me-2"></i> Aire acondicionado</li>
                          <li><i className="bi bi-check-circle text-success me-2"></i> GPS incluido</li>
                          <li><i className="bi bi-check-circle text-success me-2"></i> Seguro básico</li>
                          <li><i className="bi bi-check-circle text-success me-2"></i> Kilómetros ilimitados</li>
                        </ul>
                      </div>

                      <div className="border rounded p-3 bg-white">
                        <div className="d-flex justify-content-between align-items-center">
                          <div>
                            {selectedAuto.descuentoPorcentaje > 0 && (
                              <small className="text-muted text-decoration-line-through d-block">
                                ${selectedAuto.precioNormalPorDia?.toFixed(2)}/día
                              </small>
                            )}
                            <span className="fs-3 fw-bold text-primary">
                              ${selectedAuto.precioActualPorDia?.toFixed(2)}
                            </span>
                            <span className="text-muted">/día</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                ) : (
                  <p className="text-muted text-center">No se encontró información del vehículo</p>
                )}
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-outline-secondary" onClick={closeModal}>
                  Cerrar
                </button>
                {selectedAuto && (
                  <>
                    <button 
                      type="button" 
                      className="btn btn-outline-success"
                      onClick={() => {
                        handleCheckAvailability(selectedAuto);
                      }}
                      disabled={checkingAvailability === selectedAuto.idAuto}
                    >
                      {checkingAvailability === selectedAuto.idAuto ? (
                        <><span className="spinner-border spinner-border-sm me-2"></span> Verificando...</>
                      ) : (
                        <><i className="bi bi-calendar-check me-2"></i> Verificar Disponibilidad</>
                      )}
                    </button>
                    <button 
                      type="button" 
                      className="btn btn-primary"
                      onClick={() => {
                        closeModal();
                        handleOpenReservaModal(selectedAuto);
                      }}
                    >
                      <i className="bi bi-cart-plus me-2"></i> Reservar
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Modal de Reserva con Fechas Obligatorias */}
      {showReservaModal && autoParaReserva && (
        <div className="modal show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header bg-primary text-white">
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
                {/* Info del vehículo */}
                <div className="d-flex mb-4 p-3 bg-light rounded">
                  <img 
                    src={autoParaReserva.uriImagen || getFallbackImage(autoParaReserva.tipo)}
                    alt={autoParaReserva.tipo}
                    style={{ width: '100px', height: '80px', objectFit: 'cover', borderRadius: '8px' }}
                  />
                  <div className="ms-3">
                    <h6 className="mb-1">{autoParaReserva.tipo}</h6>
                    <p className="text-muted small mb-1">
                      <i className="bi bi-shop me-1"></i>{autoParaReserva.nombreProveedor}
                    </p>
                    <p className="text-muted small mb-0">
                      <i className="bi bi-geo-alt me-1"></i>{autoParaReserva.ciudad}
                    </p>
                  </div>
                </div>

                {/* Formulario de fechas */}
                <div className="row g-3">
                  <div className="col-md-6">
                    <label className="form-label fw-bold">
                      <i className="bi bi-calendar-event me-1 text-success"></i>
                      Fecha de Recogida *
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
                      Fecha de Entrega *
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
                </div>

                {/* Resumen de precios */}
                {calcularDias() > 0 && (
                  <div className="mt-4 p-3 bg-primary bg-opacity-10 rounded border border-primary">
                    <div className="d-flex justify-content-between mb-2">
                      <span>Precio por día:</span>
                      <span className="fw-bold">${autoParaReserva.precioActualPorDia?.toFixed(2)}</span>
                    </div>
                    <div className="d-flex justify-content-between mb-2">
                      <span>Número de días:</span>
                      <span className="fw-bold">{calcularDias()}</span>
                    </div>
                    <hr />
                    <div className="d-flex justify-content-between">
                      <span className="fw-bold fs-5">Total estimado:</span>
                      <span className="fw-bold fs-5 text-primary">
                        ${(autoParaReserva.precioActualPorDia * calcularDias()).toFixed(2)}
                      </span>
                    </div>
                  </div>
                )}
              </div>
              <div className="modal-footer">
                <button 
                  type="button" 
                  className="btn btn-secondary" 
                  onClick={() => setShowReservaModal(false)}
                >
                  Cancelar
                </button>
                <button 
                  type="button" 
                  className="btn btn-primary"
                  onClick={handleConfirmReserva}
                >
                  <i className="bi bi-cart-plus me-2"></i>
                  Agregar al Carrito
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Autos;
