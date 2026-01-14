import { Link, useNavigate } from 'react-router-dom';
import useAuthStore from '../../store/authStore';
import useCartStore from '../../store/cartStore';

const Navbar = () => {
  const navigate = useNavigate();
  const { user, isLoggedIn, isAdmin, logout } = useAuthStore();
  const cartCount = useCartStore((state) => state.getTotalItems());

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <header>
      <nav className="navbar navbar-expand-lg navbar-light bg-white fixed-top border-bottom shadow-sm" style={{ zIndex: 1030 }}>
        <div className="container">
          <Link className="navbar-brand fw-bold text-dark d-flex align-items-center gap-2" to="/">
            <div
              className="bg-primary text-white rounded-circle d-flex align-items-center justify-content-center"
              style={{ width: '32px', height: '32px' }}
            >
              <i className="bi bi-airplane-fill" style={{ fontSize: '16px', transform: 'rotate(-45deg)' }}></i>
            </div>
            Travelio
          </Link>

          <button className="navbar-toggler border-0" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav">
            <span className="navbar-toggler-icon"></span>
          </button>

          <div className="collapse navbar-collapse" id="mainNav">
            <ul className="navbar-nav ms-auto mb-2 mb-lg-0 align-items-center">
              {isLoggedIn && isAdmin ? (
                // Menú para Administrador
                <>
                  <li className="nav-item ms-lg-3">
                    <Link
                      className="btn btn-danger text-white rounded-pill px-4 fw-semibold btn-sm"
                      to="/admin"
                    >
                      <i className="bi bi-shield-lock-fill me-1"></i> Panel Admin
                    </Link>
                  </li>
                  <li className="nav-item ms-2">
                    <button
                      className="btn btn-outline-secondary btn-sm rounded-pill px-3"
                      onClick={handleLogout}
                    >
                      Salir
                    </button>
                  </li>
                </>
              ) : (
                // Menú para Usuarios y Visitantes
                <>
                  <li className="nav-item">
                    <Link className="nav-link text-secondary fw-medium" to="/">
                      Explorar
                    </Link>
                  </li>
                  <li className="nav-item">
                    <Link className="nav-link text-secondary fw-medium" to="/modulos">
                      Módulos
                    </Link>
                  </li>

                  {/* Carrito */}
                  <li className="nav-item ms-lg-3">
                    <Link
                      className="nav-link position-relative btn btn-light border-0 rounded-pill px-3 d-flex align-items-center gap-2"
                      to="/carrito"
                      style={{ backgroundColor: '#f1f5f9' }}
                    >
                      <i className="bi bi-cart-fill text-dark"></i>
                      <span className="text-dark small fw-bold">Carrito</span>
                      {cartCount > 0 && (
                        <span
                          id="cart-badge"
                          className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger border border-light"
                        >
                          {cartCount}
                          <span className="visually-hidden">items</span>
                        </span>
                      )}
                    </Link>
                  </li>

                  {/* Usuario logueado o botones de login/register */}
                  {isLoggedIn ? (
                    <>
                      <li className="nav-item ms-lg-3 border-start ps-lg-3">
                        <Link className="nav-link fw-bold text-primary" to="/profile">
                          <i className="bi bi-person-circle me-1"></i> Hola, {user?.nombre}
                        </Link>
                      </li>
                      <li className="nav-item ms-2">
                        <button
                          className="btn btn-outline-secondary btn-sm rounded-pill px-3"
                          onClick={handleLogout}
                        >
                          Salir
                        </button>
                      </li>
                    </>
                  ) : (
                    <>
                      <li className="nav-item ms-lg-3 border-start ps-lg-3">
                        <Link
                          className="btn btn-link text-decoration-none text-dark fw-semibold"
                          to="/login"
                        >
                          Log in
                        </Link>
                      </li>
                      <li className="nav-item ms-2">
                        <Link
                          className="btn btn-primary text-white rounded-pill px-4 fw-semibold shadow-sm"
                          to="/register"
                        >
                          Registrarse
                        </Link>
                      </li>
                    </>
                  )}
                </>
              )}
            </ul>
          </div>
        </div>
      </nav>
    </header>
  );
};

export default Navbar;
