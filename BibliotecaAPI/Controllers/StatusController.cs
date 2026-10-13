using Microsoft.AspNetCore.Mvc;
using BibliotecaAPI.Data;

namespace BibliotecaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly DatabaseService _db;

    public StatusController(DatabaseService db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        var (ok, mensaje, stats) = await _db.ProbarConexionAsync();
        if (ok)
        {
            return Ok(new
            {
                status = "Online",
                conectado = true,
                mensaje,
                datos = stats,
                timestamp = DateTime.Now
            });
        }

        return StatusCode(500, new
        {
            status = "Error",
            conectado = false,
            mensaje,
            timestamp = DateTime.Now
        });
    }
}
