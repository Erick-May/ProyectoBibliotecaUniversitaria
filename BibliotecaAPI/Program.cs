using BibliotecaAPI.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRAR SERVICIO DE BASE DE DATOS
builder.Services.AddSingleton<DatabaseService>();

// 2. CONFIGURAR CORS (Permitir peticiones desde cualquier origen o frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. REGISTRAR CONTROLADORES
builder.Services.AddControllers();

// 4. CONFIGURAR OPENAPI / SWAGGER
builder.Services.AddOpenApi();

var app = builder.Build();

// Servir la documentación OpenAPI en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 5. HABILITAR CORS
app.UseCors("PermitirTodo");

// 6. PERMITIR SERVIR Index.html DESDE EL DIRECTORIO PRINCIPAL
var rootDir = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, ".."));
if (Directory.Exists(rootDir))
{
    var fileProvider = new PhysicalFileProvider(rootDir);
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider,
        DefaultFileNames = new List<string> { "Index.html", "index.html" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider,
        RequestPath = ""
    });
}

app.UseAuthorization();
app.MapControllers();

// Redirección amigable si navegan a la raíz y no se cargó el index
app.MapGet("/api", () => Results.Ok(new
{
    sistema = "API Biblioteca UNI - Sistema de Control",
    version = "1.0",
    estado = "Activo",
    documentacion = "/openapi/v1.json",
    endpoints = new[] {
        "/api/status",
        "/api/auth/login",
        "/api/libros",
        "/api/prestamos",
        "/api/estudiantes",
        "/api/administradores",
        "/api/dashboard/stats"
    }
}));

app.Run();
