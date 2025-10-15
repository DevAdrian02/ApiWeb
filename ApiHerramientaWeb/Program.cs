using ApiHerramientaWeb.Controllers.Integraciones.Krill;
using ApiHerramientaWeb.Controllers.Integraciones.SmartOlt;
using ApiHerramientaWeb.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModeloPrincipal.Entity;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Servicios base
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// 2. Base de datos
builder.Services.AddDbContextPool<CVGEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// 4. Autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 5. Autorización
builder.Services.AddAuthorization();

// 6. Servicios de negocio
builder.Services.AddScoped<SmartOltController>();
builder.Services.AddScoped<KrillController>();
builder.Services.AddScoped<KrillService>();
builder.Services.AddScoped<SmartOltService>();
builder.Services.AddScoped<SmartOltCatvService>();
builder.Services.AddScoped<ModemService>();
builder.Services.AddScoped<ICatvService, SmartOltCatvService>();
builder.Services.AddScoped<ConfiguracionEmail>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<OrdenService>();
// 7. Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

builder.Services.AddScoped<ReciboController>();


var app = builder.Build();

// 🔧 Configuración del pipeline

// Si tu API vive bajo subruta
app.UsePathBase("/ApiHerramientaWeb");

app.UseRouting();

// ✅ CORS debe ir ANTES de auth
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", context =>
{
    context.Response.Redirect("/ApiHerramientaWeb/swagger");
    return Task.CompletedTask;
});

// Endpoint de prueba para CORS
app.MapGet("/testcors", () => Results.Ok("CORS funciona!"));

// Mapear controladores
app.MapControllers();

app.Run();
