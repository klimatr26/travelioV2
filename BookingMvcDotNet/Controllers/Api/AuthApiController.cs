using BookingMvcDotNet.Models;
using BookingMvcDotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingMvcDotNet.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthApiController(IAuthService authService, ILogger<AuthApiController> logger) : ControllerBase
{
    /// <summary>
    /// Registro de nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });
            }

            var model = new RegisterViewModel
            {
                Email = request.Email?.Trim().ToLower() ?? "",
                Password = request.Password ?? "",
                Nombre = request.Nombre?.Trim() ?? "",
                Apellido = request.Apellido?.Trim() ?? "",
                TipoIdentificacion = request.TipoIdentificacion ?? "Cedula",
                DocumentoIdentidad = request.DocumentoIdentidad ?? "",
                FechaNacimiento = request.FechaNacimiento,
                Telefono = request.Telefono ?? "",
                Pais = request.Pais ?? "Ecuador"
            };

            var (exito, mensaje, cliente) = await authService.RegistrarAsync(model);

            if (!exito)
            {
                return BadRequest(new { success = false, message = mensaje });
            }

            logger.LogInformation("Usuario registrado: {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = mensaje,
                data = new
                {
                    id = cliente!.Id,
                    email = cliente.CorreoElectronico,
                    nombre = cliente.Nombre,
                    apellido = cliente.Apellido
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en registro");
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Login de usuario
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Email y contraseña son requeridos" });
            }

            // Credenciales de admin hardcoded para pruebas
            if (request.Email.ToLower() == "admin@admin.com" && request.Password == "Admin123!")
            {
                return Ok(new
                {
                    success = true,
                    message = "Login exitoso",
                    data = new
                    {
                        id = -1,
                        email = "admin@admin.com",
                        nombre = "Administrador",
                        apellido = "",
                        isAdmin = true
                    }
                });
            }

            var (exito, mensaje, cliente) = await authService.LoginAsync(request.Email.Trim().ToLower(), request.Password);

            if (!exito)
            {
                return Unauthorized(new { success = false, message = mensaje });
            }

            logger.LogInformation("Login exitoso: {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = mensaje,
                data = new
                {
                    id = cliente!.Id,
                    email = cliente.CorreoElectronico,
                    nombre = cliente.Nombre,
                    apellido = cliente.Apellido,
                    isAdmin = false
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en login");
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener perfil del usuario
    /// </summary>
    [HttpGet("profile/{id}")]
    public async Task<IActionResult> GetProfile(int id)
    {
        try
        {
            var cliente = await authService.ObtenerClientePorIdAsync(id);

            if (cliente == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = cliente.Id,
                    email = cliente.CorreoElectronico,
                    nombre = cliente.Nombre,
                    apellido = cliente.Apellido,
                    tipoIdentificacion = cliente.TipoIdentificacion,
                    documentoIdentidad = cliente.DocumentoIdentidad,
                    fechaNacimiento = cliente.FechaNacimiento.ToString("yyyy-MM-dd"),
                    telefono = cliente.Telefono,
                    pais = cliente.Pais
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo perfil");
            return StatusCode(500, new { success = false, message = "Error interno" });
        }
    }
}

public class RegisterRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public string? TipoIdentificacion { get; set; }
    public string? DocumentoIdentidad { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Pais { get; set; }
}

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
