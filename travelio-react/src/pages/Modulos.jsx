import { Link } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';

const Modulos = () => {
  const modules = [
    {
      path: '/hoteles',
      title: 'Hoteles',
      subtitle: '6 proveedores integrados',
      description: 'Hospedajes, resorts, cabañas en todo el país. Reserva habitaciones desde económicas hasta suites de lujo.',
      icon: 'bi-building',
      gradient: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
      providers: ['HotelB', 'Marriott', 'Hilton', 'Sheraton', 'Hyatt', 'Radisson'],
      features: ['Desayuno incluido', 'Cancelación gratuita', 'WiFi gratis'],
    },
    {
      path: '/vuelos',
      title: 'Vuelos',
      subtitle: '5 aerolíneas integradas',
      description: 'Vuelos nacionales e internacionales. Económica, ejecutiva y primera clase.',
      icon: 'bi-airplane',
      gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      providers: ['Withfly', 'Astrawings', 'SkaywardAir', 'SkyAndes', 'Caribbean'],
      features: ['Equipaje incluido', 'Selección de asiento', 'Cambios flexibles'],
    },
    {
      path: '/autos',
      title: 'Alquiler de Autos',
      subtitle: '6 rentadoras integradas',
      description: 'Desde económicos hasta SUVs de lujo. Recoge en aeropuertos o sucursales.',
      icon: 'bi-car-front',
      gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      providers: ['Hertz', 'Cuenca Wheels', 'LojitaGO', 'RentaCar EC', 'AutoRent', 'DriveNow'],
      features: ['Seguro incluido', 'Kilometraje ilimitado', 'GPS opcional'],
    },
    {
      path: '/restaurantes',
      title: 'Restaurantes',
      subtitle: '7 restaurantes integrados',
      description: 'Reserva mesas en los mejores restaurantes. Mariscos, asiático, gourmet y más.',
      icon: 'bi-cup-straw',
      gradient: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
      providers: ['Cangrejitos Felices', 'Dragón Rojo', 'Sánctum', 'Sabor Andino', 'Bar Sinson', '7 Mares', 'La Parrilla'],
      features: ['Menú especial', 'Zona VIP', 'Eventos privados'],
    },
    {
      path: '/paquetes',
      title: 'Paquetes Turísticos',
      subtitle: '3 agencias integradas',
      description: 'Tours completos con alojamiento, transporte y actividades. Aventura, cultura y relax.',
      icon: 'bi-backpack2',
      gradient: 'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
      textDark: true,
      providers: ['Cuenca Travel', 'World Agency', 'Paquetes Web'],
      features: ['Todo incluido', 'Guía turístico', 'Seguro de viaje'],
    },
  ];

  return (
    <div className="container py-4">
      <PageHeader
        title="Módulos de Servicios"
        subtitle="5 categorías de servicios turísticos integrados para tu viaje perfecto"
        icon="bi-grid-3x3-gap"
        gradient="primary"
      />

      <div className="row g-4">
        {modules.map((module, index) => (
          <div key={index} className="col-12">
            <div className="card border-0 shadow-sm overflow-hidden">
              <div className="row g-0">
                {/* Left Side - Icon & Gradient */}
                <div 
                  className="col-md-3 d-flex flex-column align-items-center justify-content-center p-4"
                  style={{ background: module.gradient, minHeight: '200px' }}
                >
                  <i className={`bi ${module.icon} ${module.textDark ? 'text-dark' : 'text-white'}`} style={{ fontSize: '4rem' }}></i>
                  <h4 className={`fw-bold mt-3 mb-0 ${module.textDark ? 'text-dark' : 'text-white'}`}>{module.title}</h4>
                  <small className={module.textDark ? 'text-dark' : 'text-white-50'}>{module.subtitle}</small>
                </div>

                {/* Right Side - Details */}
                <div className="col-md-9">
                  <div className="card-body p-4">
                    <p className="text-muted mb-3">{module.description}</p>

                    {/* Providers */}
                    <div className="mb-3">
                      <strong className="small text-uppercase text-muted">Proveedores:</strong>
                      <div className="d-flex flex-wrap gap-2 mt-2">
                        {module.providers.map((provider, i) => (
                          <span key={i} className="badge bg-light text-dark border">{provider}</span>
                        ))}
                      </div>
                    </div>

                    {/* Features */}
                    <div className="mb-3">
                      <strong className="small text-uppercase text-muted">Características:</strong>
                      <div className="d-flex flex-wrap gap-2 mt-2">
                        {module.features.map((feature, i) => (
                          <span key={i} className="badge bg-success">
                            <i className="bi bi-check-circle me-1"></i>{feature}
                          </span>
                        ))}
                      </div>
                    </div>

                    <Link 
                      to={module.path} 
                      className="btn btn-primary btn-lg"
                      style={{ background: module.gradient, border: 'none' }}
                    >
                      Explorar {module.title} <i className="bi bi-arrow-right ms-2"></i>
                    </Link>
                  </div>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Integración Info */}
      <div className="card border-0 shadow-sm mt-5">
        <div className="card-body p-4">
          <div className="row align-items-center">
            <div className="col-md-8">
              <h5 className="fw-bold mb-2">
                <i className="bi bi-gear me-2"></i>Integración Multi-Servicio
              </h5>
              <p className="text-muted mb-0">
                Travelio integra servicios REST y SOAP de múltiples proveedores, unificando la experiencia 
                de reserva en una sola plataforma. Todos los servicios están conectados al sistema de facturación 
                y carrito de compras unificado.
              </p>
            </div>
            <div className="col-md-4 text-md-end mt-3 mt-md-0">
              <div className="d-flex gap-2 justify-content-md-end">
                <span className="badge bg-info fs-6 p-2">REST API</span>
                <span className="badge bg-warning fs-6 p-2">SOAP</span>
                <span className="badge bg-success fs-6 p-2">Integrado</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Modulos;
