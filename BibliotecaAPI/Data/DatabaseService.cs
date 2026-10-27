using System.Data;
using Microsoft.Data.SqlClient;
using BibliotecaAPI.Models;

namespace BibliotecaAPI.Data;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SQLConexion") 
            ?? "Server=.;Database=SistemaBibliotecaDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;";
    }

    private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    // ==========================================
    // 0. DIAGNÓSTICO / HEALTH CHECK
    // ==========================================
    public async Task<(bool ok, string mensaje, object? stats)> ProbarConexionAsync()
    {
        try
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    (SELECT COUNT(*) FROM dbo.Libros) AS Libros,
                    (SELECT COUNT(*) FROM dbo.Estudiantes) AS Estudiantes,
                    (SELECT COUNT(*) FROM dbo.Prestamos) AS Prestamos,
                    (SELECT COUNT(*) FROM dbo.Administradores) AS Admins;
            ";

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var stats = new
                {
                    totalLibros = reader.GetInt32(0),
                    totalEstudiantes = reader.GetInt32(1),
                    totalPrestamos = reader.GetInt32(2),
                    totalAdmins = reader.GetInt32(3),
                    servidor = conn.DataSource,
                    baseDeDatos = conn.Database
                };
                return (true, "Conexión a SistemaBibliotecaDB exitosa", stats);
            }

            return (true, "Conectado a SQL Server", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión a SQL Server: {ex.Message}", null);
        }
    }

    // ==========================================
    // 1. AUTENTICACIÓN / LOGIN
    // ==========================================
    public async Task<Administrador?> LoginAsync(string usuario, string contrasena)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, Usuario, NombreCompleto, Rol, FechaRegistro 
            FROM dbo.Administradores 
            WHERE LOWER(Usuario) = LOWER(@Usuario) AND Contrasena = @Contrasena;
        ";
        cmd.Parameters.AddWithValue("@Usuario", usuario.Trim());
        cmd.Parameters.AddWithValue("@Contrasena", contrasena.Trim());

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Administrador
            {
                Id = reader.GetInt32(0),
                Usuario = reader.GetString(1),
                NombreCompleto = reader.GetString(2),
                Rol = reader.GetString(3),
                FechaRegistro = reader.GetDateTime(4)
            };
        }
        return null;
    }

    // ==========================================
    // 2. GESTIÓN DE LIBROS (INVENTARIO)
    // ==========================================
    public async Task<List<Libro>> ObtenerLibrosAsync()
    {
        var lista = new List<Libro>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Isbn, Titulo, Autor, AnioPublicacion, Categoria, Stock FROM dbo.Libros ORDER BY Id DESC;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Libro
            {
                Id = reader.GetInt32(0),
                Isbn = reader.GetString(1),
                Titulo = reader.GetString(2),
                Autor = reader.GetString(3),
                AnioPublicacion = reader.IsDBNull(4) ? 2024 : reader.GetInt32(4),
                Categoria = reader.GetString(5),
                Stock = reader.GetInt32(6)
            });
        }
        return lista;
    }

    public async Task<Libro?> ObtenerLibroPorIdAsync(int id)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Isbn, Titulo, Autor, AnioPublicacion, Categoria, Stock FROM dbo.Libros WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Libro
            {
                Id = reader.GetInt32(0),
                Isbn = reader.GetString(1),
                Titulo = reader.GetString(2),
                Autor = reader.GetString(3),
                AnioPublicacion = reader.IsDBNull(4) ? 2024 : reader.GetInt32(4),
                Categoria = reader.GetString(5),
                Stock = reader.GetInt32(6)
            };
        }
        return null;
    }

    public async Task<int> CrearLibroAsync(Libro libro)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        // 1-Validación de Unicidad estricta para Libros (ISBN + Título)
        await using var cmdCheck = conn.CreateCommand();
        cmdCheck.CommandText = "SELECT COUNT(*) FROM dbo.Libros WHERE LOWER(Isbn) = LOWER(@Isbn) AND LOWER(Titulo) = LOWER(@Titulo);";
        cmdCheck.Parameters.AddWithValue("@Isbn", libro.Isbn.Trim());
        cmdCheck.Parameters.AddWithValue("@Titulo", libro.Titulo.Trim());
        var count = (int)(await cmdCheck.ExecuteScalarAsync() ?? 0);
        if (count > 0)
        {
            throw new InvalidOperationException("Ya existe un registro correspondiente con este ISBN y título en el sistema.");
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO dbo.Libros (Isbn, Titulo, Autor, AnioPublicacion, Categoria, Stock)
            VALUES (@Isbn, @Titulo, @Autor, @AnioPublicacion, @Categoria, @Stock);
            SELECT SCOPE_IDENTITY();
        ";
        cmd.Parameters.AddWithValue("@Isbn", libro.Isbn.Trim());
        cmd.Parameters.AddWithValue("@Titulo", libro.Titulo.Trim());
        cmd.Parameters.AddWithValue("@Autor", libro.Autor.Trim());
        cmd.Parameters.AddWithValue("@AnioPublicacion", libro.AnioPublicacion ?? 2024);
        cmd.Parameters.AddWithValue("@Categoria", libro.Categoria.Trim());
        cmd.Parameters.AddWithValue("@Stock", libro.Stock);

        var id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    public async Task<bool> ActualizarLibroAsync(Libro libro)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE dbo.Libros 
            SET Isbn = @Isbn, 
                Titulo = @Titulo, 
                Autor = @Autor, 
                AnioPublicacion = @AnioPublicacion, 
                Categoria = @Categoria, 
                Stock = @Stock
            WHERE Id = @Id;
        ";
        cmd.Parameters.AddWithValue("@Id", libro.Id);
        cmd.Parameters.AddWithValue("@Isbn", libro.Isbn.Trim());
        cmd.Parameters.AddWithValue("@Titulo", libro.Titulo.Trim());
        cmd.Parameters.AddWithValue("@Autor", libro.Autor.Trim());
        cmd.Parameters.AddWithValue("@AnioPublicacion", libro.AnioPublicacion ?? 2024);
        cmd.Parameters.AddWithValue("@Categoria", libro.Categoria.Trim());
        cmd.Parameters.AddWithValue("@Stock", libro.Stock);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> EliminarLibroAsync(int id)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Libros WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ==========================================
    // 3. GESTIÓN DE ESTUDIANTES
    // ==========================================
    public async Task<List<Estudiante>> ObtenerEstudiantesAsync()
    {
        var lista = new List<Estudiante>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Carnet, NombreCompleto, Carrera, AnioLectivo FROM dbo.Estudiantes ORDER BY Carnet ASC;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Estudiante
            {
                Carnet = reader.GetString(0),
                NombreCompleto = reader.GetString(1),
                Carrera = reader.GetString(2),
                AnioLectivo = reader.GetString(3)
            });
        }
        return lista;
    }

    public async Task<(bool ok, string mensaje)> CrearEstudianteAsync(Estudiante est)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        // 2-Validación de Unicidad para Estudiantes (Carné y NombreCompleto)
        await using var cmdCheck = conn.CreateCommand();
        cmdCheck.CommandText = @"
            SELECT 
                ISNULL(SUM(CASE WHEN LOWER(Carnet) = LOWER(@Carnet) THEN 1 ELSE 0 END), 0) AS CarnetExiste,
                ISNULL(SUM(CASE WHEN LOWER(NombreCompleto) = LOWER(@NombreCompleto) THEN 1 ELSE 0 END), 0) AS NombreExiste
            FROM dbo.Estudiantes;
        ";
        cmdCheck.Parameters.AddWithValue("@Carnet", est.Carnet.Trim());
        cmdCheck.Parameters.AddWithValue("@NombreCompleto", est.NombreCompleto.Trim());

        await using (var reader = await cmdCheck.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                int carnetExiste = reader.GetInt32(0);
                int nombreExiste = reader.GetInt32(1);

                if (carnetExiste > 0)
                    return (false, $"Ya existe un estudiante registrado con el carné '{est.Carnet}'.");
                if (nombreExiste > 0)
                    return (false, $"Ya existe un estudiante registrado con el nombre completo '{est.NombreCompleto}'.");
            }
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO dbo.Estudiantes (Carnet, NombreCompleto, Carrera, AnioLectivo)
            VALUES (@Carnet, @NombreCompleto, @Carrera, @AnioLectivo);
        ";
        cmd.Parameters.AddWithValue("@Carnet", est.Carnet.Trim());
        cmd.Parameters.AddWithValue("@NombreCompleto", est.NombreCompleto.Trim());
        cmd.Parameters.AddWithValue("@Carrera", est.Carrera.Trim());
        cmd.Parameters.AddWithValue("@AnioLectivo", est.AnioLectivo.Trim());

        var rows = await cmd.ExecuteNonQueryAsync();
        return (rows > 0, rows > 0 ? "Estudiante registrado exitosamente." : "No se pudo registrar el estudiante.");
    }

    public async Task<bool> EliminarEstudianteAsync(string carnet)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Estudiantes WHERE Carnet = @Carnet;";
        cmd.Parameters.AddWithValue("@Carnet", carnet.Trim());

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ==========================================
    // 4. GESTIÓN DE PRÉSTAMOS
    // ==========================================
    public async Task<List<PrestamoDetalle>> ObtenerPrestamosAsync()
    {
        var lista = new List<PrestamoDetalle>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        // Usamos la vista v_ReportePrestamos creada por las consultas del proyecto
        cmd.CommandText = @"
            SELECT 
                p.Id AS IdPrestamo,
                p.CarnetEstudiante,
                e.NombreCompleto AS NombreEstudiante,
                e.Carrera,
                p.IdLibro,
                l.Titulo AS TituloLibro,
                l.Isbn,
                p.FechaPrestamo,
                p.FechaLimite,
                p.Estado,
                CASE 
                    WHEN p.Estado = 'Prestado' AND GETDATE() > p.FechaLimite 
                    THEN DATEDIFF(DAY, p.FechaLimite, GETDATE())
                    ELSE 0 
                END AS DiasMora
            FROM dbo.Prestamos p
            INNER JOIN dbo.Estudiantes e ON p.CarnetEstudiante = e.Carnet
            INNER JOIN dbo.Libros l ON p.IdLibro = l.Id
            ORDER BY p.Id DESC;
        ";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new PrestamoDetalle
            {
                IdPrestamo = reader.GetInt32(0),
                CarnetEstudiante = reader.GetString(1),
                NombreEstudiante = reader.GetString(2),
                Carrera = reader.GetString(3),
                IdLibro = reader.GetInt32(4),
                TituloLibro = reader.GetString(5),
                Isbn = reader.GetString(6),
                FechaPrestamo = reader.GetDateTime(7),
                FechaLimite = reader.GetDateTime(8),
                Estado = reader.GetString(9),
                DiasMora = reader.GetInt32(10)
            });
        }
        return lista;
    }

    public async Task<(bool ok, string mensaje, int? idPrestamo)> RegistrarPrestamoAsync(CrearPrestamoDto dto)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var tran = conn.BeginTransaction();

        try
        {
            // 1. Validar que el estudiante exista
            await using var cmdEst = conn.CreateCommand();
            cmdEst.Transaction = tran;
            cmdEst.CommandText = "SELECT COUNT(*) FROM dbo.Estudiantes WHERE Carnet = @Carnet;";
            cmdEst.Parameters.AddWithValue("@Carnet", dto.CarnetEstudiante.Trim());
            var estCount = (int)(await cmdEst.ExecuteScalarAsync() ?? 0);
            if (estCount == 0)
            {
                await tran.RollbackAsync();
                return (false, $"El estudiante con carnet '{dto.CarnetEstudiante}' no está registrado.", null);
            }

            // 3-Control de Préstamos Activos (Evitar doble préstamo del mismo libro)
            await using var cmdActivo = conn.CreateCommand();
            cmdActivo.Transaction = tran;
            cmdActivo.CommandText = @"
                SELECT COUNT(*) FROM dbo.Prestamos 
                WHERE LOWER(CarnetEstudiante) = LOWER(@Carnet) 
                  AND IdLibro = @IdLibro 
                  AND Estado <> 'Devuelto';
            ";
            cmdActivo.Parameters.AddWithValue("@Carnet", dto.CarnetEstudiante.Trim());
            cmdActivo.Parameters.AddWithValue("@IdLibro", dto.IdLibro);
            var activoCount = (int)(await cmdActivo.ExecuteScalarAsync() ?? 0);
            if (activoCount > 0)
            {
                await tran.RollbackAsync();
                return (false, "El estudiante ya posee una copia de este libro en préstamo pendiente de devolución.", null);
            }

            // 2. Validar que el libro exista y tenga stock
            await using var cmdLib = conn.CreateCommand();
            cmdLib.Transaction = tran;
            cmdLib.CommandText = "SELECT Stock, Titulo FROM dbo.Libros WHERE Id = @IdLibro;";
            cmdLib.Parameters.AddWithValue("@IdLibro", dto.IdLibro);

            int stockActual = 0;
            string tituloLibro = "";
            await using (var reader = await cmdLib.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    stockActual = reader.GetInt32(0);
                    tituloLibro = reader.GetString(1);
                }
                else
                {
                    await tran.RollbackAsync();
                    return (false, "El libro seleccionado no existe.", null);
                }
            }

            if (stockActual <= 0)
            {
                await tran.RollbackAsync();
                return (false, $"El libro '{tituloLibro}' no tiene ejemplares disponibles en stock.", null);
            }

            // 3. Descontar 1 del stock
            await using var cmdUpdateStock = conn.CreateCommand();
            cmdUpdateStock.Transaction = tran;
            cmdUpdateStock.CommandText = "UPDATE dbo.Libros SET Stock = Stock - 1 WHERE Id = @IdLibro AND Stock > 0;";
            cmdUpdateStock.Parameters.AddWithValue("@IdLibro", dto.IdLibro);
            await cmdUpdateStock.ExecuteNonQueryAsync();

            // 4. Insertar el préstamo
            DateTime fechaHoy = DateTime.Now;
            DateTime fechaLimite = fechaHoy.AddDays(dto.DiasPrestamo > 0 ? dto.DiasPrestamo : 15);

            await using var cmdInsertPrestamo = conn.CreateCommand();
            cmdInsertPrestamo.Transaction = tran;
            cmdInsertPrestamo.CommandText = @"
                INSERT INTO dbo.Prestamos (CarnetEstudiante, IdLibro, FechaPrestamo, FechaLimite, Estado)
                VALUES (@Carnet, @IdLibro, @FechaPrestamo, @FechaLimite, 'Prestado');
                SELECT SCOPE_IDENTITY();
            ";
            cmdInsertPrestamo.Parameters.AddWithValue("@Carnet", dto.CarnetEstudiante.Trim());
            cmdInsertPrestamo.Parameters.AddWithValue("@IdLibro", dto.IdLibro);
            cmdInsertPrestamo.Parameters.AddWithValue("@FechaPrestamo", fechaHoy);
            cmdInsertPrestamo.Parameters.AddWithValue("@FechaLimite", fechaLimite);

            var nuevoId = Convert.ToInt32(await cmdInsertPrestamo.ExecuteScalarAsync());

            await tran.CommitAsync();
            return (true, "Préstamo registrado exitosamente.", nuevoId);
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            return (false, $"Error al registrar el préstamo: {ex.Message}", null);
        }
    }

    public async Task<(bool ok, string mensaje)> DevolverPrestamoAsync(int idPrestamo)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var tran = conn.BeginTransaction();

        try
        {
            // 1. Obtener datos del préstamo
            await using var cmdGet = conn.CreateCommand();
            cmdGet.Transaction = tran;
            cmdGet.CommandText = "SELECT IdLibro, Estado FROM dbo.Prestamos WHERE Id = @Id;";
            cmdGet.Parameters.AddWithValue("@Id", idPrestamo);

            int idLibro = 0;
            string estado = "";
            await using (var reader = await cmdGet.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    idLibro = reader.GetInt32(0);
                    estado = reader.GetString(1);
                }
                else
                {
                    await tran.RollbackAsync();
                    return (false, "El préstamo especificado no existe.");
                }
            }

            if (estado == "Devuelto")
            {
                await tran.RollbackAsync();
                return (false, "Este libro ya ha sido devuelto anteriormente.");
            }

            // 2. Marcar como devuelto
            await using var cmdUpdatePrestamo = conn.CreateCommand();
            cmdUpdatePrestamo.Transaction = tran;
            cmdUpdatePrestamo.CommandText = "UPDATE dbo.Prestamos SET Estado = 'Devuelto' WHERE Id = @Id;";
            cmdUpdatePrestamo.Parameters.AddWithValue("@Id", idPrestamo);
            await cmdUpdatePrestamo.ExecuteNonQueryAsync();

            // 3. Devolver 1 unidad al stock del libro
            await using var cmdUpdateStock = conn.CreateCommand();
            cmdUpdateStock.Transaction = tran;
            cmdUpdateStock.CommandText = "UPDATE dbo.Libros SET Stock = Stock + 1 WHERE Id = @IdLibro;";
            cmdUpdateStock.Parameters.AddWithValue("@IdLibro", idLibro);
            await cmdUpdateStock.ExecuteNonQueryAsync();

            await tran.CommitAsync();
            return (true, "Devolución registrada exitosamente y stock reincorporado.");
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            return (false, $"Error al devolver libro: {ex.Message}");
        }
    }

    // ==========================================
    // 5. GESTIÓN DE ADMINISTRADORES
    // ==========================================
    public async Task<List<Administrador>> ObtenerAdministradoresAsync()
    {
        var lista = new List<Administrador>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Usuario, NombreCompleto, Rol, FechaRegistro FROM dbo.Administradores ORDER BY Id DESC;";

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new Administrador
            {
                Id = reader.GetInt32(0),
                Usuario = reader.GetString(1),
                NombreCompleto = reader.GetString(2),
                Rol = reader.GetString(3),
                FechaRegistro = reader.GetDateTime(4)
            });
        }
        return lista;
    }

    public async Task<(bool ok, string mensaje)> CrearAdministradorAsync(Administrador admin)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        // 5-Validar usuario duplicado
        await using var cmdCheck = conn.CreateCommand();
        cmdCheck.CommandText = "SELECT COUNT(*) FROM dbo.Administradores WHERE LOWER(Usuario) = LOWER(@Usuario);";
        cmdCheck.Parameters.AddWithValue("@Usuario", admin.Usuario.Trim());
        var count = (int)(await cmdCheck.ExecuteScalarAsync() ?? 0);
        if (count > 0)
        {
            return (false, $"El usuario '{admin.Usuario}' ya existe en el sistema.");
        }

        // 7-Validación de Límite de Cuentas por Rol
        var rol = string.IsNullOrWhiteSpace(admin.Rol) ? "Administrador" : admin.Rol.Trim();
        if (rol == "SuperAdmin")
        {
            await using var cmdSuper = conn.CreateCommand();
            cmdSuper.CommandText = "SELECT COUNT(*) FROM dbo.Administradores WHERE Rol = 'SuperAdmin';";
            var totalSuper = (int)(await cmdSuper.ExecuteScalarAsync() ?? 0);
            if (totalSuper >= 7)
            {
                return (false, "Aviso de restricción: Se ha alcanzado el límite estricto de un máximo de 7 cuentas permitidas para SuperAdmin.");
            }
        }
        else
        {
            await using var cmdAdmin = conn.CreateCommand();
            cmdAdmin.CommandText = "SELECT COUNT(*) FROM dbo.Administradores WHERE Rol <> 'SuperAdmin';";
            var totalAdmin = (int)(await cmdAdmin.ExecuteScalarAsync() ?? 0);
            if (totalAdmin >= 10)
            {
                return (false, "Aviso de restricción: Se ha alcanzado el límite máximo de 10 cuentas operativas permitidas para Administrador de Biblioteca.");
            }
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO dbo.Administradores (Usuario, NombreCompleto, Contrasena, Rol, FechaRegistro)
            VALUES (@Usuario, @NombreCompleto, @Contrasena, @Rol, GETDATE());
        ";
        cmd.Parameters.AddWithValue("@Usuario", admin.Usuario.Trim());
        cmd.Parameters.AddWithValue("@NombreCompleto", admin.NombreCompleto.Trim());
        cmd.Parameters.AddWithValue("@Contrasena", admin.Contrasena?.Trim() ?? "1234");
        cmd.Parameters.AddWithValue("@Rol", rol);

        await cmd.ExecuteNonQueryAsync();
        return (true, "Administrador registrado exitosamente.");
    }

    public async Task<bool> EliminarAdministradorAsync(int id)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Administradores WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    // ==========================================
    // 6. DASHBOARD / MÉTRICAS
    // ==========================================
    public async Task<DashboardStats> ObtenerDashboardStatsAsync()
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                ISNULL((SELECT SUM(Stock) FROM dbo.Libros), 0) AS TotalEjemplares,
                (SELECT COUNT(*) FROM dbo.Prestamos WHERE Estado = 'Prestado') AS PrestamosActivos,
                (SELECT COUNT(*) FROM dbo.Estudiantes) AS TotalEstudiantes,
                (SELECT COUNT(*) FROM dbo.Administradores) AS TotalAdmins;
        ";

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DashboardStats
            {
                TotalLibros = reader.GetInt32(0),
                PrestamosActivos = reader.GetInt32(1),
                TotalEstudiantes = reader.GetInt32(2),
                TotalAdministradores = reader.GetInt32(3)
            };
        }

        return new DashboardStats();
    }
}
