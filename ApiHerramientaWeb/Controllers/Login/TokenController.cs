namespace ApiHerramientaWeb.Controllers.Login
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;
    using System.Collections.Generic;
    using System.Linq;

    namespace ApiHerramientaWeb.Controllers.Login
    {
        [Route("api/[controller]")]
        [ApiController]
        public class TokenController : ControllerBase
        {
            private readonly IConfiguration _configuration;

            public TokenController(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public class RefreshTokenRequest
            {
                public string AccessToken { get; set; }
                public string RefreshToken { get; set; }
            }

            [HttpPost("refresh")]
            public IActionResult Refresh([FromBody] RefreshTokenRequest request)
            {
                try
                {
                    // Validar el token de acceso expirado
                    var principal = GetPrincipalFromExpiredToken(request.AccessToken);
                    var userId = principal.FindFirst("userIdHW")?.Value;

                    if (string.IsNullOrEmpty(userId))
                        return Unauthorized(new { Message = "Token inválido" });

                    // Validar el refresh token (en producción debería verificar en base de datos)
                    if (!IsValidRefreshToken(request.RefreshToken))
                        return Unauthorized(new { Message = "Refresh token inválido" });

                    // Generar nuevo token
                    var newAccessToken = GenerateJwtToken(principal.Claims);
                    var newRefreshToken = GenerateRefreshToken();

                    return Ok(new
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        Expiration = DateTime.UtcNow.AddMinutes(1440)
                    });
                }
                catch (SecurityTokenException ex)
                {
                    return Unauthorized(new { Message = $"Token inválido: {ex.Message}" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = $"Error interno: {ex.Message}" });
                }
            }

            private bool IsValidRefreshToken(string refreshToken)
            {
                // Implementación básica (en producción debería verificar en base de datos)
                return !string.IsNullOrWhiteSpace(refreshToken);
            }

            private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(_configuration["Jwt:Key"])),
                    ValidateLifetime = false // Ignorar expiración
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out var securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Token inválido");

                return principal;
            }

            private string GenerateJwtToken(IEnumerable<Claim> claims)
            {
                var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(1440),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            private string GenerateRefreshToken()
            {
                var randomNumber = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
