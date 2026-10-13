using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DatabaseService _db;

    public DashboardController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = await _db.ObtenerDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener estadísticas: {ex.Message}" });
        }
    }
}
