using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ModeloPrincipal.Entity;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiHerramientaWeb.Controllers.Login
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly CVGEntities _context;
        private readonly IConfiguration _configuration;

        public LoginController(CVGEntities context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public class LoginRequest
        {
            public string CODUSR { get; set; }
            public string USRPAS { get; set; }
        }

        public class LoginResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public string AccessToken { get; set; }
            public int UserId { get; set; }
            public List<string> Roles { get; set; }
            public string Username { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime Expiration { get; set; }

            public List<int> Sucursales { get; set; }
        }

        [HttpPost("Authorize")]
        [SwaggerOperation(Summary = "Autentica al usuario")]
        public IActionResult Authorize([FromBody] LoginRequest request)
        {
            try
            {
                // 1. Validar credenciales
                var encryptedPassword = sifco.Security.sifcoSec.EncryptData(request.USRPAS);
                var user = _context.Mstusrs
                    .FirstOrDefault(u => u.Codusr.ToUpper() == request.CODUSR.ToUpper() &&
                                        u.Usrpas == encryptedPassword &&
                                        u.Estusr == 0);
                var sucursales = _context.Relusrsucs
                    .Where(s => s.Idusuario == user.Ideusr)
                    .Select(s => s.Idesuc)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (user == null)
                    return Unauthorized(new LoginResponse
                    {
                        StatusCode = 401,
                        Message = "Credenciales inválidas"
                    });

                // 2. Obtener roles
                var roles = _context.Relusrrols
                    .Where(r => r.Ideusr == user.Ideusr)
                    .Select(r => r.Iderol.ToString())
                    .ToList();

                // 3. Generar token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Codusr),
                        new Claim("userId", user.Ideusr.ToString()),
                        new Claim("username", user.Codusr),
                        new Claim(ClaimTypes.Role, string.Join(",", roles))
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // 4. Auditoría
                _context.Logus.Add(new Logu
                {
                    Idsesapp = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Ideusr = user.Ideusr,
                    Codusr = user.Codusr,
                    Fchappcre = DateTime.Now,
                    Fchcre = DateTime.Now,
                    Creips = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Crehsn = Environment.MachineName
                });
                _context.SaveChanges();

                // 5. Retornar respuesta
                return Ok(new LoginResponse
                {
                    StatusCode = 200,
                    Message = "Autenticación exitosa",
                    AccessToken = tokenString,
                    UserId = user.Ideusr,
                    Roles = roles,
                    Username = user.Codusr,
                    Nombre = user.Nomusr,
                    Apellido = user.Apepri,
                    Expiration = tokenDescriptor.Expires.Value,
                    Sucursales = sucursales
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    StatusCode = 500,
                    Message = $"Error interno: {ex.Message}"
                });
            }
        }

        [HttpGet("Auth/me")]
        [SwaggerOperation(Summary = "Obtiene datos del usuario autenticado")]
        public IActionResult GetAuthenticatedUser()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader)) return Unauthorized();

                var token = authHeader.Split(" ").Last();
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var userId = int.Parse(jwt.Claims.First(c => c.Type == "userId").Value);
                var user = _context.Mstusrs.FirstOrDefault(u => u.Ideusr == userId);

                var roles = _context.Relusrrols
                    .Where(r => r.Ideusr == userId)
                    .Select(r => r.Iderol.ToString())
                    .ToList();

                // Obtener sucursales igual que en Authorize
                var sucursales = _context.Relusrsucs
                    .Where(s => s.Idusuario == userId)
                    .Select(s => s.Idesuc)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (user == null) return NotFound();

                // Obtener la fecha de expiración del token
                var expiration = jwt.ValidTo;

                return Ok(new LoginResponse
                {
                    StatusCode = 200,
                    Message = "Autenticación exitosa",
                    AccessToken = token,
                    UserId = user.Ideusr,
                    Roles = roles,
                    Username = user.Codusr,
                    Nombre = user.Nomusr,
                    Apellido = user.Apepri,
                    Expiration = expiration,
                    Sucursales = sucursales
                });
            }
            catch
            {
                return Unauthorized();
            }
        }
    }
}