using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _db;

    public AuthController(DatabaseService db)
    {
        _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest(new { success = false, mensaje = "El usuario y la contraseña son requeridos." });
        }

        var admin = await _db.LoginAsync(request.Usuario, request.Contrasena);
        if (admin == null)
        {
            return Unauthorized(new { success = false, mensaje = "Credenciales incorrectas. Verifique su usuario o contraseña." });
        }

        return Ok(new
        {
            success = true,
            mensaje = "Inicio de sesión exitoso.",
            usuario = new
            {
                admin.Id,
                admin.Usuario,
                admin.NombreCompleto,
                admin.Rol,
                admin.FechaRegistro
            }
        });
    }
}
