# TravelioIntegrator

Capa de orquestación que integra base de datos (EF Core), APIs de servicios (TravelioAPIConnector) y API bancaria (TravelioBankConnector). Todas las funciones tienen versión async y sync, reciben `ILogger?` y retornan null en errores catastróficos (registrados en el logger).

## Setup
1) Agrega una instancia de `TravelioDbContext` (configurada con `SqlServerContextFactory` del proyecto DatabaseConnector).
2) Crea el servicio:  
   ```csharp
   var ctx = new TravelioDbContext(SqlServerContextFactory.CreateOptions());
   var integrator = new TravelioIntegrationService(ctx);
   ```
3) Usa `ILogger` de tu preferencia (o pasa null).

## Flujo principal
- **Usuarios**: `CrearUsuarioAsync(UserCreateRequest, ILogger?)`, `IniciarSesionAsync(correo, password, ILogger?)`.
- **Catálogos** (REST/SOAP según DetalleServicio):  
  - Vuelos: `BuscarVuelosAsync(FiltroVuelos)` y `BuscarVuelosPorServicioAsync(id, filtros)`  
  - Autos: `BuscarAutosAsync`, `BuscarAutosPorServicioAsync`  
  - Habitaciones: `BuscarHabitacionesAsync`, `BuscarHabitacionesPorServicioAsync`  
  - Paquetes: `BuscarPaquetesAsync`, `BuscarPaquetesPorServicioAsync`  
  - Mesas: `BuscarMesasAsync`, `BuscarMesasPorServicioAsync`
- **Carrito** (por tipo): agregar, listar y eliminar (`Agregar*ACarritoAsync`, `ObtenerCarrito*Async`, `EliminarItemCarritoAsync`).
- **Disponibilidad**: `VerificarDisponibilidadItemAsync(TipoServicio, itemId)`.
- **Crear holds**: `CrearHoldsParaUsuarioAsync(clienteId, duracionHoldSegundos)` verifica disponibilidad y crea hold por tipo; si falla disponibilidad retorna false; si falla creación elimina el item del carrito.
- **Checkout/Reservas**: `ProcesarCompraYReservasAsync(clienteId, cuentaCliente, FacturaInfo)`  
  - Debita el total desde la cuenta del cliente a la cuenta Travelio (242).  
  - Transfiere 90% al negocio antes de reservar; si la reserva falla, reembolsa al cliente y elimina el item.  
  - Crea cliente externo (cuando aplica dentro de cada conector), reserva, factura y persiste `Compra`, `Reserva`, `ReservaCompra`; elimina items con hold.  
  - Usa precios almacenados en el carrito (ej.: vuelos = precioActual * pasajeros; habitaciones y autos multiplican por noches/días; paquetes = precioActual * personas; mesas = precio).  
- **Consultas**: `ObtenerReservasIdsPorClienteAsync(clienteId)` y `ObtenerReservaPorIdAsync(reservaId)`.

## Endpoints y URIs
Los endpoints se leen de `DetalleServicio` (UriBase + endpoint de cada operación si no es null). El servicio usa SOAP hoy (IsREST es false en TravelioAPIConnector), pero está preparado para REST cuando se habilite.

## Logging
- **Trace**: datos sensibles (ej. usuario/contraseña en login/alta).  
- **Debug**: URIs invocadas y filtros/payloads.  
- **Error**: excepciones y fallos de disponibilidad, reservas, facturación o pagos.
