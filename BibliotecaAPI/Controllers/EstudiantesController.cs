using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstudiantesController : ControllerBase
{
    private readonly DatabaseService _db;

    public EstudiantesController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetEstudiantes()
    {
        try
        {
            var estudiantes = await _db.ObtenerEstudiantesAsync();
            return Ok(estudiantes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener estudiantes: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearEstudiante([FromBody] Estudiante estudiante)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(estudiante.Carnet) || string.IsNullOrWhiteSpace(estudiante.NombreCompleto))
            {
                return BadRequest(new { mensaje = "El carnet y el nombre completo son obligatorios." });
            }

            var ok = await _db.CrearEstudianteAsync(estudiante);
            if (!ok) return BadRequest(new { mensaje = "No se pudo registrar el estudiante. Puede que el carnet ya exista." });

            return Ok(new { mensaje = "Estudiante registrado exitosamente.", estudiante });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al registrar estudiante: {ex.Message}" });
        }
    }

    [HttpDelete("{carnet}")]
    public async Task<IActionResult> EliminarEstudiante(string carnet)
    {
        try
        {
            var ok = await _db.EliminarEstudianteAsync(carnet);
            if (!ok) return NotFound(new { mensaje = "Estudiante no encontrado." });

            return Ok(new { mensaje = "Estudiante eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al eliminar estudiante: {ex.Message}" });
        }
    }
}
