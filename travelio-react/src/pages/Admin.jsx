import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';
import useAuthStore from '../store/authStore';
import api from '../api/axios';
import { ENDPOINTS } from '../api/config';

const Admin = () => {
  const navigate = useNavigate();
  const { isAdmin, isLoggedIn, logout } = useAuthStore();
  const [activeTab, setActiveTab] = useState('dashboard');
  const [loading, setLoading] = useState(false);

  // Estados de datos
  const [stats, setStats] = useState({
    totalClientes: 0,
    totalCompras: 0,
    totalReservas: 0,
    totalProveedores: 0,
    ingresosMes: 0,
    reservasPendientes: 0,
  });
  const [clientes, setClientes] = useState([]);
  const [compras, setCompras] = useState([]);
  const [reservas, setReservas] = useState([]);
  const [proveedores, setProveedores] = useState([]);
  const [actividad, setActividad] = useState([]);

  // Paginación
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    if (!isLoggedIn || !isAdmin) {
      navigate('/login');
    }
  }, [isLoggedIn, isAdmin, navigate]);

  // Cargar estadísticas
  const loadStats = useCallback(async () => {
    try {
      const response = await api.get(ENDPOINTS.ADMIN_STATS);
      if (response.data.success) {
        setStats(response.data.data);
      }
    } catch (error) {
      console.error('Error cargando stats:', error);
    }
  }, []);

  // Cargar actividad reciente
  const loadActividad = useCallback(async () => {
    try {
      const response = await api.get(ENDPOINTS.ADMIN_ACTIVIDAD);
      if (response.data.success) {
        setActividad(response.data.data || []);
      }
    } catch (error) {
      console.error('Error cargando actividad:', error);
    }
  }, []);

  // Cargar clientes
  const loadClientes = useCallback(async (p = 1) => {
    setLoading(true);
    try {
      const response = await api.get(`${ENDPOINTS.ADMIN_CLIENTES}?page=${p}&limit=10`);
      if (response.data.success) {
        setClientes(response.data.data || []);
        setTotalPages(response.data.totalPages || 1);
        setPage(p);
      }
    } catch (error) {
      console.error('Error cargando clientes:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Cargar compras
  const loadCompras = useCallback(async (p = 1) => {
    setLoading(true);
    try {
      const response = await api.get(`${ENDPOINTS.ADMIN_COMPRAS}?page=${p}&limit=10`);
      if (response.data.success) {
        setCompras(response.data.data || []);
        setTotalPages(response.data.totalPages || 1);
        setPage(p);
      }
    } catch (error) {
      console.error('Error cargando compras:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Cargar reservas
  const loadReservas = useCallback(async (p = 1) => {
    setLoading(true);
    try {
      const response = await api.get(`${ENDPOINTS.ADMIN_RESERVAS}?page=${p}&limit=10`);
      if (response.data.success) {
        setReservas(response.data.data || []);
        setTotalPages(response.data.totalPages || 1);
        setPage(p);
      }
    } catch (error) {
      console.error('Error cargando reservas:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Cargar proveedores
  const loadProveedores = useCallback(async (p = 1) => {
    setLoading(true);
    try {
      const response = await api.get(`${ENDPOINTS.ADMIN_PROVEEDORES}?page=${p}&limit=10`);
      if (response.data.success) {
        setProveedores(response.data.data || []);
        setTotalPages(response.data.totalPages || 1);
        setPage(p);
      }
    } catch (error) {
      console.error('Error cargando proveedores:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Toggle proveedor activo/inactivo
  const toggleProveedor = async (id) => {
    try {
      const response = await api.put(`${ENDPOINTS.ADMIN_PROVEEDORES}/${id}/toggle`);
      if (response.data.success) {
        Swal.fire({
          toast: true,
          position: 'top-end',
          icon: 'success',
          title: response.data.message,
          showConfirmButton: false,
          timer: 1500,
        });
        loadProveedores(page);
      }
    } catch (error) {
      Swal.fire('Error', 'No se pudo actualizar el proveedor', 'error');
    }
  };

  // Cargar datos según tab activo
  useEffect(() => {
    if (!isAdmin) return;
    
    switch (activeTab) {
      case 'dashboard':
        loadStats();
        loadActividad();
        break;
      case 'clientes':
        loadClientes(1);
        break;
      case 'compras':
        loadCompras(1);
        break;
      case 'reservas':
        loadReservas(1);
        break;
      case 'proveedores':
        loadProveedores(1);
        break;
    }
  }, [activeTab, isAdmin, loadStats, loadActividad, loadClientes, loadCompras, loadReservas, loadProveedores]);

  if (!isAdmin) {
    return null;
  }

  const tabs = [
    { id: 'dashboard', icon: 'bi-speedometer2', label: 'Dashboard' },
    { id: 'clientes', icon: 'bi-people', label: 'Clientes' },
    { id: 'compras', icon: 'bi-cart-check', label: 'Compras' },
    { id: 'reservas', icon: 'bi-calendar-check', label: 'Reservas' },
    { id: 'proveedores', icon: 'bi-building', label: 'Proveedores' },
  ];

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return 'N/A';
    try {
      return new Date(dateStr).toLocaleDateString('es-EC', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateStr;
    }
  };

  const Pagination = ({ onPageChange }) => (
    totalPages > 1 && (
      <nav className="mt-3">
        <ul className="pagination pagination-sm justify-content-center">
          <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => onPageChange(page - 1)}>Anterior</button>
          </li>
          {[...Array(totalPages)].map((_, i) => (
            <li key={i} className={`page-item ${page === i + 1 ? 'active' : ''}`}>
              <button className="page-link" onClick={() => onPageChange(i + 1)}>{i + 1}</button>
            </li>
          ))}
          <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => onPageChange(page + 1)}>Siguiente</button>
          </li>
        </ul>
      </nav>
    )
  );

  return (
    <div className="admin-panel">
      {/* Header Admin */}
      <div className="admin-header bg-dark text-white py-3 px-4">
        <div className="d-flex justify-content-between align-items-center">
          <h4 className="mb-0">
            <i className="bi bi-shield-lock me-2"></i>
            Panel de Administración - Travelio
          </h4>
          <div className="d-flex align-items-center gap-3">
            <span className="badge bg-success">
              <i className="bi bi-circle-fill me-1" style={{ fontSize: '0.5rem' }}></i>
              Admin conectado
            </span>
            <button className="btn btn-outline-light btn-sm" onClick={handleLogout}>
              <i className="bi bi-box-arrow-right me-1"></i> Salir
            </button>
          </div>
        </div>
      </div>

      <div className="d-flex">
        {/* Sidebar */}
        <nav className="admin-sidebar bg-dark" style={{ width: '250px', minHeight: 'calc(100vh - 60px)' }}>
          <ul className="nav flex-column">
            {tabs.map((tab) => (
              <li className="nav-item" key={tab.id}>
                <button
                  className={`nav-link text-white w-100 text-start py-3 px-4 border-0 ${activeTab === tab.id ? 'bg-primary' : ''}`}
                  style={{ background: activeTab === tab.id ? '' : 'transparent' }}
                  onClick={() => setActiveTab(tab.id)}
                >
                  <i className={`${tab.icon} me-2`}></i>
                  {tab.label}
                </button>
              </li>
            ))}
          </ul>
        </nav>

        {/* Main Content */}
        <main className="flex-grow-1 p-4 bg-light" style={{ minHeight: 'calc(100vh - 60px)' }}>
          {loading && (
            <div className="text-center py-5">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Cargando...</span>
              </div>
            </div>
          )}

          {/* Dashboard */}
          {activeTab === 'dashboard' && !loading && (
            <div>
              <h4 className="mb-4">Dashboard</h4>
              <div className="row g-4 mb-4">
                <div className="col-md-4">
                  <div className="card border-0 shadow-sm h-100">
                    <div className="card-body">
                      <div className="d-flex justify-content-between">
                        <div>
                          <h6 className="text-muted">Total Clientes</h6>
                          <h2 className="fw-bold text-primary">{stats.totalClientes}</h2>
                        </div>
                        <div className="text-primary opacity-50" style={{ fontSize: '3rem' }}>
                          <i className="bi bi-people"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="card border-0 shadow-sm h-100">
                    <div className="card-body">
                      <div className="d-flex justify-content-between">
                        <div>
                          <h6 className="text-muted">Total Compras</h6>
                          <h2 className="fw-bold text-success">{stats.totalCompras}</h2>
                        </div>
                        <div className="text-success opacity-50" style={{ fontSize: '3rem' }}>
                          <i className="bi bi-cart-check"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="card border-0 shadow-sm h-100">
                    <div className="card-body">
                      <div className="d-flex justify-content-between">
                        <div>
                          <h6 className="text-muted">Reservas Activas</h6>
                          <h2 className="fw-bold text-warning">{stats.totalReservas}</h2>
                        </div>
                        <div className="text-warning opacity-50" style={{ fontSize: '3rem' }}>
                          <i className="bi bi-calendar-check"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="card border-0 shadow-sm h-100">
                    <div className="card-body">
                      <div className="d-flex justify-content-between">
                        <div>
                          <h6 className="text-muted">Proveedores</h6>
                          <h2 className="fw-bold text-info">{stats.totalProveedores}</h2>
                        </div>
                        <div className="text-info opacity-50" style={{ fontSize: '3rem' }}>
                          <i className="bi bi-building"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="card border-0 shadow-sm h-100">
                    <div className="card-body">
                      <div className="d-flex justify-content-between">
                        <div>
                          <h6 className="text-muted">Ingresos del Mes</h6>
                          <h2 className="fw-bold text-success">${stats.ingresosMes?.toLocaleString() || '0'}</h2>
                        </div>
                        <div className="text-success opacity-50" style={{ fontSize: '3rem' }}>
                          <i className="bi bi-cash-stack"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="col-md-4">
                  <div className="card border-0 shadow-sm h-100">
                    <div className="card-body">
                      <div className="d-flex justify-content-between">
                        <div>
                          <h6 className="text-muted">Reservas Pendientes</h6>
                          <h2 className="fw-bold text-danger">{stats.reservasPendientes}</h2>
                        </div>
                        <div className="text-danger opacity-50" style={{ fontSize: '3rem' }}>
                          <i className="bi bi-clock-history"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Actividad Reciente */}
              <div className="card border-0 shadow-sm">
                <div className="card-header bg-white">
                  <h5 className="mb-0">Actividad Reciente</h5>
                </div>
                <div className="card-body">
                  {actividad.length === 0 ? (
                    <p className="text-muted text-center py-3">No hay actividad reciente</p>
                  ) : (
                    <div className="list-group list-group-flush">
                      {actividad.map((act, index) => (
                        <div key={index} className="list-group-item d-flex align-items-center">
                          <span className={`badge bg-${act.tipo === 'Compra' ? 'success' : 'primary'} me-3`}>
                            {act.tipo}
                          </span>
                          <span>{act.descripcion} - {act.cliente}</span>
                          <small className="ms-auto text-muted">{formatDate(act.fecha)}</small>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Clientes */}
          {activeTab === 'clientes' && !loading && (
            <div>
              <div className="d-flex justify-content-between align-items-center mb-4">
                <h4>Gestión de Clientes</h4>
              </div>
              <div className="card border-0 shadow-sm">
                <div className="card-body">
                  <div className="table-responsive">
                    <table className="table table-hover">
                      <thead>
                        <tr>
                          <th>ID</th>
                          <th>Nombre Completo</th>
                          <th>Email</th>
                          <th>Documento</th>
                          <th>Teléfono</th>
                          <th>País</th>
                        </tr>
                      </thead>
                      <tbody>
                        {clientes.length === 0 ? (
                          <tr><td colSpan="6" className="text-center text-muted py-4">No hay clientes registrados</td></tr>
                        ) : (
                          clientes.map((cliente) => (
                            <tr key={cliente.id}>
                              <td>{cliente.id}</td>
                              <td><strong>{cliente.nombres} {cliente.apellidos}</strong></td>
                              <td>{cliente.email}</td>
                              <td>{cliente.documento}</td>
                              <td>{cliente.telefono || 'N/A'}</td>
                              <td>{cliente.pais || 'N/A'}</td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                  <Pagination onPageChange={loadClientes} />
                </div>
              </div>
            </div>
          )}

          {/* Compras */}
          {activeTab === 'compras' && !loading && (
            <div>
              <h4 className="mb-4">Historial de Compras</h4>
              <div className="card border-0 shadow-sm">
                <div className="card-body">
                  <div className="table-responsive">
                    <table className="table table-hover">
                      <thead>
                        <tr>
                          <th>ID Factura</th>
                          <th>Cliente</th>
                          <th>Fecha</th>
                          <th>Subtotal</th>
                          <th>IVA</th>
                          <th>Total</th>
                          <th>Estado</th>
                        </tr>
                      </thead>
                      <tbody>
                        {compras.length === 0 ? (
                          <tr><td colSpan="7" className="text-center text-muted py-4">No hay compras registradas</td></tr>
                        ) : (
                          compras.map((compra) => (
                            <tr key={compra.id}>
                              <td><strong>{compra.id}</strong></td>
                              <td>{compra.cliente}</td>
                              <td>{formatDate(compra.fecha)}</td>
                              <td>${compra.subtotal?.toFixed(2)}</td>
                              <td>${compra.iva?.toFixed(2)}</td>
                              <td className="text-success fw-bold">${compra.total?.toFixed(2)}</td>
                              <td>
                                <span className="badge bg-success">{compra.estado}</span>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                  <Pagination onPageChange={loadCompras} />
                </div>
              </div>
            </div>
          )}

          {/* Reservas */}
          {activeTab === 'reservas' && !loading && (
            <div>
              <h4 className="mb-4">Gestión de Reservas</h4>
              <div className="card border-0 shadow-sm">
                <div className="card-body">
                  <div className="table-responsive">
                    <table className="table table-hover">
                      <thead>
                        <tr>
                          <th>ID Reserva</th>
                          <th>Tipo</th>
                          <th>Servicio</th>
                          <th>Cliente</th>
                          <th>Fecha Reserva</th>
                          <th>Fecha Servicio</th>
                          <th>Estado</th>
                        </tr>
                      </thead>
                      <tbody>
                        {reservas.length === 0 ? (
                          <tr><td colSpan="7" className="text-center text-muted py-4">No hay reservas registradas</td></tr>
                        ) : (
                          reservas.map((reserva) => (
                            <tr key={reserva.id}>
                              <td><strong>{reserva.id}</strong></td>
                              <td><span className="badge bg-info">{reserva.tipo}</span></td>
                              <td>{reserva.servicio}</td>
                              <td>{reserva.cliente}</td>
                              <td>{formatDate(reserva.fecha)}</td>
                              <td>{reserva.fechaServicio}</td>
                              <td>
                                <span className={`badge bg-${
                                  reserva.estado === 'Confirmada' ? 'success' : 
                                  reserva.estado === 'Pendiente' ? 'warning' : 'danger'
                                }`}>
                                  {reserva.estado}
                                </span>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                  <Pagination onPageChange={loadReservas} />
                </div>
              </div>
            </div>
          )}

          {/* Proveedores */}
          {activeTab === 'proveedores' && !loading && (
            <div>
              <div className="d-flex justify-content-between align-items-center mb-4">
                <h4>Gestión de Proveedores</h4>
              </div>
              <div className="card border-0 shadow-sm">
                <div className="card-body">
                  <div className="table-responsive">
                    <table className="table table-hover">
                      <thead>
                        <tr>
                          <th>ID</th>
                          <th>Nombre</th>
                          <th>Tipo</th>
                          <th>Estado</th>
                          <th>Acciones</th>
                        </tr>
                      </thead>
                      <tbody>
                        {proveedores.length === 0 ? (
                          <tr><td colSpan="5" className="text-center text-muted py-4">No hay proveedores registrados</td></tr>
                        ) : (
                          proveedores.map((proveedor) => (
                            <tr key={proveedor.id}>
                              <td>{proveedor.id}</td>
                              <td><strong>{proveedor.nombre}</strong></td>
                              <td><span className="badge bg-secondary">{proveedor.tipo}</span></td>
                              <td>
                                <span className={`badge bg-${proveedor.activo ? 'success' : 'secondary'}`}>
                                  {proveedor.estado}
                                </span>
                              </td>
                              <td>
                                <button 
                                  className={`btn btn-sm btn-outline-${proveedor.activo ? 'danger' : 'success'}`}
                                  onClick={() => toggleProveedor(proveedor.id)}
                                  title={proveedor.activo ? 'Desactivar' : 'Activar'}
                                >
                                  <i className={`bi bi-${proveedor.activo ? 'x-lg' : 'check-lg'}`}></i>
                                  {proveedor.activo ? ' Desactivar' : ' Activar'}
                                </button>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>
                  <Pagination onPageChange={loadProveedores} />
                </div>
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default Admin;
