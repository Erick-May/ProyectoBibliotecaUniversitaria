using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdministradoresController : ControllerBase
{
    private readonly DatabaseService _db;

    public AdministradoresController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdministradores()
    {
        try
        {
            var admins = await _db.ObtenerAdministradoresAsync();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener administradores: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearAdministrador([FromBody] Administrador admin)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(admin.Usuario) || string.IsNullOrWhiteSpace(admin.NombreCompleto))
            {
                return BadRequest(new { mensaje = "El usuario y el nombre completo son obligatorios." });
            }

            var (ok, mensaje) = await _db.CrearAdministradorAsync(admin);
            if (!ok) return BadRequest(new { mensaje });

            return Ok(new { mensaje, admin = new { admin.Usuario, admin.NombreCompleto, admin.Rol } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al crear administrador: {ex.Message}" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarAdministrador(int id)
    {
        try
        {
            var ok = await _db.EliminarAdministradorAsync(id);
            if (!ok) return NotFound(new { mensaje = "Administrador no encontrado o no se pudo eliminar." });

            return Ok(new { mensaje = "Administrador eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al eliminar administrador: {ex.Message}" });
        }
    }
}
