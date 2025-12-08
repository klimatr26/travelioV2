using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TravelioAPIConnector.Aerolinea;
using TravelioAPIConnector.Autos;
using TravelioAPIConnector.Habitaciones;
using TravelioAPIConnector.Mesas;
using TravelioAPIConnector.Paquetes;
using TravelioBankConnector;
using TravelioDatabaseConnector.Data;
using TravelioDatabaseConnector.Enums;
using TravelioDatabaseConnector.Models;
using TravelioDatabaseConnector.Models.Carrito;
using TravelioDatabaseConnector.Services;
using TravelioIntegrator.Models;
using TravelioIntegrator.Models.Carrito;
using DbReserva = TravelioDatabaseConnector.Models.Reserva;
using AerolineaConnector = TravelioAPIConnector.Aerolinea.Connector;
using AutosConnector = TravelioAPIConnector.Autos.Connector;
using HabitacionesConnector = TravelioAPIConnector.Habitaciones.Connector;
using MesasConnector = TravelioAPIConnector.Mesas.Connector;
using PaquetesConnector = TravelioAPIConnector.Paquetes.Connector;

namespace TravelioIntegrator.Services;

/// <summary>
/// Capa de orquestación que integra base de datos, APIs de servicios y API bancaria.
/// Todas las funciones devuelven null ante errores catastróficos y registran eventos en ILogger.
/// </summary>
public class TravelioIntegrationService
{
    private readonly TravelioDbContext _db;

    public TravelioIntegrationService(TravelioDbContext db)
    {
        _db = db;
    }

    private static ILogger ResolveLogger(ILogger? logger) => logger ?? NullLogger.Instance;

    private static string? BuildUri(DetalleServicio detalle, string? endpoint)
    {
        if (string.IsNullOrWhiteSpace(detalle.UriBase))
        {
            return endpoint;
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return detalle.UriBase;
        }

        return $"{detalle.UriBase.TrimEnd('/')}{endpoint}";
    }

    #region Usuarios

    public async Task<Cliente?> CrearUsuarioAsync(UserCreateRequest request, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var existing = await _db.Clientes.FirstOrDefaultAsync(c => c.CorreoElectronico == request.Correo);
            if (existing is not null)
            {
                log.LogWarning("Intento de crear usuario ya existente: {Correo}", request.Correo);
                return null;
            }

            var cliente = new Cliente
            {
                CorreoElectronico = request.Correo,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Pais = request.Pais,
                FechaNacimiento = request.FechaNacimiento,
                Telefono = request.Telefono,
                TipoIdentificacion = request.TipoIdentificacion,
                DocumentoIdentidad = request.DocumentoIdentidad
            };

            log.LogTrace("Creando usuario con datos: {Correo} {Nombre} {Apellido} {PasswordPlano}", request.Correo, request.Nombre, request.Apellido, request.PasswordPlano);

            ClientePasswordService.EstablecerPassword(cliente, request.PasswordPlano);
            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            return cliente;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al crear usuario {Correo}", request.Correo);
            return null;
        }
    }

    public Cliente? CrearUsuario(UserCreateRequest request, ILogger? logger = null) =>
        CrearUsuarioAsync(request, logger).GetAwaiter().GetResult();

    public async Task<Cliente?> IniciarSesionAsync(string correo, string passwordPlano, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.CorreoElectronico == correo);
            if (cliente is null)
            {
                return null;
            }

            log.LogTrace("Intento de login usuario {Correo} con password {PasswordPlano}", correo, passwordPlano);

            var valido = ClientePasswordService.EsPasswordValido(cliente, passwordPlano);
            return valido ? cliente : null;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al iniciar sesión para {Correo}", correo);
            return null;
        }
    }

    public Cliente? IniciarSesion(string correo, string passwordPlano, ILogger? logger = null) =>
        IniciarSesionAsync(correo, passwordPlano, logger).GetAwaiter().GetResult();

    #endregion

    #region Búsqueda de productos

    public async Task<IReadOnlyList<ProductoServicio<Vuelo>>?> BuscarVuelosAsync(FiltroVuelos filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var detalles = await _db.DetallesServicio.Include(d => d.Servicio)
                .Where(d => d.Servicio.TipoServicio == TipoServicio.Aerolinea)
                .ToListAsync();

            var resultados = new List<ProductoServicio<Vuelo>>();
            foreach (var det in detalles)
            {
                var uri = BuildUri(det, det.ObtenerProductosEndpoint);
                log.LogDebug("Buscando vuelos en {Uri} con filtros {@Filtros}", uri, filtros);
                var vuelos = await AerolineaConnector.GetVuelosAsync(
                    uri!,
                    filtros.Origen,
                    filtros.Destino,
                    filtros.FechaDespegue,
                    filtros.FechaLlegada,
                    filtros.TipoCabina,
                    filtros.Pasajeros,
                    filtros.PrecioMin,
                    filtros.PrecioMax);

                resultados.AddRange(vuelos.Select(v => new ProductoServicio<Vuelo>(det.ServicioId, det.Servicio.Nombre, v)));
            }

            return resultados;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar vuelos");
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Vuelo>>? BuscarVuelos(FiltroVuelos filtros, ILogger? logger = null) =>
        BuscarVuelosAsync(filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Vuelo>>?> BuscarVuelosPorServicioAsync(int servicioId, FiltroVuelos filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var det = await _db.DetallesServicio.Include(d => d.Servicio)
                .FirstOrDefaultAsync(d => d.ServicioId == servicioId && d.Servicio.TipoServicio == TipoServicio.Aerolinea);
            if (det is null) return Array.Empty<ProductoServicio<Vuelo>>();

            var uri = BuildUri(det, det.ObtenerProductosEndpoint);
            log.LogDebug("Buscando vuelos en servicio {ServicioId} uri {Uri} con filtros {@Filtros}", servicioId, uri, filtros);
            var vuelos = await AerolineaConnector.GetVuelosAsync(
                uri!,
                filtros.Origen,
                filtros.Destino,
                filtros.FechaDespegue,
                filtros.FechaLlegada,
                filtros.TipoCabina,
                filtros.Pasajeros,
                filtros.PrecioMin,
                filtros.PrecioMax);

            return vuelos.Select(v => new ProductoServicio<Vuelo>(det.ServicioId, det.Servicio.Nombre, v)).ToArray();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar vuelos del servicio {ServicioId}", servicioId);
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Vuelo>>? BuscarVuelosPorServicio(int servicioId, FiltroVuelos filtros, ILogger? logger = null) =>
        BuscarVuelosPorServicioAsync(servicioId, filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Vehiculo>>?> BuscarAutosAsync(FiltroAutos filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var detalles = await _db.DetallesServicio.Include(d => d.Servicio)
                .Where(d => d.Servicio.TipoServicio == TipoServicio.RentaVehiculos)
                .ToListAsync();

            var resultados = new List<ProductoServicio<Vehiculo>>();
            foreach (var det in detalles)
            {
                var uri = BuildUri(det, det.ObtenerProductosEndpoint);
                log.LogDebug("Buscando autos en {Uri} con filtros {@Filtros}", uri, filtros);
                var autos = await TravelioAPIConnector.Autos.Connector.GetVehiculosAsync(
                    uri!,
                    filtros.Categoria,
                    filtros.Transmision,
                    filtros.Capacidad,
                    filtros.PrecioMin,
                    filtros.PrecioMax,
                    filtros.Sort,
                    filtros.Ciudad,
                    filtros.Pais);

                resultados.AddRange(autos.Select(a => new ProductoServicio<Vehiculo>(det.ServicioId, det.Servicio.Nombre, a)));
            }

            return resultados;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar autos");
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Vehiculo>>? BuscarAutos(FiltroAutos filtros, ILogger? logger = null) =>
        BuscarAutosAsync(filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Vehiculo>>?> BuscarAutosPorServicioAsync(int servicioId, FiltroAutos filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var det = await _db.DetallesServicio.Include(d => d.Servicio)
                .FirstOrDefaultAsync(d => d.ServicioId == servicioId && d.Servicio.TipoServicio == TipoServicio.RentaVehiculos);
            if (det is null) return Array.Empty<ProductoServicio<Vehiculo>>();

            var uri = BuildUri(det, det.ObtenerProductosEndpoint);
            log.LogDebug("Buscando autos en servicio {ServicioId} uri {Uri} con filtros {@Filtros}", servicioId, uri, filtros);
            var autos = await TravelioAPIConnector.Autos.Connector.GetVehiculosAsync(
                uri!,
                filtros.Categoria,
                filtros.Transmision,
                filtros.Capacidad,
                filtros.PrecioMin,
                filtros.PrecioMax,
                filtros.Sort,
                filtros.Ciudad,
                filtros.Pais);

            return autos.Select(a => new ProductoServicio<Vehiculo>(det.ServicioId, det.Servicio.Nombre, a)).ToArray();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar autos del servicio {ServicioId}", servicioId);
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Vehiculo>>? BuscarAutosPorServicio(int servicioId, FiltroAutos filtros, ILogger? logger = null) =>
        BuscarAutosPorServicioAsync(servicioId, filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Habitacion>>?> BuscarHabitacionesAsync(FiltroHabitaciones filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var detalles = await _db.DetallesServicio.Include(d => d.Servicio)
                .Where(d => d.Servicio.TipoServicio == TipoServicio.Hotel)
                .ToListAsync();

            var resultados = new List<ProductoServicio<Habitacion>>();
            foreach (var det in detalles)
            {
                var uri = BuildUri(det, det.ObtenerProductosEndpoint);
                log.LogDebug("Buscando habitaciones en {Uri} con filtros {@Filtros}", uri, filtros);
                var habitaciones = await TravelioAPIConnector.Habitaciones.Connector.BuscarHabitacionesAsync(
                    uri!,
                    filtros.FechaInicio,
                    filtros.FechaFin,
                    filtros.TipoHabitacion,
                    filtros.Capacidad,
                    filtros.PrecioMin,
                    filtros.PrecioMax);

                resultados.AddRange(habitaciones.Select(h => new ProductoServicio<Habitacion>(det.ServicioId, det.Servicio.Nombre, h)));
            }

            return resultados;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar habitaciones");
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Habitacion>>? BuscarHabitaciones(FiltroHabitaciones filtros, ILogger? logger = null) =>
        BuscarHabitacionesAsync(filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Habitacion>>?> BuscarHabitacionesPorServicioAsync(int servicioId, FiltroHabitaciones filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var det = await _db.DetallesServicio.Include(d => d.Servicio)
                .FirstOrDefaultAsync(d => d.ServicioId == servicioId && d.Servicio.TipoServicio == TipoServicio.Hotel);
            if (det is null) return Array.Empty<ProductoServicio<Habitacion>>();

            var uri = BuildUri(det, det.ObtenerProductosEndpoint);
            log.LogDebug("Buscando habitaciones en servicio {ServicioId} uri {Uri} con filtros {@Filtros}", servicioId, uri, filtros);
            var habitaciones = await TravelioAPIConnector.Habitaciones.Connector.BuscarHabitacionesAsync(
                uri!,
                filtros.FechaInicio,
                filtros.FechaFin,
                filtros.TipoHabitacion,
                filtros.Capacidad,
                filtros.PrecioMin,
                filtros.PrecioMax);

            return habitaciones.Select(h => new ProductoServicio<Habitacion>(det.ServicioId, det.Servicio.Nombre, h)).ToArray();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar habitaciones del servicio {ServicioId}", servicioId);
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Habitacion>>? BuscarHabitacionesPorServicio(int servicioId, FiltroHabitaciones filtros, ILogger? logger = null) =>
        BuscarHabitacionesPorServicioAsync(servicioId, filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Paquete>>?> BuscarPaquetesAsync(FiltroPaquetes filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var detalles = await _db.DetallesServicio.Include(d => d.Servicio)
                .Where(d => d.Servicio.TipoServicio == TipoServicio.PaquetesTuristicos)
                .ToListAsync();

            var resultados = new List<ProductoServicio<Paquete>>();
            foreach (var det in detalles)
            {
                var uri = BuildUri(det, det.ObtenerProductosEndpoint);
                log.LogDebug("Buscando paquetes en {Uri} con filtros {@Filtros}", uri, filtros);
                var paquetes = await TravelioAPIConnector.Paquetes.Connector.BuscarPaquetesAsync(
                    uri!,
                    filtros.Ciudad,
                    filtros.FechaInicio,
                    filtros.TipoActividad,
                    filtros.PrecioMax);

                resultados.AddRange(paquetes.Select(p => new ProductoServicio<Paquete>(det.ServicioId, det.Servicio.Nombre, p)));
            }

            return resultados;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar paquetes turísticos");
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Paquete>>? BuscarPaquetes(FiltroPaquetes filtros, ILogger? logger = null) =>
        BuscarPaquetesAsync(filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Paquete>>?> BuscarPaquetesPorServicioAsync(int servicioId, FiltroPaquetes filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var det = await _db.DetallesServicio.Include(d => d.Servicio)
                .FirstOrDefaultAsync(d => d.ServicioId == servicioId && d.Servicio.TipoServicio == TipoServicio.PaquetesTuristicos);
            if (det is null) return Array.Empty<ProductoServicio<Paquete>>();

            var uri = BuildUri(det, det.ObtenerProductosEndpoint);
            log.LogDebug("Buscando paquetes en servicio {ServicioId} uri {Uri} con filtros {@Filtros}", servicioId, uri, filtros);
            var paquetes = await TravelioAPIConnector.Paquetes.Connector.BuscarPaquetesAsync(
                uri!,
                filtros.Ciudad,
                filtros.FechaInicio,
                filtros.TipoActividad,
                filtros.PrecioMax);

            return paquetes.Select(p => new ProductoServicio<Paquete>(det.ServicioId, det.Servicio.Nombre, p)).ToArray();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar paquetes del servicio {ServicioId}", servicioId);
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Paquete>>? BuscarPaquetesPorServicio(int servicioId, FiltroPaquetes filtros, ILogger? logger = null) =>
        BuscarPaquetesPorServicioAsync(servicioId, filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Mesa>>?> BuscarMesasAsync(FiltroMesas filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var detalles = await _db.DetallesServicio.Include(d => d.Servicio)
                .Where(d => d.Servicio.TipoServicio == TipoServicio.Restaurante)
                .ToListAsync();

            var resultados = new List<ProductoServicio<Mesa>>();
            foreach (var det in detalles)
            {
                var uri = BuildUri(det, det.ObtenerProductosEndpoint);
                log.LogDebug("Buscando mesas en {Uri} con filtros {@Filtros}", uri, filtros);
                var mesas = await TravelioAPIConnector.Mesas.Connector.BuscarMesasAsync(
                    uri!,
                    filtros.Capacidad,
                    filtros.TipoMesa,
                    filtros.Estado);

                resultados.AddRange(mesas.Select(m => new ProductoServicio<Mesa>(det.ServicioId, det.Servicio.Nombre, m)));
            }

            return resultados;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar mesas");
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Mesa>>? BuscarMesas(FiltroMesas filtros, ILogger? logger = null) =>
        BuscarMesasAsync(filtros, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<ProductoServicio<Mesa>>?> BuscarMesasPorServicioAsync(int servicioId, FiltroMesas filtros, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var det = await _db.DetallesServicio.Include(d => d.Servicio)
                .FirstOrDefaultAsync(d => d.ServicioId == servicioId && d.Servicio.TipoServicio == TipoServicio.Restaurante);
            if (det is null) return Array.Empty<ProductoServicio<Mesa>>();

            var uri = BuildUri(det, det.ObtenerProductosEndpoint);
            log.LogDebug("Buscando mesas en servicio {ServicioId} uri {Uri} con filtros {@Filtros}", servicioId, uri, filtros);
            var mesas = await TravelioAPIConnector.Mesas.Connector.BuscarMesasAsync(
                uri!,
                filtros.Capacidad,
                filtros.TipoMesa,
                filtros.Estado);

            return mesas.Select(m => new ProductoServicio<Mesa>(det.ServicioId, det.Servicio.Nombre, m)).ToArray();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al buscar mesas del servicio {ServicioId}", servicioId);
            return null;
        }
    }

    public IReadOnlyList<ProductoServicio<Mesa>>? BuscarMesasPorServicio(int servicioId, FiltroMesas filtros, ILogger? logger = null) =>
        BuscarMesasPorServicioAsync(servicioId, filtros, logger).GetAwaiter().GetResult();

    #endregion

    #region Carrito - agregar/consultar/eliminar

    public async Task<CarritoAerolineaItem?> AgregarVueloACarritoAsync(AerolineaCarritoRequest request, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var entity = new CarritoAerolineaItem
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                IdVueloProveedor = request.IdVueloProveedor,
                Origen = request.Origen,
                Destino = request.Destino,
                FechaVuelo = request.FechaVuelo,
                TipoCabina = request.TipoCabina,
                NombreAerolinea = request.NombreAerolinea,
                PrecioNormal = request.PrecioNormal,
                PrecioActual = request.PrecioActual,
                DescuentoPorcentaje = request.DescuentoPorcentaje,
                CantidadPasajeros = request.CantidadPasajeros
            };

            foreach (var p in request.Pasajeros)
            {
                entity.Pasajeros.Add(new CarritoAerolineaPasajero
                {
                    Nombre = p.Nombre,
                    Apellido = p.Apellido,
                    TipoIdentificacion = p.TipoIdentificacion,
                    Identificacion = p.Identificacion,
                    FechaNacimiento = p.FechaNacimiento
                });
            }

            _db.CarritosAerolinea.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al agregar vuelo al carrito");
            return null;
        }
    }

    public CarritoAerolineaItem? AgregarVueloACarrito(AerolineaCarritoRequest request, ILogger? logger = null) =>
        AgregarVueloACarritoAsync(request, logger).GetAwaiter().GetResult();

    public async Task<CarritoHabitacionItem?> AgregarHabitacionACarritoAsync(HabitacionCarritoRequest request, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var entity = new CarritoHabitacionItem
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                IdHabitacionProveedor = request.IdHabitacionProveedor,
                NombreHabitacion = request.NombreHabitacion,
                TipoHabitacion = request.TipoHabitacion,
                Hotel = request.Hotel,
                Ciudad = request.Ciudad,
                Pais = request.Pais,
                Capacidad = request.Capacidad,
                PrecioNormal = request.PrecioNormal,
                PrecioActual = request.PrecioActual,
                PrecioVigente = request.PrecioVigente,
                Amenidades = request.Amenidades,
                Imagenes = request.Imagenes,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                NumeroHuespedes = request.NumeroHuespedes
            };

            _db.CarritosHabitaciones.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al agregar habitación al carrito");
            return null;
        }
    }

    public CarritoHabitacionItem? AgregarHabitacionACarrito(HabitacionCarritoRequest request, ILogger? logger = null) =>
        AgregarHabitacionACarritoAsync(request, logger).GetAwaiter().GetResult();

    public async Task<CarritoAutoItem?> AgregarAutoACarritoAsync(AutoCarritoRequest request, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var entity = new CarritoAutoItem
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                IdAutoProveedor = request.IdAutoProveedor,
                Tipo = request.Tipo,
                Categoria = request.Categoria,
                Transmision = request.Transmision,
                CapacidadPasajeros = request.CapacidadPasajeros,
                PrecioNormalPorDia = request.PrecioNormalPorDia,
                PrecioActualPorDia = request.PrecioActualPorDia,
                DescuentoPorcentaje = request.DescuentoPorcentaje,
                UriImagen = request.UriImagen,
                Ciudad = request.Ciudad,
                Pais = request.Pais,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin
            };

            _db.CarritosAutos.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al agregar auto al carrito");
            return null;
        }
    }

    public CarritoAutoItem? AgregarAutoACarrito(AutoCarritoRequest request, ILogger? logger = null) =>
        AgregarAutoACarritoAsync(request, logger).GetAwaiter().GetResult();

    public async Task<CarritoPaqueteItem?> AgregarPaqueteACarritoAsync(PaqueteCarritoRequest request, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var entity = new CarritoPaqueteItem
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                IdPaqueteProveedor = request.IdPaqueteProveedor,
                Nombre = request.Nombre,
                Ciudad = request.Ciudad,
                Pais = request.Pais,
                TipoActividad = request.TipoActividad,
                Capacidad = request.Capacidad,
                PrecioNormal = request.PrecioNormal,
                PrecioActual = request.PrecioActual,
                ImagenUrl = request.ImagenUrl,
                Duracion = request.Duracion,
                FechaInicio = request.FechaInicio,
                Personas = request.Personas,
                BookingUserId = request.BookingUserId
            };

            foreach (var t in request.Turistas)
            {
                entity.Turistas.Add(new CarritoPaqueteTurista
                {
                    Nombre = t.Nombre,
                    Apellido = t.Apellido,
                    FechaNacimiento = t.FechaNacimiento,
                    TipoIdentificacion = t.TipoIdentificacion,
                    Identificacion = t.Identificacion
                });
            }

            _db.CarritosPaquetes.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al agregar paquete al carrito");
            return null;
        }
    }

    public CarritoPaqueteItem? AgregarPaqueteACarrito(PaqueteCarritoRequest request, ILogger? logger = null) =>
        AgregarPaqueteACarritoAsync(request, logger).GetAwaiter().GetResult();

    public async Task<CarritoMesaItem?> AgregarMesaACarritoAsync(MesaCarritoRequest request, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var entity = new CarritoMesaItem
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                IdMesa = request.IdMesa,
                IdRestaurante = request.IdRestaurante,
                NumeroMesa = request.NumeroMesa,
                TipoMesa = request.TipoMesa,
                Capacidad = request.Capacidad,
                Precio = request.Precio,
                ImagenUrl = request.ImagenUrl,
                EstadoMesa = request.EstadoMesa,
                FechaReserva = request.FechaReserva,
                NumeroPersonas = request.NumeroPersonas
            };

            _db.CarritosMesas.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al agregar mesa al carrito");
            return null;
        }
    }

    public CarritoMesaItem? AgregarMesaACarrito(MesaCarritoRequest request, ILogger? logger = null) =>
        AgregarMesaACarritoAsync(request, logger).GetAwaiter().GetResult();

    public async Task<bool?> EliminarItemCarritoAsync(TipoServicio tipo, int itemId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            switch (tipo)
            {
                case TipoServicio.Aerolinea:
                    var aero = await _db.CarritosAerolinea.FindAsync(itemId);
                    if (aero is null) return false;
                    _db.CarritosAerolinea.Remove(aero);
                    break;
                case TipoServicio.Hotel:
                    var hab = await _db.CarritosHabitaciones.FindAsync(itemId);
                    if (hab is null) return false;
                    _db.CarritosHabitaciones.Remove(hab);
                    break;
                case TipoServicio.RentaVehiculos:
                    var auto = await _db.CarritosAutos.FindAsync(itemId);
                    if (auto is null) return false;
                    _db.CarritosAutos.Remove(auto);
                    break;
                case TipoServicio.PaquetesTuristicos:
                    var paq = await _db.CarritosPaquetes.FindAsync(itemId);
                    if (paq is null) return false;
                    _db.CarritosPaquetes.Remove(paq);
                    break;
                case TipoServicio.Restaurante:
                    var mesa = await _db.CarritosMesas.FindAsync(itemId);
                    if (mesa is null) return false;
                    _db.CarritosMesas.Remove(mesa);
                    break;
                default:
                    return null;
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al eliminar item de carrito {Tipo} {ItemId}", tipo, itemId);
            return null;
        }
    }

    public bool? EliminarItemCarrito(TipoServicio tipo, int itemId, ILogger? logger = null) =>
        EliminarItemCarritoAsync(tipo, itemId, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<CarritoAerolineaItem>?> ObtenerCarritoVuelosAsync(int clienteId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            return await _db.CarritosAerolinea
                .Include(c => c.Pasajeros)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener carrito de vuelos para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public IReadOnlyList<CarritoAerolineaItem>? ObtenerCarritoVuelos(int clienteId, ILogger? logger = null) =>
        ObtenerCarritoVuelosAsync(clienteId, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<CarritoHabitacionItem>?> ObtenerCarritoHabitacionesAsync(int clienteId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            return await _db.CarritosHabitaciones
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener carrito de habitaciones para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public IReadOnlyList<CarritoHabitacionItem>? ObtenerCarritoHabitaciones(int clienteId, ILogger? logger = null) =>
        ObtenerCarritoHabitacionesAsync(clienteId, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<CarritoAutoItem>?> ObtenerCarritoAutosAsync(int clienteId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            return await _db.CarritosAutos
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener carrito de autos para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public IReadOnlyList<CarritoAutoItem>? ObtenerCarritoAutos(int clienteId, ILogger? logger = null) =>
        ObtenerCarritoAutosAsync(clienteId, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<CarritoPaqueteItem>?> ObtenerCarritoPaquetesAsync(int clienteId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            return await _db.CarritosPaquetes
                .Include(c => c.Turistas)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener carrito de paquetes para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public IReadOnlyList<CarritoPaqueteItem>? ObtenerCarritoPaquetes(int clienteId, ILogger? logger = null) =>
        ObtenerCarritoPaquetesAsync(clienteId, logger).GetAwaiter().GetResult();

    public async Task<IReadOnlyList<CarritoMesaItem>?> ObtenerCarritoMesasAsync(int clienteId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            return await _db.CarritosMesas
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener carrito de mesas para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public IReadOnlyList<CarritoMesaItem>? ObtenerCarritoMesas(int clienteId, ILogger? logger = null) =>
        ObtenerCarritoMesasAsync(clienteId, logger).GetAwaiter().GetResult();

    #endregion

    #region Disponibilidad y holds

    public async Task<bool?> VerificarDisponibilidadItemAsync(TipoServicio tipo, int itemId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            switch (tipo)
            {
                case TipoServicio.Aerolinea:
                    var vuelo = await _db.CarritosAerolinea.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                        .FirstOrDefaultAsync(c => c.Id == itemId);
                    if (vuelo?.Servicio.DetallesServicio.FirstOrDefault() is null) return null;
                    {
                        var uri = BuildUri(vuelo.Servicio.DetallesServicio.FirstOrDefault(), vuelo.Servicio.DetallesServicio.FirstOrDefault().ConfirmarProductoEndpoint);
                        var disponible = await AerolineaConnector.VerificarDisponibilidadVueloAsync(uri!, vuelo.IdVueloProveedor, vuelo.CantidadPasajeros);
                        return disponible;
                    }
                case TipoServicio.Hotel:
                    var hab = await _db.CarritosHabitaciones.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                        .FirstOrDefaultAsync(c => c.Id == itemId);
                    if (hab?.Servicio.DetallesServicio.FirstOrDefault() is null) return null;
                    {
                        var uri = BuildUri(hab.Servicio.DetallesServicio.FirstOrDefault(), hab.Servicio.DetallesServicio.FirstOrDefault().ConfirmarProductoEndpoint);
                        var disponible = await TravelioAPIConnector.Habitaciones.Connector.ValidarDisponibilidadAsync(
                            uri!, hab.IdHabitacionProveedor, hab.FechaInicio, hab.FechaFin);
                        return disponible;
                    }
                case TipoServicio.RentaVehiculos:
                    var auto = await _db.CarritosAutos.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                        .FirstOrDefaultAsync(c => c.Id == itemId);
                    if (auto?.Servicio.DetallesServicio.FirstOrDefault() is null) return null;
                    {
                        var uri = BuildUri(auto.Servicio.DetallesServicio.FirstOrDefault(), auto.Servicio.DetallesServicio.FirstOrDefault().ConfirmarProductoEndpoint);
                        var disponible = await TravelioAPIConnector.Autos.Connector.VerificarDisponibilidadAutoAsync(
                            uri!, auto.IdAutoProveedor, auto.FechaInicio, auto.FechaFin);
                        return disponible;
                    }
                case TipoServicio.PaquetesTuristicos:
                    var paq = await _db.CarritosPaquetes.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                        .FirstOrDefaultAsync(c => c.Id == itemId);
                    if (paq?.Servicio.DetallesServicio.FirstOrDefault() is null) return null;
                    {
                        var uri = BuildUri(paq.Servicio.DetallesServicio.FirstOrDefault(), paq.Servicio.DetallesServicio.FirstOrDefault().ConfirmarProductoEndpoint);
                        var disponible = await TravelioAPIConnector.Paquetes.Connector.ValidarDisponibilidadAsync(
                            uri!, paq.IdPaqueteProveedor, paq.FechaInicio, paq.Personas);
                        return disponible;
                    }
                case TipoServicio.Restaurante:
                    var mesa = await _db.CarritosMesas.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                        .FirstOrDefaultAsync(c => c.Id == itemId);
                    if (mesa?.Servicio.DetallesServicio.FirstOrDefault() is null) return null;
                    {
                        var uri = BuildUri(mesa.Servicio.DetallesServicio.FirstOrDefault(), mesa.Servicio.DetallesServicio.FirstOrDefault().ConfirmarProductoEndpoint);
                        var disponible = await TravelioAPIConnector.Mesas.Connector.ValidarDisponibilidadAsync(
                            uri!, mesa.IdMesa, mesa.FechaReserva, mesa.NumeroPersonas);
                        return disponible;
                    }
                default:
                    return null;
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al verificar disponibilidad {Tipo} item {ItemId}", tipo, itemId);
            return null;
        }
    }

    public bool? VerificarDisponibilidadItem(TipoServicio tipo, int itemId, ILogger? logger = null) =>
        VerificarDisponibilidadItemAsync(tipo, itemId, logger).GetAwaiter().GetResult();

    public async Task<bool?> CrearHoldsParaUsuarioAsync(int clienteId, int duracionHoldSegundos = 300, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var created = false;

            var vuelos = await _db.CarritosAerolinea.Include(c => c.Pasajeros)
                .Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
            foreach (var item in vuelos)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();

                var uriDisponibilidad = BuildUri(det, det.ConfirmarProductoEndpoint);
                log.LogDebug("Verificando disponibilidad vuelo {IdVuelo} en {Uri}", item.IdVueloProveedor, uriDisponibilidad);
                var disponible = await AerolineaConnector.VerificarDisponibilidadVueloAsync(uriDisponibilidad!, item.IdVueloProveedor, item.CantidadPasajeros);
                if (!disponible)
                {
                    log.LogError("Vuelo no disponible {IdVuelo}", item.IdVueloProveedor);
                    return false;
                }

                var pasajeros = item.Pasajeros.Select(p => (p.Nombre, p.Apellido, p.TipoIdentificacion, p.Identificacion, new DateTime(p.FechaNacimiento.Year, p.FechaNacimiento.Month, p.FechaNacimiento.Day))).ToArray();
                var uriHold = BuildUri(det, det.CrearPrerreservaEndpoint);
                log.LogDebug("Creando hold de vuelo en {Uri} por {Segundos} segundos", uriHold, duracionHoldSegundos);
                try
                {
                    var hold = await AerolineaConnector.CrearPrerreservaVueloAsync(uriHold!, item.IdVueloProveedor, pasajeros, duracionHoldSegundos);
                    item.HoldId = hold.holdId;
                    item.HoldExpira = hold.expira;
                    created = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear hold de vuelo {IdVuelo}, se elimina del carrito", item.IdVueloProveedor);
                    _db.CarritosAerolinea.Remove(item);
                }
            }

            var habitaciones = await _db.CarritosHabitaciones.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
            foreach (var item in habitaciones)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var uriDisp = BuildUri(det, det.ConfirmarProductoEndpoint);
                var disponible = await TravelioAPIConnector.Habitaciones.Connector.ValidarDisponibilidadAsync(uriDisp!, item.IdHabitacionProveedor, item.FechaInicio, item.FechaFin);
                if (!disponible)
                {
                    log.LogError("Habitación no disponible {Id}", item.IdHabitacionProveedor);
                    return false;
                }

                var uriHold = BuildUri(det, det.CrearPrerreservaEndpoint);
                log.LogDebug("Creando hold de habitación en {Uri}", uriHold);
                try
                {
                    var holdId = await TravelioAPIConnector.Habitaciones.Connector.CrearPrerreservaAsync(
                        uriHold!, item.IdHabitacionProveedor, item.FechaInicio, item.FechaFin, item.NumeroHuespedes, duracionHoldSegundos, item.PrecioActual);
                    item.HoldId = holdId;
                    item.HoldExpira = null;
                    created = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear hold de habitación {Id}, se elimina del carrito", item.IdHabitacionProveedor);
                    _db.CarritosHabitaciones.Remove(item);
                }
            }

            var autos = await _db.CarritosAutos.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
            foreach (var item in autos)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var uriDisp = BuildUri(det, det.ConfirmarProductoEndpoint);
                var disponible = await TravelioAPIConnector.Autos.Connector.VerificarDisponibilidadAutoAsync(
                    uriDisp!, item.IdAutoProveedor, item.FechaInicio, item.FechaFin);
                if (!disponible)
                {
                    log.LogError("Auto no disponible {Id}", item.IdAutoProveedor);
                    return false;
                }

                var uriHold = BuildUri(det, det.CrearPrerreservaEndpoint);
                log.LogDebug("Creando hold de auto en {Uri}", uriHold);
                try
                {
                    var hold = await TravelioAPIConnector.Autos.Connector.CrearPrerreservaAsync(
                        uriHold!, item.IdAutoProveedor, item.FechaInicio, item.FechaFin, duracionHoldSegundos);
                    item.HoldId = hold.holdId;
                    item.HoldExpira = hold.holdExpiration;
                    created = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear hold de auto {Id}, se elimina del carrito", item.IdAutoProveedor);
                    _db.CarritosAutos.Remove(item);
                }
            }

            var paquetes = await _db.CarritosPaquetes.Include(c => c.Turistas)
                .Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
            foreach (var item in paquetes)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var uriDisp = BuildUri(det, det.ConfirmarProductoEndpoint);
                var disponible = await TravelioAPIConnector.Paquetes.Connector.ValidarDisponibilidadAsync(
                    uriDisp!, item.IdPaqueteProveedor, item.FechaInicio, item.Personas);
                if (!disponible)
                {
                    log.LogError("Paquete no disponible {Id}", item.IdPaqueteProveedor);
                    return false;
                }

                var uriHold = BuildUri(det, det.CrearPrerreservaEndpoint);
                log.LogDebug("Creando hold de paquete en {Uri}", uriHold);
                try
                {
                    var hold = await TravelioAPIConnector.Paquetes.Connector.CrearHoldAsync(
                        uriHold!, item.IdPaqueteProveedor, item.BookingUserId, item.FechaInicio, item.Personas, duracionHoldSegundos);
                    item.HoldId = hold.holdId;
                    item.HoldExpira = hold.expira;
                    created = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear hold de paquete {Id}, se elimina del carrito", item.IdPaqueteProveedor);
                    _db.CarritosPaquetes.Remove(item);
                }
            }

            var mesas = await _db.CarritosMesas.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId)
                .ToListAsync();
            foreach (var item in mesas)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var uriDisp = BuildUri(det, det.ConfirmarProductoEndpoint);
                var disponible = await TravelioAPIConnector.Mesas.Connector.ValidarDisponibilidadAsync(
                    uriDisp!, item.IdMesa, item.FechaReserva, item.NumeroPersonas);
                if (!disponible)
                {
                    log.LogError("Mesa no disponible {Id}", item.IdMesa);
                    return false;
                }

                var uriHold = BuildUri(det, det.CrearPrerreservaEndpoint);
                log.LogDebug("Creando hold de mesa en {Uri}", uriHold);
                try
                {
                    var hold = await TravelioAPIConnector.Mesas.Connector.CrearPreReservaAsync(
                        uriHold!, item.IdMesa, item.FechaReserva, item.NumeroPersonas, duracionHoldSegundos);
                    item.HoldId = hold.holdId;
                    item.HoldExpira = hold.expira;
                    created = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear hold de mesa {Id}, se elimina del carrito", item.IdMesa);
                    _db.CarritosMesas.Remove(item);
                }
            }

            await _db.SaveChangesAsync();
            return created;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al crear holds para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public bool? CrearHoldsParaUsuario(int clienteId, int duracionHoldSegundos = 300, ILogger? logger = null) =>
        CrearHoldsParaUsuarioAsync(clienteId, duracionHoldSegundos, logger).GetAwaiter().GetResult();

    #endregion

    #region Pago y reservas

    public async Task<bool?> ProcesarCompraYReservasAsync(int clienteId, int cuentaCliente, FacturaInfo facturaInfo, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId);
            if (cliente is null)
            {
                log.LogError("Cliente {ClienteId} no encontrado", clienteId);
                return null;
            }

            var now = DateTime.UtcNow;

            var vuelos = await _db.CarritosAerolinea.Include(c => c.Pasajeros)
                .Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId && c.HoldId != null && (c.HoldExpira == null || c.HoldExpira > now))
                .ToListAsync();
            var habitaciones = await _db.CarritosHabitaciones.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId && c.HoldId != null)
                .ToListAsync();
            var autos = await _db.CarritosAutos.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId && c.HoldId != null && (c.HoldExpira == null || c.HoldExpira > now))
                .ToListAsync();
            var paquetes = await _db.CarritosPaquetes.Include(c => c.Turistas)
                .Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId && c.HoldId != null && (c.HoldExpira == null || c.HoldExpira > now))
                .ToListAsync();
            var mesas = await _db.CarritosMesas.Include(c => c.Servicio).ThenInclude(s => s.DetallesServicio)
                .Where(c => c.ClienteId == clienteId && c.HoldId != null && (c.HoldExpira == null || c.HoldExpira > now))
                .ToListAsync();

            var allItems = vuelos.Count + habitaciones.Count + autos.Count + paquetes.Count + mesas.Count;
            if (allItems == 0)
            {
                log.LogWarning("No hay items con hold para cliente {ClienteId}", clienteId);
                return false;
            }

            decimal total = vuelos.Sum(v => v.PrecioActual * v.CantidadPasajeros)
                             + habitaciones.Sum(h => h.PrecioVigente * Math.Max(1, (decimal)(h.FechaFin.Date - h.FechaInicio.Date).TotalDays))
                             + autos.Sum(a => a.PrecioActualPorDia * Math.Max(1, (decimal)(a.FechaFin.Date - a.FechaInicio.Date).TotalDays))
                             + paquetes.Sum(p => p.PrecioActual * p.Personas)
                             + mesas.Sum(m => m.Precio);

            log.LogDebug("Total a debitar de la cuenta {CuentaCliente}: {Total}", cuentaCliente, total);
            var debitoOk = await TransferirClass.RealizarTransferenciaAsync(TransferirClass.cuentaDefaultTravelio, total, cuentaCliente);
            if (!debitoOk)
            {
                log.LogError("No se pudo debitar el total {Total} de la cuenta del cliente {Cuenta}", total, cuentaCliente);
                return null;
            }

            var compra = new Compra
            {
                ClienteId = clienteId,
                FechaCompra = DateTime.UtcNow,
                ValorPagado = total
            };
            _db.Compras.Add(compra);
            await _db.SaveChangesAsync();

            var huboExito = false;

            async Task<bool> PagarNegocioAsync(Servicio servicio, decimal monto)
            {
                if (!int.TryParse(servicio.NumeroCuenta, out var cuentaDestino))
                {
                    log.LogError("Número de cuenta inválido para servicio {ServicioId}", servicio.Id);
                    return false;
                }

                return await TransferirClass.RealizarTransferenciaAsync(cuentaDestino, monto, TransferirClass.cuentaDefaultTravelio);
            }

            foreach (var item in vuelos)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var monto = item.PrecioActual * item.CantidadPasajeros;
                var montoNegocio = monto * 0.9m;

                if (!await PagarNegocioAsync(item.Servicio, montoNegocio))
                {
                    log.LogError("No se pudo transferir al servicio {ServicioId} por vuelo {CarritoId}", item.ServicioId, item.Id);
                    continue;
                }

                var pasajeros = item.Pasajeros.Select(p => (p.Nombre, p.Apellido, p.TipoIdentificacion, p.Identificacion, new DateTime(p.FechaNacimiento.Year, p.FechaNacimiento.Month, p.FechaNacimiento.Day))).ToArray();
                var uriReserva = BuildUri(det, det.CrearReservaEndpoint);
                try
                {
                    var reserva = await AerolineaConnector.CrearReservaAsync(
                        uriReserva!,
                        item.IdVueloProveedor,
                        item.HoldId ?? string.Empty,
                        cliente.CorreoElectronico,
                        pasajeros);

                    var uriFactura = BuildUri(det, det.GenerarFacturaEndpoint);
                    string? facturaUrl = null;
                    try
                    {
                        facturaUrl = await AerolineaConnector.GenerarFacturaAsync(
                            uriFactura!,
                            reserva.idReserva,
                            monto,
                            0m,
                            monto,
                            (facturaInfo.NombreFactura, facturaInfo.TipoDocumento, facturaInfo.Documento, facturaInfo.CorreoFactura));
                    }
                    catch (Exception invoiceEx)
                    {
                        log.LogError(invoiceEx, "Error al generar factura de vuelo {CarritoId}", item.Id);
                    }

                    var reservaDb = new DbReserva
                    {
                        ServicioId = item.ServicioId,
                        CodigoReserva = reserva.codigoReserva,
                        FacturaUrl = facturaUrl
                    };
                    _db.Reservas.Add(reservaDb);
                    await _db.SaveChangesAsync();
                    _db.ReservasCompra.Add(new ReservaCompra { CompraId = compra.Id, ReservaId = reservaDb.Id });

                    _db.CarritosAerolinea.Remove(item);
                    huboExito = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear reserva de vuelo {CarritoId}, se devuelve monto", item.Id);
                    await TransferirClass.RealizarTransferenciaAsync(cuentaCliente, monto, TransferirClass.cuentaDefaultTravelio);
                    _db.CarritosAerolinea.Remove(item);
                }
            }

            foreach (var item in habitaciones)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var noches = Math.Max(1, (decimal)(item.FechaFin.Date - item.FechaInicio.Date).TotalDays);
                var monto = item.PrecioVigente * noches;
                var montoNegocio = monto * 0.9m;

                if (!await PagarNegocioAsync(item.Servicio, montoNegocio))
                {
                    log.LogError("No se pudo transferir al hotel {ServicioId} por habitación {CarritoId}", item.ServicioId, item.Id);
                    continue;
                }

                var uriReserva = BuildUri(det, det.CrearReservaEndpoint);
                try
                {
                    var reservaId = await TravelioAPIConnector.Habitaciones.Connector.CrearReservaAsync(
                        uriReserva!,
                        item.IdHabitacionProveedor,
                        item.HoldId ?? string.Empty,
                        cliente.Nombre,
                        cliente.Apellido,
                        cliente.CorreoElectronico,
                        cliente.TipoIdentificacion,
                        cliente.DocumentoIdentidad,
                        item.FechaInicio,
                        item.FechaFin,
                        item.NumeroHuespedes);

                    var uriFactura = BuildUri(det, det.GenerarFacturaEndpoint);
                    string? facturaUrl = null;
                    try
                    {
                        facturaUrl = await TravelioAPIConnector.Habitaciones.Connector.EmitirFacturaAsync(
                            uriFactura!,
                            reservaId,
                            facturaInfo.NombreFactura,
                            facturaInfo.Documento,
                            facturaInfo.TipoDocumento,
                            facturaInfo.Documento,
                            facturaInfo.CorreoFactura);
                    }
                    catch (Exception invoiceEx)
                    {
                        log.LogError(invoiceEx, "Error al generar factura de habitación {CarritoId}", item.Id);
                    }

                    var reservaDb = new DbReserva
                    {
                        ServicioId = item.ServicioId,
                        CodigoReserva = reservaId.ToString(),
                        FacturaUrl = facturaUrl
                    };
                    _db.Reservas.Add(reservaDb);
                    await _db.SaveChangesAsync();
                    _db.ReservasCompra.Add(new ReservaCompra { CompraId = compra.Id, ReservaId = reservaDb.Id });
                    _db.CarritosHabitaciones.Remove(item);
                    huboExito = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear reserva de habitación {CarritoId}, se devuelve monto", item.Id);
                    await TransferirClass.RealizarTransferenciaAsync(cuentaCliente, monto, TransferirClass.cuentaDefaultTravelio);
                    _db.CarritosHabitaciones.Remove(item);
                }
            }

            foreach (var item in autos)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var dias = Math.Max(1, (decimal)(item.FechaFin.Date - item.FechaInicio.Date).TotalDays);
                var monto = item.PrecioActualPorDia * dias;
                var montoNegocio = monto * 0.9m;

                if (!await PagarNegocioAsync(item.Servicio, montoNegocio))
                {
                    log.LogError("No se pudo transferir al rent-a-car {ServicioId} por auto {CarritoId}", item.ServicioId, item.Id);
                    continue;
                }

                var uriReserva = BuildUri(det, det.CrearReservaEndpoint);
                try
                {
                    var reservaId = await TravelioAPIConnector.Autos.Connector.CrearReservaAsync(
                        uriReserva!,
                        item.IdAutoProveedor,
                        item.HoldId ?? string.Empty,
                        cliente.Nombre,
                        cliente.Apellido,
                        cliente.TipoIdentificacion,
                        cliente.DocumentoIdentidad,
                        cliente.CorreoElectronico,
                        item.FechaInicio,
                        item.FechaFin);

                    var uriFactura = BuildUri(det, det.GenerarFacturaEndpoint);
                    string? facturaUrl = null;
                    try
                    {
                        facturaUrl = await TravelioAPIConnector.Autos.Connector.GenerarFacturaAsync(
                            uriFactura!,
                            reservaId,
                            monto,
                            0m,
                            monto,
                            (facturaInfo.NombreFactura, facturaInfo.TipoDocumento, facturaInfo.Documento, facturaInfo.CorreoFactura));
                    }
                    catch (Exception invoiceEx)
                    {
                        log.LogError(invoiceEx, "Error al generar factura de auto {CarritoId}", item.Id);
                    }

                    var reservaDb = new DbReserva
                    {
                        ServicioId = item.ServicioId,
                        CodigoReserva = reservaId.ToString(),
                        FacturaUrl = facturaUrl
                    };
                    _db.Reservas.Add(reservaDb);
                    await _db.SaveChangesAsync();
                    _db.ReservasCompra.Add(new ReservaCompra { CompraId = compra.Id, ReservaId = reservaDb.Id });
                    _db.CarritosAutos.Remove(item);
                    huboExito = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear reserva de auto {CarritoId}, se devuelve monto", item.Id);
                    await TransferirClass.RealizarTransferenciaAsync(cuentaCliente, monto, TransferirClass.cuentaDefaultTravelio);
                    _db.CarritosAutos.Remove(item);
                }
            }

            foreach (var item in paquetes)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var monto = item.PrecioActual * item.Personas;
                var montoNegocio = monto * 0.9m;

                if (!await PagarNegocioAsync(item.Servicio, montoNegocio))
                {
                    log.LogError("No se pudo transferir al proveedor de paquetes {ServicioId} por paquete {CarritoId}", item.ServicioId, item.Id);
                    continue;
                }

                var turistas = item.Turistas.Select(t => (t.Nombre, t.Apellido, t.FechaNacimiento.HasValue ? new DateTime?(new DateTime(t.FechaNacimiento.Value.Year, t.FechaNacimiento.Value.Month, t.FechaNacimiento.Value.Day)) : null, t.TipoIdentificacion, t.Identificacion)).ToArray();
                var uriReserva = BuildUri(det, det.CrearReservaEndpoint);
                try
                {
                    var reserva = await TravelioAPIConnector.Paquetes.Connector.CrearReservaAsync(
                        uriReserva!,
                        item.IdPaqueteProveedor,
                        item.HoldId ?? string.Empty,
                        item.BookingUserId,
                        "TRANSFER",
                        turistas);

                    var uriFactura = BuildUri(det, det.GenerarFacturaEndpoint);
                    string? facturaUrl = null;
                    try
                    {
                        facturaUrl = await TravelioAPIConnector.Paquetes.Connector.EmitirFacturaAsync(
                            uriFactura!,
                            reserva.IdReserva,
                            monto,
                            0m,
                            monto);
                    }
                    catch (Exception invoiceEx)
                    {
                        log.LogError(invoiceEx, "Error al generar factura de paquete {CarritoId}", item.Id);
                    }

                    var reservaDb = new DbReserva
                    {
                        ServicioId = item.ServicioId,
                        CodigoReserva = reserva.CodigoReserva,
                        FacturaUrl = facturaUrl
                    };
                    _db.Reservas.Add(reservaDb);
                    await _db.SaveChangesAsync();
                    _db.ReservasCompra.Add(new ReservaCompra { CompraId = compra.Id, ReservaId = reservaDb.Id });
                    _db.CarritosPaquetes.Remove(item);
                    huboExito = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear reserva de paquete {CarritoId}, se devuelve monto", item.Id);
                    await TransferirClass.RealizarTransferenciaAsync(cuentaCliente, monto, TransferirClass.cuentaDefaultTravelio);
                    _db.CarritosPaquetes.Remove(item);
                }
            }

            foreach (var item in mesas)
            {
                if (item.Servicio.DetallesServicio.FirstOrDefault() is null) continue;
                var det = item.Servicio.DetallesServicio.FirstOrDefault();
                var monto = item.Precio;
                var montoNegocio = monto * 0.9m;

                if (!await PagarNegocioAsync(item.Servicio, montoNegocio))
                {
                    log.LogError("No se pudo transferir al restaurante {ServicioId} por mesa {CarritoId}", item.ServicioId, item.Id);
                    continue;
                }

                var uriReserva = BuildUri(det, det.CrearReservaEndpoint);
                try
                {
                    var reserva = await TravelioAPIConnector.Mesas.Connector.ConfirmarReservaAsync(
                        uriReserva!,
                        item.IdMesa,
                        item.HoldId ?? string.Empty,
                        cliente.Nombre,
                        cliente.Apellido,
                        cliente.CorreoElectronico,
                        cliente.TipoIdentificacion,
                        cliente.DocumentoIdentidad,
                        item.FechaReserva,
                        item.NumeroPersonas);

                    var uriFactura = BuildUri(det, det.GenerarFacturaEndpoint);
                    string? facturaUrl = null;
                    try
                    {
                        facturaUrl = await TravelioAPIConnector.Mesas.Connector.GenerarFacturaAsync(
                            uriFactura!,
                            reserva.IdReserva,
                            facturaInfo.CorreoFactura,
                            facturaInfo.NombreFactura,
                            facturaInfo.TipoDocumento,
                            facturaInfo.Documento,
                            monto);
                    }
                    catch (Exception invoiceEx)
                    {
                        log.LogError(invoiceEx, "Error al generar factura de mesa {CarritoId}", item.Id);
                    }

                    var reservaDb = new DbReserva
                    {
                        ServicioId = item.ServicioId,
                        CodigoReserva = reserva.IdReserva ?? string.Empty,
                        FacturaUrl = facturaUrl
                    };
                    _db.Reservas.Add(reservaDb);
                    await _db.SaveChangesAsync();
                    _db.ReservasCompra.Add(new ReservaCompra { CompraId = compra.Id, ReservaId = reservaDb.Id });
                    _db.CarritosMesas.Remove(item);
                    huboExito = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error al crear reserva de mesa {CarritoId}, se devuelve monto", item.Id);
                    await TransferirClass.RealizarTransferenciaAsync(cuentaCliente, monto, TransferirClass.cuentaDefaultTravelio);
                    _db.CarritosMesas.Remove(item);
                }
            }

            await _db.SaveChangesAsync();
            return huboExito;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al procesar compra y reservas para cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public bool? ProcesarCompraYReservas(int clienteId, int cuentaCliente, FacturaInfo facturaInfo, ILogger? logger = null) =>
        ProcesarCompraYReservasAsync(clienteId, cuentaCliente, facturaInfo, logger).GetAwaiter().GetResult();

    #endregion
    #region Consultas de reservas

    public async Task<int[]?> ObtenerReservasIdsPorClienteAsync(int clienteId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            var ids = await _db.ReservasCompra
                .Include(rc => rc.Compra)
                .Where(rc => rc.Compra.ClienteId == clienteId)
                .Select(rc => rc.ReservaId)
                .Distinct()
                .ToArrayAsync();
            return ids;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener ids de reservas del cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public int[]? ObtenerReservasIdsPorCliente(int clienteId, ILogger? logger = null) =>
        ObtenerReservasIdsPorClienteAsync(clienteId, logger).GetAwaiter().GetResult();

    public async Task<DbReserva?> ObtenerReservaPorIdAsync(int reservaId, ILogger? logger = null)
    {
        var log = ResolveLogger(logger);
        try
        {
            return await _db.Reservas.FirstOrDefaultAsync(r => r.Id == reservaId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error al obtener reserva {ReservaId}", reservaId);
            return null;
        }
    }

    public DbReserva? ObtenerReservaPorId(int reservaId, ILogger? logger = null) =>
        ObtenerReservaPorIdAsync(reservaId, logger).GetAwaiter().GetResult();

    #endregion
}
