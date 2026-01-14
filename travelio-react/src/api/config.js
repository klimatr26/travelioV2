// Configuración de la API
// La URL base apunta al backend MVC de .NET
export const API_BASE_URL = 'http://localhost:5226';

// Endpoints de la API
export const ENDPOINTS = {
  // Auth
  LOGIN: '/api/AuthApi/login',
  REGISTER: '/api/AuthApi/register',
  PROFILE: '/api/AuthApi/profile',
  
  // Servicios específicos - API Controllers
  AUTOS: '/api/AutosApi',
  HOTELES: '/api/HotelesApi',
  VUELOS: '/api/VuelosApi',
  RESTAURANTES: '/api/RestaurantesApi',
  PAQUETES: '/api/PaquetesApi',
  
  // Checkout
  CHECKOUT_PROCESAR: '/api/CheckoutApi/procesar',
  CHECKOUT_CANCELAR: '/api/CheckoutApi/cancelar',
  CHECKOUT_HISTORIAL: '/api/CheckoutApi/historial',
  CHECKOUT_RESERVAS: '/api/CheckoutApi/reservas',
  CHECKOUT_FACTURA_TRAVELIO: '/api/CheckoutApi/factura-travelio',
  
  // Admin
  ADMIN_STATS: '/api/AdminApi/stats',
  ADMIN_CLIENTES: '/api/AdminApi/clientes',
  ADMIN_COMPRAS: '/api/AdminApi/compras',
  ADMIN_RESERVAS: '/api/AdminApi/reservas',
  ADMIN_PROVEEDORES: '/api/AdminApi/proveedores',
  ADMIN_ACTIVIDAD: '/api/AdminApi/actividad-reciente',
};
