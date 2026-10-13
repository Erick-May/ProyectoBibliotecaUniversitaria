-- ============================================================================
-- SCRIPT DE CREACIÓN Y POBLADO DE BASE DE DATOS PARA EL SISTEMA BIBLIOTECA UNI
-- Servidor objetivo: MONTOYA\SQL2019
-- Base de Datos: SistemaBibliotecaDB
-- Autor: Ervin Enmanuel Molina Montoya (Montoya)
-- ============================================================================

USE master;
GO

-- 1. CREACIÓN DE LA BASE DE DATOS
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SistemaBibliotecaDB')
BEGIN
    CREATE DATABASE SistemaBibliotecaDB;
    PRINT '✅ Base de datos SistemaBibliotecaDB creada correctamente.';
END
GO

USE SistemaBibliotecaDB;
GO

-- 2. ELIMINACIÓN PREVIA DE TABLAS (Para reconstrucción limpia)
IF OBJECT_ID('dbo.Prestamos', 'U') IS NOT NULL DROP TABLE dbo.Prestamos;
IF OBJECT_ID('dbo.Libros', 'U') IS NOT NULL DROP TABLE dbo.Libros;
IF OBJECT_ID('dbo.Estudiantes', 'U') IS NOT NULL DROP TABLE dbo.Estudiantes;
IF OBJECT_ID('dbo.Administradores', 'U') IS NOT NULL DROP TABLE dbo.Administradores;
GO

-- 3. CREACIÓN DE ESTRUCTURA DE TABLAS (DDL)

-- TABLA 1: Libros (Inventario bibliográfico)
CREATE TABLE dbo.Libros (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Isbn VARCHAR(20) NOT NULL UNIQUE,
    Titulo VARCHAR(250) NOT NULL,
    Autor VARCHAR(150) NOT NULL,
    AnioPublicacion INT NULL DEFAULT 2024,
    Categoria VARCHAR(100) NOT NULL,
    Stock INT NOT NULL DEFAULT 1
);

-- TABLA 2: Estudiantes (Padrón de usuarios de la UNI)
CREATE TABLE dbo.Estudiantes (
    Carnet VARCHAR(20) PRIMARY KEY,
    NombreCompleto VARCHAR(150) NOT NULL,
    Carrera VARCHAR(100) NOT NULL,
    AnioLectivo VARCHAR(10) NOT NULL
);

-- TABLA 3: Administradores (Credenciales de operadores)
CREATE TABLE dbo.Administradores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Usuario VARCHAR(50) NOT NULL UNIQUE,
    NombreCompleto VARCHAR(150) NOT NULL,
    Contrasena VARCHAR(100) NOT NULL,
    Rol VARCHAR(30) NOT NULL DEFAULT 'Administrador',
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);

-- TABLA 4: Prestamos (Control de préstamos y mora)
CREATE TABLE dbo.Prestamos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CarnetEstudiante VARCHAR(20) NOT NULL,
    IdLibro INT NOT NULL,
    FechaPrestamo DATETIME NOT NULL DEFAULT GETDATE(),
    FechaLimite DATETIME NOT NULL,
    Estado VARCHAR(20) NOT NULL DEFAULT 'Prestado', -- 'Prestado', 'Devuelto'
    CONSTRAINT FK_Prestamos_Estudiantes FOREIGN KEY (CarnetEstudiante) 
        REFERENCES dbo.Estudiantes(Carnet) ON DELETE CASCADE,
    CONSTRAINT FK_Prestamos_Libros FOREIGN KEY (IdLibro) 
        REFERENCES dbo.Libros(Id) ON DELETE CASCADE
);
GO

-- 4. POBLADO Y INSERCIÓN MASIVA DE DATOS (DML)

-- A. INSERTAR 40 LIBROS DE INGENIERÍA
INSERT INTO dbo.Libros (Isbn, Titulo, Autor, AnioPublicacion, Categoria, Stock) VALUES
('978-0073398204', 'Cálculo de una Variable', 'James Stewart', 2018, 'Matemáticas', 5),
('978-6071507709', 'Álgebra Lineal y sus Aplicaciones', 'David C. Lay', 2016, 'Matemáticas', 4),
('978-0133915426', 'Física para Ciencias e Ingeniería', 'Douglas Giancoli', 2014, 'Física', 6),
('978-6071503152', 'Química La Ciencia Central', 'Theodore L. Brown', 2014, 'Química', 3),
('978-0134093413', 'Mecánica Vectorial: Estática', 'Beer & Johnston', 2017, 'Mecánica', 5),
('978-0134319650', 'Mecánica Vectorial: Dinámica', 'Beer & Johnston', 2017, 'Mecánica', 4),
('978-0133918922', 'Ingeniería de Materiales', 'Russell C. Hibbeler', 2016, 'Materiales', 3),
('978-6071505460', 'Fundamentos de Circuitos Eléctricos', 'Charles Sadiku', 2013, 'Electricidad', 7),
('978-0132145770', 'Sistemas de Control para Ingeniería', 'Norman Nise', 2011, 'Automática', 2),
('978-0133594140', 'Ecuaciones Diferenciales', 'Dennis G. Zill', 2015, 'Matemáticas', 5),
('978-0134670959', 'Estructuras de Datos en C++', 'Luis Joyanes Aguilar', 2015, 'Sistemas', 4),
('978-0134444321', 'Diseño de Sistemas Operativos', 'Andrew S. Tanenbaum', 2015, 'Sistemas', 3),
('978-0133591620', 'Fundamentos de Bases de Datos', 'Silberschatz & Korth', 2014, 'Sistemas', 6),
('978-0132145336', 'Ingeniería de Software', 'Ian Sommerville', 2011, 'Sistemas', 4),
('978-0133918800', 'Termodinámica para Ingenieros', 'Yunus A. Cengel', 2015, 'Termodinámica', 5),
('978-0073398181', 'Mecánica de Fluidos', 'Frank M. White', 2011, 'Fluidos', 3),
('978-6071505484', 'Transferencia de Calor y Masa', 'Yunus A. Cengel', 2011, 'Termodinámica', 4),
('978-0133918923', 'Ingeniería Económica', 'Leland Blank & Tarquin', 2012, 'Economía', 5),
('978-0133591651', 'Diseño Digital y Arquitectura', 'M. Morris Mano', 2013, 'Electrónica', 3),
('978-6071507754', 'Investigación de Operaciones', 'Hamdy A. Taha', 2012, 'Optimización', 4),
('978-0134670980', 'Principios de Electrónica', 'Albert Malvino', 2007, 'Electrónica', 6),
('978-0133918915', 'Redes de Computadoras', 'Andrew S. Tanenbaum', 2012, 'Teleinformática', 5),
('978-6071505491', 'Hidráulica de Canales Abiertos', 'Ven Te Chow', 2004, 'Civil', 2),
('978-0133591682', 'Diseño de Estructuras de Concreto', 'Arthur H. Nilson', 2010, 'Civil', 4),
('978-0133918953', 'Mecánica de Suelos', 'Braja M. Das', 2013, 'Geotecnia', 3),
('978-0134671000', 'Topografía General', 'Paul R. Wolf', 2012, 'Civil', 5),
('978-6071507808', 'Inteligencia Artificial Modern Enfoque', 'Stuart Russell', 2011, 'Sistemas', 2),
('978-0133918984', 'Estadística para Ingenieros', 'Walpole & Myers', 2012, 'Matemáticas', 7),
('978-0133591705', 'Métodos Numéricos para Ingenieros', 'Steven C. Chapra', 2011, 'Matemáticas', 4),
('978-6071507907', 'Gestión de la Calidad Total', 'James R. Evans', 2014, 'Industrial', 3),
('978-0131495081', 'Compiladores: Principios y Herramientas', 'Aho, Sethi & Ullman', 2008, 'Sistemas', 3),
('978-0201633610', 'Patrones de Diseño (Design Patterns)', 'Erich Gamma et al.', 2002, 'Sistemas', 5),
('978-0132350884', 'Clean Code: Código Limpio', 'Robert C. Martin', 2008, 'Sistemas', 6),
('978-0321125217', 'Domain-Driven Design', 'Eric Evans', 2003, 'Sistemas', 2),
('978-0134494166', 'Clean Architecture', 'Robert C. Martin', 2017, 'Sistemas', 4),
('978-0596007126', 'Head First Design Patterns', 'Eric Freeman', 2004, 'Sistemas', 4),
('978-0321146533', 'Test-Driven Development', 'Kent Beck', 2002, 'Sistemas', 3),
('978-0137081073', 'The Clean Coder', 'Robert C. Martin', 2011, 'Sistemas', 5),
('978-0201616224', 'The Pragmatic Programmer', 'Andrew Hunt', 1999, 'Sistemas', 4),
('978-0135957059', 'Refactoring', 'Martin Fowler', 2018, 'Sistemas', 3);

-- B. INSERTAR 40 ESTUDIANTES DE LA UNI
INSERT INTO dbo.Estudiantes (Carnet, NombreCompleto, Carrera, AnioLectivo) VALUES
('2023-0012U', 'Ervin Enmanuel Molina Montoya', 'Ing. de Sistemas', '3ro'),
('2023-0045U', 'Carlos Alberto Ruiz', 'Ing. Industrial', '2do'),
('2022-0189U', 'María José Gutiérrez', 'Ing. Civil', '4to'),
('2024-0003U', 'Ana Sofía Brenes', 'Ing. Electrónica', '1ro'),
('2023-0015U', 'Jorge Luis Blandón', 'Ing. de Sistemas', '3ro'),
('2022-0088U', 'Lucía Fernanda Morales', 'Ing. Mecánica', '4to'),
('2021-0201U', 'Kevin Alexander Sevilla', 'Ing. Agrícola', '5to'),
('2023-0112U', 'Valeria Isabel Castillo', 'Ing. Industrial', '3ro'),
('2024-0040U', 'Gabriel Antonio López', 'Ing. de Sistemas', '1ro'),
('2022-0310U', 'Andrea Carolina Mendoza', 'Ing. Química', '4to'),
('2023-0155U', 'Roberto José Solís', 'Ing. Civil', '3ro'),
('2021-0099U', 'Daniela María Rocha', 'Ing. Electrónica', '5to'),
('2024-0120U', 'Fernando José Mairena', 'Ing. Mecánica', '1ro'),
('2023-0200U', 'Camila Andrea Espinoza', 'Ing. Industrial', '2do'),
('2022-0144U', 'Socrates Ramón Obando', 'Ing. de Sistemas', '4to'),
('2024-0089U', 'Adriana Lucía Fonseca', 'Ing. Química', '1ro'),
('2023-0301U', 'Manuel Salvador Umaña', 'Ing. Civil', '3ro'),
('2021-0177U', 'Patricia Elena Vivas', 'Ing. Industrial', '5to'),
('2022-0222U', 'David Emmanuel Jarquín', 'Ing. Electrónica', '4to'),
('2024-0150U', 'Nohelia Beatríz Baltodano', 'Ing. de Sistemas', '1ro'),
('2023-0410U', 'Christian José Pavón', 'Ing. Mecánica', '2do'),
('2022-0333U', 'Gema María Talavera', 'Ing. Civil', '4to'),
('2021-0250U', 'Alejandro José Tinoco', 'Ing. Agrícola', '5to'),
('2024-0210U', 'Claudia Stephania Tercero', 'Ing. Industrial', '1ro'),
('2023-0500U', 'Marcos Aurelio Centeno', 'Ing. de Sistemas', '3ro'),
('2022-0404U', 'Diana Marcela Ugarte', 'Ing. Química', '4to'),
('2024-0300U', 'Héctor José Reyes', 'Ing. Civil', '1ro'),
('2021-0311U', 'Lester Antonio Amador', 'Ing. Electrónica', '5to'),
('2023-0612U', 'Guissel del Carmen Aráuz', 'Ing. Industrial', '2do'),
('2022-0515U', 'Iván Francisco Zeledón', 'Ing. Mecánica', '4to'),
('2024-0401U', 'Francis Janina Torrez', 'Ing. de Sistemas', '1ro'),
('2023-0711U', 'Wilber José Laguna', 'Ing. Civil', '3ro'),
('2021-0422U', 'Tania Valeska Membreño', 'Ing. Agrícola', '5to'),
('2022-0620U', 'Erick Javier Orozco', 'Ing. Electrónica', '4to'),
('2024-0510U', 'Katherine Vanessa Montalván', 'Ing. Química', '1ro'),
('2023-0820U', 'Bismark Antonio Estrada', 'Ing. Industrial', '2do'),
('2022-0730U', 'Scarleth María Vallecillo', 'Ing. de Sistemas', '4to'),
('2021-0533U', 'Reynaldo José Cárcamo', 'Ing. Civil', '5to'),
('2024-0615U', 'Genesis Nazareth Pichardo', 'Ing. Mecánica', '1ro'),
('2023-0930U', 'Byron Francisco Huete', 'Ing. Electrónica', '3ro');

-- C. INSERTAR 10 ADMINISTRADORES
INSERT INTO dbo.Administradores (Usuario, NombreCompleto, Contrasena, Rol, FechaRegistro) VALUES
('admin', 'Ing. Julio Pérez', '1234', 'SuperAdmin', '2026-10-10'),
('montoya', 'Ervin Molina Montoya', 'admin123', 'SuperAdmin', '2026-10-11'),
('mgutierrez', 'Lic. Marta Gutiérrez', '1234', 'Administrador', '2026-01-15'),
('cflores', 'Ing. Carlos Flores', '1234', 'Administrador', '2026-02-20'),
('rhernandez', 'Lic. Rosa Hernández', '1234', 'Administrador', '2026-03-05'),
('jromero', 'Ing. Juan Romero', '1234', 'Administrador', '2026-04-12'),
('aespinoza', 'Lic. Ana Espinoza', '1234', 'Administrador', '2026-05-18'),
('mcastillo', 'Ing. Mario Castillo', '1234', 'Administrador', '2026-06-22'),
('vsilva', 'Lic. Veronica Silva', '1234', 'Administrador', '2026-07-30'),
('dvalle', 'Ing. Daniel Valle', '1234', 'Administrador', '2026-08-14');

-- D. INSERTAR 40 PRÉSTAMOS
INSERT INTO dbo.Prestamos (CarnetEstudiante, IdLibro, FechaPrestamo, FechaLimite, Estado) VALUES
('2023-0012U', 1, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0045U', 2, '2026-10-01', '2026-10-08', 'Prestado'),
('2022-0189U', 3, '2026-10-01', '2026-10-18', 'Devuelto'),
('2024-0003U', 4, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0015U', 5, '2026-10-01', '2026-10-08', 'Prestado'),
('2022-0088U', 6, '2026-10-01', '2026-10-18', 'Devuelto'),
('2021-0201U', 7, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0112U', 8, '2026-10-01', '2026-10-08', 'Prestado'),
('2024-0040U', 9, '2026-10-01', '2026-10-18', 'Devuelto'),
('2022-0310U', 10, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0155U', 11, '2026-10-01', '2026-10-08', 'Prestado'),
('2021-0099U', 12, '2026-10-01', '2026-10-18', 'Devuelto'),
('2024-0120U', 13, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0200U', 14, '2026-10-01', '2026-10-08', 'Prestado'),
('2022-0144U', 15, '2026-10-01', '2026-10-18', 'Devuelto'),
('2024-0089U', 16, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0301U', 17, '2026-10-01', '2026-10-08', 'Prestado'),
('2021-0177U', 18, '2026-10-01', '2026-10-18', 'Devuelto'),
('2022-0222U', 19, '2026-10-01', '2026-10-18', 'Prestado'),
('2024-0150U', 20, '2026-10-01', '2026-10-08', 'Prestado'),
('2023-0410U', 21, '2026-10-01', '2026-10-18', 'Devuelto'),
('2022-0333U', 22, '2026-10-01', '2026-10-18', 'Prestado'),
('2021-0250U', 23, '2026-10-01', '2026-10-08', 'Prestado'),
('2024-0210U', 24, '2026-10-01', '2026-10-18', 'Devuelto'),
('2023-0500U', 25, '2026-10-01', '2026-10-18', 'Prestado'),
('2022-0404U', 26, '2026-10-01', '2026-10-08', 'Prestado'),
('2024-0300U', 27, '2026-10-01', '2026-10-18', 'Devuelto'),
('2021-0311U', 28, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0612U', 29, '2026-10-01', '2026-10-08', 'Prestado'),
('2022-0515U', 30, '2026-10-01', '2026-10-18', 'Devuelto'),
('2024-0401U', 31, '2026-10-01', '2026-10-18', 'Prestado'),
('2023-0711U', 32, '2026-10-01', '2026-10-08', 'Prestado'),
('2021-0422U', 33, '2026-10-01', '2026-10-18', 'Devuelto'),
('2022-0620U', 34, '2026-10-01', '2026-10-18', 'Prestado'),
('2024-0510U', 35, '2026-10-01', '2026-10-08', 'Prestado'),
('2023-0820U', 36, '2026-10-01', '2026-10-18', 'Devuelto'),
('2022-0730U', 37, '2026-10-01', '2026-10-18', 'Prestado'),
('2021-0533U', 38, '2026-10-01', '2026-10-08', 'Prestado'),
('2024-0615U', 39, '2026-10-01', '2026-10-18', 'Devuelto'),
('2023-0930U', 40, '2026-10-01', '2026-10-18', 'Prestado');
GO

-- 5. VERIFICACIÓN FINAL
PRINT '==========================================================';
PRINT ' RESUMEN DE REGISTROS INSERTADOS EN SistemaBibliotecaDB  ';
PRINT '==========================================================';
SELECT 'Libros' AS Tabla, COUNT(*) AS TotalRegistros FROM dbo.Libros
UNION ALL
SELECT 'Estudiantes', COUNT(*) FROM dbo.Estudiantes
UNION ALL
SELECT 'Administradores', COUNT(*) FROM dbo.Administradores
UNION ALL
SELECT 'Prestamos', COUNT(*) FROM dbo.Prestamos;
GO