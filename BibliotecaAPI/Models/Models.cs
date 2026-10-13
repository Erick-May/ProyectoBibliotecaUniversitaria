namespace BibliotecaAPI.Models;

public class Libro
{
    public int Id { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public int? AnioPublicacion { get; set; } = 2024;
    public string Categoria { get; set; } = string.Empty;
    public int Stock { get; set; } = 1;
}

public class Estudiante
{
    public string Carnet { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public string AnioLectivo { get; set; } = string.Empty;
}

public class Administrador
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Contrasena { get; set; }
    public string Rol { get; set; } = "Administrador";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

public class PrestamoDetalle
{
    public int IdPrestamo { get; set; }
    public string CarnetEstudiante { get; set; } = string.Empty;
    public string NombreEstudiante { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public int IdLibro { get; set; }
    public string TituloLibro { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public string Estado { get; set; } = "Prestado";
    public int DiasMora { get; set; }
}

public class CrearPrestamoDto
{
    public string CarnetEstudiante { get; set; } = string.Empty;
    public int IdLibro { get; set; }
    public int DiasPrestamo { get; set; } = 15;
}

public class LoginRequest
{
    public string Usuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class DashboardStats
{
    public int TotalLibros { get; set; }
    public int PrestamosActivos { get; set; }
    public int TotalEstudiantes { get; set; }
    public int TotalAdministradores { get; set; }
}
