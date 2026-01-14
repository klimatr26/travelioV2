import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Home = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useState({
    q: '',
    checkIn: '',
    tipo: '',
  });
  const [featuredCars, setFeaturedCars] = useState([]);
  const [loadingCars, setLoadingCars] = useState(true);

  // Cargar autos destacados
  useEffect(() => {
    const fetchFeaturedCars = async () => {
      try {
        const response = await api.get(ENDPOINTS.AUTOS);
        if (response.data.success) {
          // Tomar solo los primeros 3 autos para la sección destacada
          setFeaturedCars((response.data.data || []).slice(0, 3));
        }
      } catch (error) {
        console.error('Error loading featured cars:', error);
      } finally {
        setLoadingCars(false);
      }
    };
    fetchFeaturedCars();
  }, []);

  const handleSearch = (e) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchParams.q) params.append('q', searchParams.q);
    if (searchParams.checkIn) params.append('checkIn', searchParams.checkIn);
    if (searchParams.tipo) params.append('tipo', searchParams.tipo);
    navigate(`/results?${params.toString()}`);
  };

  const modules = [
    {
      path: '/hoteles',
      title: 'Hoteles',
      subtitle: '6 proveedores',
      icon: 'bi-building',
      gradient: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
    },
    {
      path: '/vuelos',
      title: 'Vuelos',
      subtitle: '5 aerolíneas',
      icon: 'bi-airplane',
      gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    },
    {
      path: '/autos',
      title: 'Autos',
      subtitle: '6 rentadoras',
      icon: 'bi-car-front',
      gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    },
    {
      path: '/restaurantes',
      title: 'Restaurantes',
      subtitle: '7 restaurantes',
      icon: 'bi-cup-straw',
      gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
    },
    {
      path: '/paquetes',
      title: 'Paquetes',
      subtitle: '3 agencias',
      icon: 'bi-backpack2',
      gradient: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
      textDark: true,
    },
  ];

  const getFallbackImage = (tipo) => {
    const tipoLower = tipo?.toLowerCase() || '';
    if (tipoLower.includes('suv') || tipoLower.includes('sportage'))
      return 'https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=400&h=250&fit=crop';
    if (tipoLower.includes('sedan') || tipoLower.includes('corolla'))
      return 'https://images.unsplash.com/photo-1550355291-bbee04a92027?w=400&h=250&fit=crop';
    if (tipoLower.includes('pickup') || tipoLower.includes('hilux'))
      return 'https://images.unsplash.com/photo-1559416523-140ddc3d238c?w=400&h=250&fit=crop';
    return 'https://images.unsplash.com/photo-1502877338535-766e1452684a?w=400&h=250&fit=crop';
  };

  return (
    <>
      {/* Hero Section */}
      <div className="hero-section" style={{ marginTop: '-24px', marginLeft: '-12px', marginRight: '-12px' }}>
        <img
          src="https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?auto=format&fit=crop&w=2000&q=80"
          alt="Viajes"
        />
        <div className="hero-content">
          <h1 className="display-4 fw-bold mb-3">
            <i className="bi bi-globe-americas"></i> Tu próxima aventura comienza aquí
          </h1>
          <p className="fs-5 fw-light mb-0">
            Hoteles, vuelos, autos, restaurantes y paquetes turísticos. Todo integrado.
          </p>
        </div>
      </div>

      {/* Barra de búsqueda */}
      <div className="container" style={{ maxWidth: '1000px' }}>
        <div className="search-card">
          <form className="row g-3 align-items-end" onSubmit={handleSearch}>
            <div className="col-md-5">
              <label className="form-label small fw-bold text-muted">
                <i className="bi bi-search"></i> ¿A dónde vas?
              </label>
              <input
                type="text"
                className="form-control form-control-lg"
                placeholder="Ciudad o destino..."
                value={searchParams.q}
                onChange={(e) => setSearchParams({ ...searchParams, q: e.target.value })}
              />
            </div>
            <div className="col-md-3">
              <label className="form-label small fw-bold text-muted">
                <i className="bi bi-calendar3"></i> Fecha
              </label>
              <input
                type="date"
                className="form-control form-control-lg"
                min={new Date().toISOString().split('T')[0]}
                value={searchParams.checkIn}
                onChange={(e) => setSearchParams({ ...searchParams, checkIn: e.target.value })}
              />
            </div>
            <div className="col-md-2">
              <label className="form-label small fw-bold text-muted">
                <i className="bi bi-box"></i> Tipo
              </label>
              <select
                className="form-select form-select-lg"
                value={searchParams.tipo}
                onChange={(e) => setSearchParams({ ...searchParams, tipo: e.target.value })}
              >
                <option value="">Todos</option>
                <option value="HOTEL">Hoteles</option>
                <option value="FLIGHT">Vuelos</option>
                <option value="CAR">Autos</option>
                <option value="RESTAURANT">Restaurantes</option>
                <option value="PACKAGE">Paquetes</option>
              </select>
            </div>
            <div className="col-md-2">
              <button type="submit" className="btn btn-primary btn-lg w-100 fw-bold">
                Buscar
              </button>
            </div>
          </form>
        </div>
      </div>

      {/* Módulos Principales */}
      <section className="container py-5">
        <div className="text-center mb-5">
          <h2 className="fw-bold">
            <i className="bi bi-rocket-takeoff text-primary"></i> Explora nuestros servicios
          </h2>
          <p className="text-muted">5 módulos integrados con proveedores reales</p>
        </div>

        <div className="row g-4">
          {modules.map((module, index) => (
            <div key={index} className="col-6 col-lg">
              <Link to={module.path} className="text-decoration-none">
                <div
                  className="card module-card text-center p-4 h-100"
                  style={{ background: module.gradient }}
                >
                  <div className={`module-icon ${module.textDark ? '' : 'text-white'}`}>
                    <i className={`bi ${module.icon}`}></i>
                  </div>
                  <h5 className={`fw-bold ${module.textDark ? '' : 'text-white'} mb-1`}>{module.title}</h5>
                  <small className={module.textDark ? 'text-muted' : 'text-white-50'}>{module.subtitle}</small>
                </div>
              </Link>
            </div>
          ))}
        </div>
      </section>

      {/* Estadísticas */}
      <section className="container mb-5">
        <div className="stats-card">
          <div className="row text-center">
            <div className="col-md-3 col-6 mb-3 mb-md-0">
              <h2 className="fw-bold mb-0">27+</h2>
              <small className="opacity-75">Proveedores</small>
            </div>
            <div className="col-md-3 col-6 mb-3 mb-md-0">
              <h2 className="fw-bold mb-0">5</h2>
              <small className="opacity-75">Tipos de servicio</small>
            </div>
            <div className="col-md-3 col-6">
              <h2 className="fw-bold mb-0">100%</h2>
              <small className="opacity-75">Integración</small>
            </div>
            <div className="col-md-3 col-6">
              <h2 className="fw-bold mb-0">24/7</h2>
              <small className="opacity-75">Disponible</small>
            </div>
          </div>
        </div>
      </section>

      {/* Ofertas Destacadas */}
      <section className="container pb-5">
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h3 className="fw-bold mb-1">
              <i className="bi bi-fire text-danger"></i> Ofertas Destacadas
            </h3>
            <p className="text-muted mb-0">Vehículos de nuestros proveedores integrados</p>
          </div>
          <Link to="/autos" className="btn btn-outline-primary">
            Ver todos <i className="bi bi-arrow-right"></i>
          </Link>
        </div>

        {loadingCars ? (
          <div className="text-center py-4">
            <div className="spinner-border text-primary" role="status"></div>
            <p className="mt-2 text-muted">Cargando ofertas...</p>
          </div>
        ) : featuredCars.length === 0 ? (
          <div className="alert alert-info">
            <i className="bi bi-info-circle me-2"></i>
            No hay vehículos disponibles en este momento.
            <Link to="/autos" className="ms-2">Ver sección de autos</Link>
          </div>
        ) : (
          <div className="row g-4">
            {featuredCars.map((car, index) => {
              const imagen = car.uriImagen || getFallbackImage(car.tipo);
              return (
                <div key={`${car.servicioId}-${car.idAuto}-${index}`} className="col-md-6 col-lg-4">
                  <div className="card featured-card h-100">
                    <div className="position-relative">
                      <img
                        src={imagen}
                        className="card-img-top featured-img"
                        alt={car.tipo}
                        style={{ height: '200px', objectFit: 'cover' }}
                        onError={(e) => { e.target.src = getFallbackImage(car.tipo); }}
                      />
                      <span className="badge bg-success badge-provider">{car.nombreProveedor}</span>
                      {car.descuentoPorcentaje > 0 && (
                        <span className="position-absolute top-0 end-0 m-2 badge bg-danger">
                          -{car.descuentoPorcentaje}%
                        </span>
                      )}
                    </div>
                    <div className="card-body">
                      <h6 className="fw-bold mb-1">{car.tipo}</h6>
                      <p className="text-muted small mb-2">
                        <i className="bi bi-geo-alt"></i> {car.ciudad}, {car.pais}
                      </p>
                      <div className="d-flex justify-content-between align-items-center">
                        <div>
                          <span className="fw-bold text-primary fs-5">${car.precioActualPorDia?.toFixed(2)}</span>
                          <small className="text-muted">/día</small>
                        </div>
                        <Link to="/autos" className="btn btn-sm btn-primary">
                          Ver más
                        </Link>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </section>

      {/* Hoteles Populares */}
      <section className="container pb-5">
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h3 className="fw-bold mb-1">
              <i className="bi bi-building text-success"></i> Hoteles Populares
            </h3>
            <p className="text-muted mb-0">Habitaciones de 6 hoteles integrados</p>
          </div>
          <Link to="/hoteles" className="btn btn-outline-success">
            Ver todos <i className="bi bi-arrow-right"></i>
          </Link>
        </div>

        <div className="row g-4">
          {['Hotel Campestre', 'AllpahouseNYC', 'Brisamar', 'Hotel Andino'].map((hotel, index) => {
            const gradients = [
              'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
              'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
              'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
            ];
            return (
              <div key={index} className="col-md-6 col-lg-3">
                <div className="card featured-card h-100">
                  <div className="featured-placeholder" style={{ background: gradients[index] }}>
                    <i className="bi bi-building text-white" style={{ fontSize: '3rem' }}></i>
                  </div>
                  <div className="card-body text-center">
                    <h6 className="fw-bold mb-1">{hotel}</h6>
                    <small className="text-muted">Integrado</small>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </section>

      {/* Aerolíneas */}
      <section className="container pb-5">
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h3 className="fw-bold mb-1">
              <i className="bi bi-airplane text-warning"></i> Aerolíneas Integradas
            </h3>
            <p className="text-muted mb-0">5 aerolíneas con reservas en tiempo real</p>
          </div>
          <Link to="/vuelos" className="btn btn-outline-warning">
            Ver vuelos <i className="bi bi-arrow-right"></i>
          </Link>
        </div>

        <div className="row g-3">
          {[
            { name: 'Withfly', color: 'primary' },
            { name: 'Astrawings', color: 'danger' },
            { name: 'SkaywardAir', color: 'info' },
            { name: 'SkyAndes', color: 'success' },
            { name: 'Caribbean', color: 'warning' },
          ].map((airline, index) => (
            <div key={index} className="col">
              <div className="card text-center p-3 border-0 shadow-sm">
                <div>
                  <i className={`bi bi-airplane text-${airline.color}`} style={{ fontSize: '2rem' }}></i>
                </div>
                <small className="fw-bold">{airline.name}</small>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Restaurantes */}
      <section className="container pb-5">
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h3 className="fw-bold mb-1">
              <i className="bi bi-cup-straw text-danger"></i> Restaurantes
            </h3>
            <p className="text-muted mb-0">7 restaurantes para reservar mesas</p>
          </div>
          <Link to="/restaurantes" className="btn btn-outline-danger">
            Ver restaurantes <i className="bi bi-arrow-right"></i>
          </Link>
        </div>

        <div className="row g-3">
          {[
            { name: 'Cangrejitos Felices', type: 'Mariscos', icon: 'bi-cup-straw', color: 'danger' },
            { name: 'Dragón Rojo', type: 'Asiático', icon: 'bi-cup-hot', color: 'warning' },
            { name: 'Sánctum', type: 'Gourmet', icon: 'bi-cup', color: '#764ba2' },
            { name: 'Sabor Andino', type: 'Típico', icon: 'bi-tree', color: 'success' },
            { name: 'Bar Sinson', type: 'Bar & Grill', icon: 'bi-music-note-beamed', color: 'dark' },
            { name: '7 Mares', type: 'Mariscos', icon: 'bi-water', color: 'info' },
          ].map((restaurant, index) => (
            <div key={index} className="col-md-4">
              <div className="card p-3 border-0 shadow-sm">
                <div className="d-flex align-items-center gap-3">
                  <div
                    className={`rounded-circle d-flex align-items-center justify-content-center ${
                      !restaurant.color.startsWith('#') ? `bg-${restaurant.color}` : ''
                    }`}
                    style={{
                      width: '50px',
                      height: '50px',
                      background: restaurant.color.startsWith('#') ? restaurant.color : undefined,
                    }}
                  >
                    <i className={`bi ${restaurant.icon} text-white`}></i>
                  </div>
                  <div>
                    <h6 className="fw-bold mb-0">{restaurant.name}</h6>
                    <small className="text-muted">{restaurant.type}</small>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Call to Action */}
      <section className="container pb-5">
        <div
          className="card border-0 text-white text-center p-5"
          style={{
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            borderRadius: '20px',
          }}
        >
          <h2 className="fw-bold mb-3">
            ¿Listo para tu próximo viaje? <i className="bi bi-sun"></i>
          </h2>
          <p className="fs-5 mb-4 opacity-90">Más de 27 proveedores integrados esperando por ti</p>
          <div className="d-flex gap-3 justify-content-center flex-wrap">
            <Link to="/register" className="btn btn-light btn-lg px-5 fw-bold">
              <i className="bi bi-rocket"></i> Crear cuenta gratis
            </Link>
            <Link to="/modules" className="btn btn-outline-light btn-lg px-5">
              Ver todos los módulos
            </Link>
          </div>
        </div>
      </section>
    </>
  );
};

export default Home;
