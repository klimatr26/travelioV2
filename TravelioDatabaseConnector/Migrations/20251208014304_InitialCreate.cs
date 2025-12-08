using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TravelioDatabaseConnector.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DocumentoIdentidad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoServicio = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorPagado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FacturaUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritosAerolinea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IdVueloProveedor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaVuelo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoCabina = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NombreAerolinea = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PrecioNormal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DescuentoPorcentaje = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    CantidadPasajeros = table.Column<int>(type: "int", nullable: false),
                    HoldId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HoldExpira = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritosAerolinea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritosAerolinea_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritosAerolinea_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritosAutos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IdAutoProveedor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Transmision = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CapacidadPasajeros = table.Column<int>(type: "int", nullable: false),
                    PrecioNormalPorDia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioActualPorDia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DescuentoPorcentaje = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    UriImagen = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoldId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HoldExpira = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritosAutos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritosAutos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritosAutos_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritosHabitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IdHabitacionProveedor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NombreHabitacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoHabitacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Hotel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    PrecioNormal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioVigente = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amenidades = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Imagenes = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroHuespedes = table.Column<int>(type: "int", nullable: false),
                    HoldId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HoldExpira = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritosHabitaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritosHabitaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritosHabitaciones_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritosMesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IdMesa = table.Column<int>(type: "int", nullable: false),
                    IdRestaurante = table.Column<int>(type: "int", nullable: false),
                    NumeroMesa = table.Column<int>(type: "int", nullable: false),
                    TipoMesa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    EstadoMesa = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroPersonas = table.Column<int>(type: "int", nullable: false),
                    HoldId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HoldExpira = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritosMesas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritosMesas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritosMesas_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritosPaquetes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    IdPaqueteProveedor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TipoActividad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    PrecioNormal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Duracion = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Personas = table.Column<int>(type: "int", nullable: false),
                    BookingUserId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HoldId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    HoldExpira = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritosPaquetes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritosPaquetes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritosPaquetes_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    TipoProtocolo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UriBase = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ObtenerProductosEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RegistrarClienteEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ConfirmarProductoEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CrearPrerreservaEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CrearReservaEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    GenerarFacturaEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ObtenerReservaEndpoint = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesServicio_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    CodigoReserva = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FacturaUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarritoAerolineaPasajeros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarritoAerolineaItemId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoAerolineaPasajeros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritoAerolineaPasajeros_CarritosAerolinea_CarritoAerolineaItemId",
                        column: x => x.CarritoAerolineaItemId,
                        principalTable: "CarritosAerolinea",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarritoPaqueteTuristas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarritoPaqueteItemId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    TipoIdentificacion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoPaqueteTuristas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritoPaqueteTuristas_CarritosPaquetes_CarritoPaqueteItemId",
                        column: x => x.CarritoPaqueteItemId,
                        principalTable: "CarritosPaquetes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservasCompra",
                columns: table => new
                {
                    CompraId = table.Column<int>(type: "int", nullable: false),
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasCompra", x => new { x.CompraId, x.ReservaId });
                    table.ForeignKey(
                        name: "FK_ReservasCompra_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservasCompra_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "Id", "Activo", "Nombre", "NumeroCuenta", "TipoServicio" },
                values: new object[,]
                {
                    { 1, true, "Withfly", "265", "Aerolinea" },
                    { 2, true, "Astrawings", "192", "Aerolinea" },
                    { 3, true, "SkaywardAir", "247", "Aerolinea" },
                    { 4, true, "SkyAndes", "160", "Aerolinea" },
                    { 5, true, "Caribbean Skyways", "244", "Aerolinea" },
                    { 101, true, "Hotel Campestre", "285", "Hotel" },
                    { 102, true, "AllpahouseNYC", "275", "Hotel" },
                    { 103, true, "Reca", "261", "Hotel" },
                    { 104, true, "Brisamar", "280", "Hotel" },
                    { 105, true, "Hotel Andino", "297", "Hotel" },
                    { 106, true, "Aureacuen", "256", "Hotel" },
                    { 201, true, "Cuenca Wheels", "174", "RentaVehiculos" },
                    { 202, true, "LojitaGO", "185", "RentaVehiculos" },
                    { 203, true, "EasyCar", "221", "RentaVehiculos" },
                    { 204, true, "Auto Car Rent", "230", "RentaVehiculos" },
                    { 205, true, "RentaAutosGYE", "225", "RentaVehiculos" },
                    { 206, true, "UrbanDrive NY", "227", "RentaVehiculos" },
                    { 301, true, "World Agency", "198", "PaquetesTuristicos" },
                    { 302, true, "Cuenca Travel", "299", "PaquetesTuristicos" },
                    { 303, true, "Paquetes Turísticos Web", "220", "PaquetesTuristicos" },
                    { 401, true, "Cangrejitos Felices", "218", "Restaurante" },
                    { 402, true, "Dragón Rojo", "216", "Restaurante" },
                    { 403, false, "Café San Juan", "1", "Restaurante" },
                    { 404, true, "Sánctum", "215", "Restaurante" },
                    { 405, true, "Sabor Andino", "210", "Restaurante" },
                    { 406, true, "Bar Sinson", "167", "Restaurante" },
                    { 407, true, "7 Mares", "171", "Restaurante" }
                });

            migrationBuilder.InsertData(
                table: "DetallesServicio",
                columns: new[] { "Id", "ConfirmarProductoEndpoint", "CrearPrerreservaEndpoint", "CrearReservaEndpoint", "GenerarFacturaEndpoint", "ObtenerProductosEndpoint", "ObtenerReservaEndpoint", "RegistrarClienteEndpoint", "ServicioId", "TipoProtocolo", "UriBase" },
                values: new object[,]
                {
                    { 1, "", "", "", "", "", "", "", 1, "Soap", "http://withflysoaint.runasp.net/IntegracionService.asmx?wsdl" },
                    { 2, "", "", "", "", "", "", "", 2, "Soap", "http://astrawingss.runasp.net/IntegracionService.asmx?wsdl" },
                    { 3, "", "", "", "", "", "", "", 3, "Soap", "http://skaywardairsoap.runasp.net/IntegracionService.asmx?WSDL" },
                    { 4, "", "", "", "", "", "", "", 4, "Soap", "https://skyandesairlines-ws.runasp.net/SkyAndes_Integracion/WS_Integracion.asmx?WSDL" },
                    { 5, "", "", "", "", "", "", "", 5, "Soap", "http://caribbeanskyways.runasp.net/WS_Integracion.asmx?WSDL" },
                    { 101, "/ValidarDisponibilidadWS.asmx?WSDL", "/CrearPreReservaWS.asmx?WSDL", "/ReservarHabitacionWS.asmx?WSDL", "/EmitirFacturaHotelWS.asmx?WSDL", "/BuscarHabitacionesWS.asmx?WSDL", "/BuscarDatosReservaWS.asmx?WSDL", "/CrearUsuarioExternoWS.asmx?WSDL", 101, "Soap", "http://hotelecampestresoap.runasp.net" },
                    { 102, "/ValidarDisponibilidadWS.asmx?WSDL", "/CrearPreReservaWS.asmx?WSDL", "/ReservarHabitacionWS.asmx?WSDL", "/EmitirFacturaHotelWS.asmx?WSDL", "/BuscarHabitacionesWS.asmx?WSDL", "/BuscarDatosReservaWS.asmx?WSDL", "/CrearUsuarioExternoWS.asmx?WSDL", 102, "Soap", "http://allpahousenyc.runasp.net" },
                    { 103, "/ValidarDisponibilidadWS.asmx?WSDL", "/CrearPreReservaWS.asmx?WSDL", "/ReservarHabitacionWS.asmx?WSDL", "/EmitirFacturaHotelWS.asmx?WSDL", "/buscarHabitacionesWS.asmx?WSDL", "/buscarDatosReservaWS.asmx?WSDL", "/CrearUsuarioExternoWS.asmx?WSDL", 103, "Soap", "https://intehoca-eheqd8h6bvdyfqfy.canadacentral-01.azurewebsites.net" },
                    { 104, "/ValidarDisponibilidadWS.asmx?WSDL", "/CrearPreReservaWS.asmx?WSDL", "/ReservarHabitacionWS.asmx?WSDL", "/EmitirFacturaHotelWS.asmx?WSDL", "/buscarHabitacionesWS.asmx?WSDL", "/buscarDatosReservaWS.asmx?WSDL", "/CrearUsuarioExternoWS.asmx?WSDL", 104, "Soap", "http://soapbrisamar.runasp.net" },
                    { 105, "/ValidarDisponibilidadWS.asmx?WSDL", "/CrearPreReservaWS.asmx?WSDL", "/ReservarHabitacionWS.asmx?WSDL", "/EmitirFacturaHotelWS.asmx?WSDL", "/BuscarHabitacionesWS.asmx?WSDL", "/BuscarDatosReservaWS.asmx?WSDL", "/CrearUsuarioExternoWS.asmx?WSDL", 105, "Soap", "http://hotelandinosoap.runasp.net" },
                    { 106, "/ValidarDisponibilidadWS.asmx?WSDL", "/CrearPreReservaWS.asmx?WSDL", "/ReservarHabitacionWS.asmx?WSDL", "/EmitirFacturaHotelWS.asmx?WSDL", "/buscarHabitacionesWS.asmx?WSDL", "/buscarDatosReservaWS.asmx?WSDL", "/CrearUsuarioExternoWS.asmx?WSDL", 106, "Soap", "http://aureacuen.runasp.net" },
                    { 201, "/WS_DisponibilidadAutos.asmx?WSDL", "/WS_PreReserva.asmx?WSDL", "/WS_ReservarAutos.asmx?WSDL", "/WS_FacturaIntegracion.asmx?WSDL", "/WS_BuscarAutos.asmx?WSDL", "/WS_BuscarDatos.asmx?WSDL", "/WS_UsuarioExterno.asmx?WSDL", 201, "Soap", "http://cuencautosrenta.runasp.net" },
                    { 202, "/WS_DisponibilidadAutos.asmx?WSDL", "/WS_PreReserva.asmx?WSDL", "/WS_ReservarAutos.asmx?WSDL", "/WS_FacturaIntegracion.asmx?WSDL", "/WS_BuscarAutos.asmx?WSDL", "/WS_BuscarDatos.asmx?WSDL", "/WS_UsuarioExterno.asmx?WSDL", 202, "Soap", "http://integsoap.runasp.net" },
                    { 203, "/WS_DisponibilidadAutos.asmx?WSDL", "/WS_PreReserva.asmx?WSDL", "/WS_ReservarAutos.asmx?WSDL", "/WS_FacturaIntegracion.asmx?WSDL", "/WS_BuscarAutos.asmx?WSDL", "/WS_BuscarDatos.asmx?WSDL", "/WS_UsuarioExterno.asmx?WSDL", 203, "Soap", "http://gestintsoa.runasp.net" },
                    { 204, "/WS_DisponibilidadAutos.asmx?WSDL", "/WS_PreReserva.asmx?WSDL", "/WS_ReservarAutos.asmx?WSDL", "/WS_FacturaIntegracion.asmx?WSDL", "/WS_BuscarAutos.asmx?WSDL", "/WS_BuscarDatos.asmx?WSDL", "/WS_UsuarioExterno.asmx?WSDL", 204, "Soap", "http://autocarrent.runasp.net" },
                    { 205, "/WS_DisponibilidadAutos.asmx?WSDL", "/WS_PreReserva.asmx?WSDL", "/WS_ReservarAutos.asmx?WSDL", "/WS_FacturaIntegracion.asmx?WSDL", "/WS_BuscarAutos.asmx?WSDL", "/WS_BuscarDatos.asmx?WSDL", "/WS_UsuarioExterno.asmx?WSDL", 205, "Soap", "http://autocarrent.runasp.net" },
                    { 206, "/WS_DisponibilidadAutos.asmx?WSDL", "/WS_PreReserva.asmx?WSDL", "/WS_ReservarAutos.asmx?WSDL", "/WS_FacturaIntegracion.asmx?WSDL", "/WS_BuscarAutos.asmx?WSDL", "/WS_BuscarDatos.asmx?WSDL", "/WS_UsuarioExterno.asmx?WSDL", 206, "Soap", "http://autocarrent.runasp.net" },
                    { 301, "", "", "", "", "", "", "", 301, "Soap", "https://worldagencysoa.runasp.net/PaquetesService.asmx?WSDL" },
                    { 302, "", "", "", "", "", "", "", 302, "Soap", "https://backend-cuenca.onrender.com/WS_Integracion.asmx?wsdl" },
                    { 303, "", "", "", "", "", "", "", 303, "Soap", "http://paquetesturisticosweb.runasp.net/Soap/PaquetesService.asmx?WSDL" },
                    { 401, "/BusDisponibilidadWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusFacturaWS.asmx?WSDL", "/BusBusquedaWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusUsuarioWS.asmx?WSDL", 401, "Soap", "http://cangrejitosfelicessoa.runasp.net" },
                    { 402, "/BusDisponibilidadWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusFacturaWS.asmx?WSDL", "/BusBusquedaWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusUsuarioWS.asmx?WSDL", 402, "Soap", "http://dragonrojosoap.runasp.net" },
                    { 403, "/BusDisponibilidadWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusFacturaWS.asmx?WSDL", "/BusBusquedaWS.asmx?WSDL", "/BusReservaWS.asmx?WSDL", "/BusUsuarioWS.asmx?WSDL", 403, "Soap", "http://cafesanjuansoap.runasp.net" },
                    { 404, "/BusDisponibilidadWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusFacturaWS.asmx?wsdl", "/BusBusquedaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusUsuarioWS.asmx?wsdl", 404, "Soap", "http://sanctum-soap.runasp.net" },
                    { 405, "/BusDisponibilidadWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusFacturaWS.asmx?wsdl", "/BusBusquedaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusUsuarioWS.asmx?wsdl", 405, "Soap", "http://saborandinosoa.runasp.net" },
                    { 406, "/BusDisponibilidadWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusFacturaWS.asmx?wsdl", "/BusBusquedaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusUsuarioWS.asmx?wsdl", 406, "Soap", "http://ingtegracion-bar-sinson-soap.runasp.net" },
                    { 407, "/BusDisponibilidadWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusFacturaWS.asmx?wsdl", "/BusBusquedaWS.asmx?wsdl", "/BusReservaWS.asmx?wsdl", "/BusUsuarioWS.asmx?wsdl", 407, "Soap", "http://7maresbacksoap.runasp.net" },
                    { 501, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 1, "Rest", "http://aerolineaintrest.runasp.net/api/v1/integracion/aerolinea" },
                    { 502, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 2, "Rest", "http://astrawings.runasp.net/api/v1/integracion/aerolinea" },
                    { 503, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 3, "Rest", "http://skaywardair.runasp.net/api/v1/integracion/aerolinea" },
                    { 504, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 4, "Rest", "http://skyandes.runasp.net/api/integracion/aerolinea" },
                    { 505, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 5, "Rest", "http://skyandes.runasp.net/api/integracion/aerolinea" },
                    { 601, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 101, "Rest", "http://hotelcampestrerest.runasp.net/api/v1/hoteles" },
                    { 602, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 102, "Rest", "http://restallpahousenyc.runasp.net/api/v1/hoteles" },
                    { 603, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 103, "Rest", "https://reca.azurewebsites.net/api/v1/hoteles" },
                    { 604, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 104, "Rest", "http://restbrisamar.runasp.net/api/v1/hoteles" },
                    { 605, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 105, "Rest", "http://restallpahousenyc.runasp.net/api/v1/hoteles" },
                    { 606, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 106, "Rest", "http://aureacuenrest.runasp.net/api/v1/hoteles" },
                    { 701, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 201, "Rest", "http://cuencautosinte.runasp.net/api/v1/integracion/autos" },
                    { 702, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 202, "Rest", "http://restintegracin.runasp.net/api/v1/integracion/autos" },
                    { 703, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 203, "Rest", "http://integracionrest.runasp.net/api/v1/integracion/autos" },
                    { 704, "/availability", "/hold", "/book", "/invoices", "/search", "/reserva", "/usuarios/externo", 204, "Rest", "http://autocarent.runasp.net/api/v1/integracion/autos" },
                    { 705, "/v2/integracion/autos/availability", "/v1/integracion/autos/hold", "/v1/integracion/autos/book", "/v1/integracion/autos/invoices", "/v2/integracion/autos/search", "/v2/prereserva/auto", "/v1/integracion/autos/usuarios/externo", 205, "Rest", "http://restinte.runasp.net/api" },
                    { 706, "/integracion/autos/availability", "/integracion/autos/hold", "/integracion/autos/book", "/integracion/autos/invoices", "/integracion/autos/search", "/prereserva/auto", "/integracion/autos/usuarios/externo", 206, "Rest", "http://urbandriveinterest.runasp.net/api/v1" },
                    { 801, "/availability", "/pre-reserva", "/reserva", "/invoices", "", "/{id}/reserva", "/usuarios/externo", 301, "Rest", "https://worldagencybk.runasp.net/api/v2/paquetes" },
                    { 802, "/availability", "/pre-reserva", "/reserva", "/invoices", "", "/{id}/reserva", "/usuarios/externo", 302, "Rest", "https://rest-back-xnjm.onrender.com/api/v2/paquetes" },
                    { 803, "/availability", "/pre-reserva", "/reserva", "/invoices", "", "/{id}/reserva", "/usuarios/externo", 303, "Rest", "http://paquetesturisticosweb-bk.runasp.net/api/v2/paquetes" },
                    { 901, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 401, "Rest", "http://cangrejitosfelices.runasp.net/api/v1/integracion/restaurantes" },
                    { 902, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 402, "Rest", "http://dragonrojobus.runasp.net/api/v1/integracion/restaurantes" },
                    { 903, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 403, "Rest", "http://cafesanjuan.runasp.net/api/integracion/restaurantes" },
                    { 904, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 404, "Rest", "http://sanctum.runasp.net/api/v1/integracion/restaurantes" },
                    { 905, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 405, "Rest", "http://saborandino.runasp.net/api/v1/integracion/restaurantes" },
                    { 906, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 406, "Rest", "http://ingtegracion-bar-sinson.runasp.net/api/v1/integracion/restaurantes" },
                    { 907, "/availability", "/hold", "/book", "/invoices", "/search", "/reservas", "/usuarios", 407, "Rest", "http://7maresback.runasp.net/api/v1/integracion/restaurantes" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarritoAerolineaPasajeros_CarritoAerolineaItemId",
                table: "CarritoAerolineaPasajeros",
                column: "CarritoAerolineaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoPaqueteTuristas_CarritoPaqueteItemId",
                table: "CarritoPaqueteTuristas",
                column: "CarritoPaqueteItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosAerolinea_ClienteId",
                table: "CarritosAerolinea",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosAerolinea_ServicioId",
                table: "CarritosAerolinea",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosAutos_ClienteId",
                table: "CarritosAutos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosAutos_ServicioId",
                table: "CarritosAutos",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosHabitaciones_ClienteId",
                table: "CarritosHabitaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosHabitaciones_ServicioId",
                table: "CarritosHabitaciones",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosMesas_ClienteId",
                table: "CarritosMesas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosMesas_ServicioId",
                table: "CarritosMesas",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosPaquetes_ClienteId",
                table: "CarritosPaquetes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritosPaquetes_ServicioId",
                table: "CarritosPaquetes",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CorreoElectronico",
                table: "Clientes",
                column: "CorreoElectronico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ClienteId",
                table: "Compras",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesServicio_ServicioId",
                table: "DetallesServicio",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ServicioId",
                table: "Reservas",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasCompra_ReservaId",
                table: "ReservasCompra",
                column: "ReservaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarritoAerolineaPasajeros");

            migrationBuilder.DropTable(
                name: "CarritoPaqueteTuristas");

            migrationBuilder.DropTable(
                name: "CarritosAutos");

            migrationBuilder.DropTable(
                name: "CarritosHabitaciones");

            migrationBuilder.DropTable(
                name: "CarritosMesas");

            migrationBuilder.DropTable(
                name: "DetallesServicio");

            migrationBuilder.DropTable(
                name: "ReservasCompra");

            migrationBuilder.DropTable(
                name: "CarritosAerolinea");

            migrationBuilder.DropTable(
                name: "CarritosPaquetes");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Servicios");
        }
    }
}
