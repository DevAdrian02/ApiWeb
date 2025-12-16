using ApiHerramientaWeb.Controllers.Integraciones.Krill;
using ApiHerramientaWeb.Controllers.Integraciones.SmartOlt;
using ApiHerramientaWeb.Controllers.MoviTv;
using ApiHerramientaWeb.Hubs;
using ApiHerramientaWeb.Modelos;
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
string[] allowedOrigins = new string[]
{
    "http://localhost:3000",               // Desarrollo
    "https://wservices.casavision.com" ,   // Producción
    "https://herramientaweb3.vercel.app"    // ProducciónF
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // necesario para JWT/cookies
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
builder.Services.AddScoped<MoviTvService>();
builder.Services.AddScoped<MoviTVController>();

builder.Services.AddScoped<IDesactivarDispositivoService, DesactivarDispositivoService>();
builder.Services.AddScoped<Utils>(); // 👈 Agregá esto


// 7. Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

builder.Services.AddScoped<ReciboController>();

builder.Services.AddSignalR();


var app = builder.Build();

// 🔧 Configuración del pipeline
app.UseRouting();

// CORS antes de Auth
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

// Primero controladores
app.MapControllers();

// Luego Hubs (✔ esto evita el 404 intermitente)
app.MapHub<UbicacionHub>("/hubs/ubicacion");

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Redirección segura
app.MapGet("/", context =>
{
    context.Response.Redirect("/ApiHerramientaWeb/swagger");
    return Task.CompletedTask;
});

// Endpoint de prueba
app.MapGet("/testcors", () => Results.Ok("CORS funciona!"));

app.Run();
