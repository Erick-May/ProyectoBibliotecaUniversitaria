using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly DatabaseService _db;

    public PrestamosController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetPrestamos()
    {
        try
        {
            var prestamos = await _db.ObtenerPrestamosAsync();
            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener préstamos: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarPrestamo([FromBody] CrearPrestamoDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CarnetEstudiante) || dto.IdLibro <= 0)
            {
                return BadRequest(new { mensaje = "El carnet del estudiante y el ID del libro son requeridos." });
            }

            var (ok, mensaje, idPrestamo) = await _db.RegistrarPrestamoAsync(dto);
            if (!ok)
            {
                return BadRequest(new { mensaje });
            }

            return Ok(new { mensaje, idPrestamo });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al registrar préstamo: {ex.Message}" });
        }
    }

    [HttpPut("{id:int}/devolver")]
    public async Task<IActionResult> DevolverPrestamo(int id)
    {
        try
        {
            var (ok, mensaje) = await _db.DevolverPrestamoAsync(id);
            if (!ok)
            {
                return BadRequest(new { mensaje });
            }

            return Ok(new { mensaje });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al devolver libro: {ex.Message}" });
        }
    }
}
