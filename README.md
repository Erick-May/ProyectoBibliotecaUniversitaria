# Sistema de Control de Biblioteca - Universidad Nacional de Ingeniería (UNI)

Sistema de gestión bibliotecaria universitaria desarrollado con arquitectura cliente-servidor, frontend interactivo y backend REST en **ASP.NET Core (.NET 9)** con persistencia en **Microsoft SQL Server**.

---

## 🏛️ Características del Sistema

1. **Autenticación y Seguridad**:
   - Módulo de login institucional para administradores y bibliotecarios con control de roles (`SuperAdmin`, `Administrador`).
2. **Control de Inventario**:
   - Catálogo de libros físicos con gestión completa (Agregar, Ver, Modificar, Eliminar y Búsqueda en tiempo real).
3. **Gestión de Préstamos**:
   - Registro de préstamos vinculados al carnet del estudiante.
   - Descuento y reposición automática de ejemplares en stock.
   - Control de plazos y cálculo automático de días de mora.
4. **Padrón de Estudiantes**:
   - Registro y control de estudiantes por carrera y año lectivo.
5. **Administración de Operadores**:
   - Gestión y asignación de credenciales para nuevos administradores del sistema.
6. **Dashboard y Métricas**:
   - Resumen visual en tiempo real de ejemplares disponibles, préstamos activos y totales.

---

## 🛠️ Tecnologías Utilizadas

* **Frontend**: HTML5, CSS3 (Vanilla CSS, variables institucionales UNI), JavaScript moderno (Fetch API asíncrona).
* **Backend**: ASP.NET Core Web API (.NET 9.0), Minimal API y Controllers REST.
* **Base de Datos**: Microsoft SQL Server (`SistemaBibliotecaDB`), Transact-SQL, Vistas con `INNER JOIN` y procedimientos.
* **Acceso a Datos**: `Microsoft.Data.SqlClient` con transacciones atómicas.

---

## 🚀 Instrucciones de Instalación y Puesta en Marcha

### 1. Base de Datos (SQL Server)
1. Abrir **SQL Server Management Studio (SSMS)** o consola `sqlcmd`.
2. Ejecutar el script principal:
   ```sql
   ScriptCompletoBibliotecaUNI.sql
   ```
3. Ejecutar el script de vistas y consultas analíticas:
   ```sql
   Consultas_MontoyaBiblioteca.sql
   ```

### 2. Configuración de Conexión
Verificar en `BibliotecaAPI/appsettings.json` la cadena de conexión:
```json
"ConnectionStrings": {
  "SQLConexion": "Server=.;Database=SistemaBibliotecaDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;"
}
```

### 3. Levantar la Aplicación
Navegar al directorio de la API y ejecutar:
```bash
cd BibliotecaAPI
dotnet run
```
La aplicación web estará disponible en:
👉 **http://localhost:5087/Index.html**

### Credenciales de Acceso por Defecto:
* **Usuario**: `admin` | **Contraseña**: `1234`
* **Usuario**: `montoya` | **Contraseña**: `admin123`
