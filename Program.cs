using dotenv.net;
using CompanyProductAPI.Data;
using CompanyProductAPI.Repositories;
using CompanyProductAPI.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ? Cargar variables de entorno desde .env
DotEnv.Load();
builder.Configuration.AddEnvironmentVariables();

// ? Agregar servicios de controladores y opciones JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ? Swagger (documentación de la API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Company Products API",
        Version = "v1",
        Description = "API for managing companies and their products",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });
});

// ? Registro de servicios personalizados

// Base de datos: fábrica de conexiones ADO.NET
builder.Services.AddScoped<DbConnectionFactory>();

// Repositorios (capa de acceso a datos)
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Servicios (lógica de negocio)
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// ? Middleware

if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("SWAGGER_ENABLED")?.ToLower() == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
