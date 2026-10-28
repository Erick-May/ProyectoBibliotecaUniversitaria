using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibrosController : ControllerBase
{
    private readonly DatabaseService _db;

    public LibrosController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetLibros()
    {
        try
        {
            var libros = await _db.ObtenerLibrosAsync();
            return Ok(libros);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener libros: {ex.Message}" });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLibro(int id)
    {
        try
        {
            var libro = await _db.ObtenerLibroPorIdAsync(id);
            if (libro == null) return NotFound(new { mensaje = "Libro no encontrado." });
            return Ok(libro);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener libro: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CrearLibro([FromBody] Libro libro)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(libro.Titulo) || string.IsNullOrWhiteSpace(libro.Isbn))
            {
                return BadRequest(new { mensaje = "El título y el ISBN son obligatorios." });
            }

            var nuevoId = await _db.CrearLibroAsync(libro);
            libro.Id = nuevoId;
            return CreatedAtAction(nameof(GetLibro), new { id = nuevoId }, libro);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al crear libro: {ex.Message}" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarLibro(int id, [FromBody] Libro libro)
    {
        try
        {
            libro.Id = id;
            var actualizado = await _db.ActualizarLibroAsync(libro);
            if (!actualizado) return NotFound(new { mensaje = "El libro especificado no existe para actualizar." });

            return Ok(new { mensaje = "Libro actualizado correctamente.", libro });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al actualizar libro: {ex.Message}" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarLibro(int id)
    {
        try
        {
            var eliminado = await _db.EliminarLibroAsync(id);
            if (!eliminado) return NotFound(new { mensaje = "El libro no existe o ya fue eliminado." });

            return Ok(new { mensaje = "Libro eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al eliminar libro: {ex.Message}" });
        }
    }
}
