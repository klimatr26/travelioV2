import { Link, useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import useCartStore from '../store/cartStore';
import useAuthStore from '../store/authStore';

const Carrito = () => {
  const navigate = useNavigate();
  const { isLoggedIn } = useAuthStore();
  const { 
    items, 
    removeItem, 
    updateQuantity, 
    getSubtotal, 
    getIva, 
    getTotal, 
    getTotalItems,
    ivaPercent 
  } = useCartStore();

  const getTypeIcon = (tipo) => {
    switch (tipo) {
      case 'HOTEL': return '🏨';
      case 'CAR': return '🚗';
      case 'FLIGHT': return '✈️';
      case 'RESTAURANT': return '🍽️';
      case 'PACKAGE': return '🎒';
      default: return '📦';
    }
  };

  const isReservaService = (tipo) => {
    return ['CAR', 'HOTEL', 'FLIGHT', 'RESTAURANT', 'PACKAGE'].includes(tipo);
  };

  const handleRemove = (titulo) => {
    Swal.fire({
      title: '¿Eliminar del carrito?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
    }).then((result) => {
      if (result.isConfirmed) {
        removeItem(titulo);
        Swal.fire({
          toast: true,
          position: 'top-end',
          icon: 'success',
          title: 'Eliminado del carrito',
          showConfirmButton: false,
          timer: 1500,
        });
      }
    });
  };

  const handleCheckout = () => {
    if (!isLoggedIn) {
      Swal.fire({
        title: 'Inicia sesión',
        text: 'Debes iniciar sesión para continuar con el pago',
        icon: 'info',
        showCancelButton: true,
        confirmButtonText: 'Iniciar sesión',
        cancelButtonText: 'Cancelar',
      }).then((result) => {
        if (result.isConfirmed) {
          navigate('/login');
        }
      });
      return;
    }
    navigate('/checkout');
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  return (
    <section className="container py-5">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="fw-bold mb-0">🛒 Tu Carrito</h2>
        <span className="badge bg-secondary rounded-pill">
          {getTotalItems()} items
        </span>
      </div>

      <div className="row g-4">
        <div className="col-lg-8">
          <div className="card shadow-sm border-0">
            <ul className="list-group list-group-flush">
              {items.length === 0 ? (
                <li className="list-group-item text-center py-5">
                  <div className="text-muted mb-3 display-4">☹️</div>
                  <h4 className="fw-normal">Tu carrito está vacío</h4>
                  <Link to="/" className="btn btn-outline-primary mt-3">
                    Explorar servicios
                  </Link>
                </li>
              ) : (
                items.map((item, index) => (
                  <li
                    key={index}
                    className="list-group-item p-4 d-flex align-items-center gap-3"
                  >
                    {/* Imagen o icono */}
                    {item.imagenUrl ? (
                      <div
                        className="rounded-3 overflow-hidden flex-shrink-0"
                        style={{ width: '80px', height: '80px' }}
                      >
                        <img
                          src={item.imagenUrl}
                          alt={item.titulo}
                          style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                          onError={(e) => {
                            e.target.parentElement.innerHTML = `
                              <div class="bg-light d-flex align-items-center justify-content-center h-100" style="font-size: 2rem;">
                                ${getTypeIcon(item.tipo)}
                              </div>
                            `;
                          }}
                        />
                      </div>
                    ) : (
                      <div
                        className="rounded-3 bg-light d-flex align-items-center justify-content-center flex-shrink-0"
                        style={{ width: '80px', height: '80px', fontSize: '2rem' }}
                      >
                        {getTypeIcon(item.tipo)}
                      </div>
                    )}

                    {/* Info del item */}
                    <div className="flex-grow-1">
                      <div className="d-flex align-items-center gap-2 mb-1">
                        <h5 className="mb-0 fw-bold">{item.titulo}</h5>
                        {item.tieneDescuento && (
                          <span className="badge bg-danger">
                            OFERTA -{item.porcentajeDescuento}%
                          </span>
                        )}
                      </div>
                      <p className="text-muted small mb-2">
                        {item.detalle || item.tipo}
                      </p>

                      <div>
                        {item.precioOriginal > item.precioFinal && (
                          <span className="text-decoration-line-through text-muted me-2 small">
                            {formatCurrency(item.precioOriginal)}
                          </span>
                        )}
                        <span className="text-primary fw-bold fs-5">
                          {formatCurrency(item.precioFinal)}
                        </span>
                        {item.unidadPrecio && (
                          <span className="text-muted small"> {item.unidadPrecio}</span>
                        )}
                      </div>
                    </div>

                    {/* Total y controles */}
                    <div className="text-end" style={{ minWidth: '120px' }}>
                      <div className="fw-bold fs-5 mb-2">
                        {formatCurrency(item.precioFinal * item.cantidad)}
                      </div>

                      <div className="d-flex align-items-center justify-content-end gap-2">
                        {!isReservaService(item.tipo) ? (
                          <div className="input-group input-group-sm" style={{ width: '100px' }}>
                            <button
                              className="btn btn-outline-secondary"
                              type="button"
                              onClick={() => updateQuantity(item.titulo, item.cantidad - 1)}
                            >
                              -
                            </button>
                            <input
                              type="text"
                              className="form-control text-center bg-white"
                              value={item.cantidad}
                              readOnly
                            />
                            <button
                              className="btn btn-outline-secondary"
                              type="button"
                              onClick={() => updateQuantity(item.titulo, item.cantidad + 1)}
                            >
                              +
                            </button>
                          </div>
                        ) : (
                          <span className="badge bg-light text-dark border">1 reserva</span>
                        )}
                        <button
                          className="btn btn-sm btn-light text-danger border"
                          onClick={() => handleRemove(item.titulo)}
                          title="Eliminar"
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      </div>
                    </div>
                  </li>
                ))
              )}
            </ul>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="card border-0 shadow-sm sticky-top" style={{ top: '2rem' }}>
            <div className="card-body p-4">
              <h5 className="fw-bold mb-4">Resumen de compra</h5>

              <div className="d-flex justify-content-between mb-2">
                <span className="text-muted">Subtotal</span>
                <span className="fw-bold">{formatCurrency(getSubtotal())}</span>
              </div>

              <div className="d-flex justify-content-between mb-3 pb-3 border-bottom">
                <span className="text-muted">IVA ({ivaPercent}%)</span>
                <span className="fw-bold">{formatCurrency(getIva())}</span>
              </div>

              <div className="d-flex justify-content-between align-items-center mb-4">
                <span className="h5 fw-bold mb-0">Total</span>
                <span className="h3 fw-bold text-primary mb-0">
                  {formatCurrency(getTotal())}
                </span>
              </div>

              <button
                className="btn btn-primary w-100 btn-lg fw-bold"
                onClick={handleCheckout}
                disabled={items.length === 0}
              >
                Continuar al Pago
              </button>

              <div className="mt-3 text-center small text-muted">
                <i className="bi bi-shield-lock"></i> Compra 100% segura
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default Carrito;
