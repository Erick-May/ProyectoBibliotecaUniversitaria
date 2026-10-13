USE SistemaBibliotecaDB;
GO

-- ============================================================================
-- CONSULTAS Y VISTAS CON INNER JOIN & LEFT JOIN (REPORTES DEL SISTEMA)
-- ============================================================================

-- 1. CONSULTA PRINCIPAL DE PRÉSTAMOS (INNER JOIN)
-- Combina Préstamos, Estudiantes y Libros para mostrar los nombres en lugar de sólo IDs/Carnets
SELECT 
    p.Id AS [ID Préstamo],
    e.Carnet,
    e.NombreCompleto AS [Estudiante],
    e.Carrera,
    l.Titulo AS [Libro Prestado],
    l.Isbn,
    CONVERT(VARCHAR, p.FechaPrestamo, 103) AS [Fecha Préstamo],
    CONVERT(VARCHAR, p.FechaLimite, 103) AS [Fecha Límite],
    p.Estado,
    CASE 
        WHEN p.Estado = 'Prestado' AND GETDATE() > p.FechaLimite 
        THEN DATEDIFF(DAY, p.FechaLimite, GETDATE())
        ELSE 0 
    END AS [Días de Mora]
FROM dbo.Prestamos p
INNER JOIN dbo.Estudiantes e ON p.CarnetEstudiante = e.Carnet
INNER JOIN dbo.Libros l ON p.IdLibro = l.Id
ORDER BY p.Id DESC;
GO


-- 2. REPORTE DE ESTUDIANTES CON SUS PRÉSTAMOS ACTIVOS (INNER JOIN)
-- Muestra únicamente los estudiantes que tienen libros actualmente en su poder
SELECT 
    e.Carnet,
    e.NombreCompleto AS [Estudiante],
    e.Carrera,
    l.Titulo AS [Libro],
    p.FechaLimite AS [Fecha Entrega]
FROM dbo.Estudiantes e
INNER JOIN dbo.Prestamos p ON e.Carnet = p.CarnetEstudiante
INNER JOIN dbo.Libros l ON p.IdLibro = l.Id
WHERE p.Estado = 'Prestado';
GO


-- 3. HISTORIAL COMPLETO DE TODOS LOS ESTUDIANTES (LEFT JOIN)
-- Muestra A TODOS los estudiantes, hayan solicitado préstamos o no (si no tienen préstamos muestra NULL)
SELECT 
    e.Carnet,
    e.NombreCompleto AS [Estudiante],
    e.Carrera,
    e.AnioLectivo AS [Año],
    COUNT(p.Id) AS [Total Préstamos Solicitados]
FROM dbo.Estudiantes e
LEFT JOIN dbo.Prestamos p ON e.Carnet = p.CarnetEstudiante
GROUP BY e.Carnet, e.NombreCompleto, e.Carrera, e.AnioLectivo
ORDER BY [Total Préstamos Solicitados] DESC;
GO


-- 4. CONTROL DE INVENTARIO Y LIBROS MENOS/MÁS PRESTADOS (LEFT JOIN)
-- Incluye todos los libros del catálogo y cuenta cuántas veces se han prestado
SELECT 
    l.Id,
    l.Isbn,
    l.Titulo,
    l.Categoria,
    l.Stock AS [Disponibles],
    COUNT(p.Id) AS [Veces Prestado]
FROM dbo.Libros l
LEFT JOIN dbo.Prestamos p ON l.Id = p.IdLibro
GROUP BY l.Id, l.Isbn, l.Titulo, l.Categoria, l.Stock
ORDER BY [Veces Prestado] DESC;
GO


-- 5. REPORTE DE ALERTA DE MORA Y VENCIDOS (INNER JOIN CON FILTRO)
-- Lista únicamente los préstamos que tienen retraso para cobro de multa
SELECT 
    p.Id AS [ID Préstamo],
    e.Carnet,
    e.NombreCompleto AS [Estudiante],
    l.Titulo AS [Libro],
    DATEDIFF(DAY, p.FechaLimite, GETDATE()) AS [Días de Retraso]
FROM dbo.Prestamos p
INNER JOIN dbo.Estudiantes e ON p.CarnetEstudiante = e.Carnet
INNER JOIN dbo.Libros l ON p.IdLibro = l.Id
WHERE p.Estado = 'Prestado' AND GETDATE() > p.FechaLimite;
GO


-- 6. VISTA OFICIAL REUTILIZABLE (VIEW EN SQL SERVER)
-- Crea una vista para que puedas hacer SELECT * FROM v_ReportePrestamos en tu API
IF OBJECT_ID('dbo.v_ReportePrestamos', 'V') IS NOT NULL DROP VIEW dbo.v_ReportePrestamos;
GO
CREATE VIEW dbo.v_ReportePrestamos AS
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
INNER JOIN dbo.Libros l ON p.IdLibro = l.Id;
GO