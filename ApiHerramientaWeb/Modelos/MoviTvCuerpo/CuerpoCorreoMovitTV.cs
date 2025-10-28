namespace ApiHerramientaWeb.Modelos.MoviTvCuerpo
{
    public class CuerpoCorreoMovitTV
    {
        public string GenerarCorreoBienvenida(UsuarioMoviTv datos)
        {
            var htmlTemplate = @"
<!DOCTYPE html>
<html lang='es'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Bienvenido a Casavision+</title>
  <style>
    /* Estilos compatibles con email - Tema oscuro moderno */
    body {
      font-family: 'Arial', 'Helvetica', sans-serif;
      background-color: #0a0a0a;
      margin: 0;
      padding: 20px;
      color: #e0e0e0;
    }
    .container {
      max-width: 600px;
      margin: 0 auto;
      background-color: #1a1a1a;
      border: 1px solid #333;
      border-radius: 8px;
      overflow: hidden;
    }
    .header {
      background-color: #151515;
      color: #ffffff;
      padding: 40px 20px;
      text-align: center;
      border-bottom: 3px solid #ff6a00;
    }
    .content {
      padding: 30px 20px;
      background-color: #1a1a1a;
    }
    .credentials {
      background-color: #252525;
      border: 1px solid #333;
      padding: 25px;
      margin: 25px 0;
      border-radius: 8px;
      border-left: 4px solid #ff6a00;
    }
    .credential-item {
      display: table;
      width: 100%;
      margin-bottom: 18px;
      padding-bottom: 18px;
      border-bottom: 1px solid #333;
    }
    .credential-item:last-child {
      border-bottom: none;
      margin-bottom: 0;
      padding-bottom: 0;
    }
    .credential-label {
      display: table-cell;
      width: 130px;
      font-weight: bold;
      color: #a0a0a0;
      font-size: 14px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .credential-value {
      display: table-cell;
      font-weight: bold;
      color: #ffffff;
      font-size: 16px;
      letter-spacing: 0.5px;
    }
    .step {
      background-color: #252525;
      padding: 25px;
      margin: 20px 0;
      border-radius: 8px;
      border: 1px solid #333;
    }
    .step-number {
      background-color: #ff6a00;
      color: white;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: inline-block;
      text-align: center;
      line-height: 32px;
      font-weight: bold;
      margin-right: 15px;
      font-size: 14px;
    }
    .cta-container {
      text-align: center;
      margin: 30px 0;
    }
    .cta {
      display: inline-block;
      background-color: #ff6a00;
      color: white;
      padding: 16px 35px;
      text-decoration: none;
      border-radius: 6px;
      font-weight: bold;
      font-size: 16px;
      text-transform: uppercase;
      letter-spacing: 1px;
    }
    .footer {
      background-color: #151515;
      padding: 30px 20px;
      text-align: center;
      border-top: 1px solid #333;
    }
    .warning {
      background-color: #2a1a0a;
      border: 1px solid #ff6a00;
      padding: 18px;
      margin: 25px 0;
      border-radius: 6px;
      color: #ffa057;
      border-left: 4px solid #ff6a00;
    }
    .social-links {
      margin: 25px 0;
    }
    .social-link {
      display: inline-block;
      width: 40px;
      height: 40px;
      background-color: #333;
      color: #a0a0a0;
      text-decoration: none;
      border-radius: 6px;
      line-height: 40px;
      text-align: center;
      margin: 0 8px;
      font-weight: bold;
    }
    .section-title {
      color: #ffffff;
      font-size: 18px;
      font-weight: bold;
      margin-bottom: 20px;
      padding-bottom: 10px;
      border-bottom: 2px solid #333;
      text-transform: uppercase;
      letter-spacing: 1px;
    }
    @media only screen and (max-width: 600px) {
      .container {
        width: 100% !important;
        border-radius: 0;
      }
      body {
        padding: 10px;
      }
    }
  </style>
</head>
<body>
  <center>
    <table class='container' border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #1a1a1a;'>
      <!-- Header -->
      <tr>
        <td class='header'>
          <table width='100%' border='0' cellpadding='0' cellspacing='0'>
            <tr>
              <td style='text-align: center;'>
                <div style='font-size: 48px; margin-bottom: 15px;'>✨</div>
                <h1 style='margin:0; font-size: 32px; color: #ffffff; font-weight: 800; letter-spacing: 1px;'>CASAVISION+</h1>
                <div style='height: 3px; background: linear-gradient(90deg, transparent, #ff6a00, transparent); width: 100px; margin: 15px auto;'></div>
                <h2 style='margin:15px 0 8px 0; font-size: 24px; color: #ffffff;'>Bienvenido, {{Nombre}}</h2>
                <p style='margin:0; opacity:0.8; font-size: 16px;'>Tu acceso a la nueva era del entretenimiento</p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
      
      <!-- Content -->
      <tr>
        <td class='content'>
          <p style='font-size:16px; text-align:center; margin-bottom:30px; color:#a0a0a0; line-height: 1.6;'>
            Tu suscripción premium está activa. Disfruta de contenido exclusivo en la plataforma más avanzada.
          </p>
          
          <div class='section-title'>Tus credenciales</div>
          
          <table class='credentials' width='100%' border='0' cellpadding='0' cellspacing='0'>
            <tr>
              <td>
                <div class='credential-item'>
                  <span class='credential-label'>Usuario</span>
                  <span class='credential-value'>{{Usuario}}</span>
                </div>
                <div class='credential-item'>
                  <span class='credential-label'>Contraseña</span>
                  <span class='credential-value'>{{Password}}</span>
                </div>
              </td>
            </tr>
          </table>
          
          <div class='section-title'>Acceso inmediato</div>
          
          <table width='100%' border='0' cellpadding='0' cellspacing='0'>
            <tr>
              <td class='step'>
                <span class='step-number'>1</span>
                <strong style='color: #ffffff; font-size: 16px;'>Conéctate a nuestra plataforma</strong><br>
                <span style='color: #a0a0a0;'>Accede desde cualquier dispositivo en <a href='{{Url}}' style='color:#ff6a00; font-weight:bold; text-decoration:none;'>{{Url}}</a></span>
              </td>
            </tr>
            <tr>
              <td class='step'>
                <span class='step-number'>2</span>
                <strong style='color: #ffffff; font-size: 16px;'>Inicia sesión</strong><br>
                <span style='color: #a0a0a0;'>Utiliza tus credenciales para acceder al contenido</span>
              </td>
            </tr>
            <tr>
              <td class='step'>
                <span class='step-number'>3</span>
                <strong style='color: #ffffff; font-size: 16px;'>Experimenta el futuro</strong><br>
                <span style='color: #a0a0a0;'>Descubre contenido exclusivo con la mejor calidad</span>
              </td>
            </tr>
          </table>
          
          <div class='cta-container'>
            <a href='{{Url}}' class='cta' style='color:#ffffff; text-decoration:none;'>
              ACCEDER AHORA →
            </a>
          </div>
          
          <p style='text-align:center; color:#a0a0a0; font-size:14px; line-height: 1.6;'>
            ¿Necesitas ayuda? Nuestro equipo de soporte está disponible 24/7.
          </p>
        </td>
      </tr>
      
      <!-- Footer -->
      <tr>
        <td class='footer'>
          <table width='100%' border='0' cellpadding='0' cellspacing='0'>
            <tr>
              <td style='text-align: center;'>
                <div style='font-weight:bold; font-size:20px; color:#ffffff; margin-bottom:10px; letter-spacing: 1px;'>CASAVISION+</div>
                <p style='color:#a0a0a0; margin-bottom:25px; font-size:14px;'>El futuro del entretenimiento, hoy</p>
                
                <div class='social-links'>
                <div class='social-links'>
                  <a href='https://www.facebook.com/casavisionnicaragua' class='social-link'>FB</a>
                  <a href='https://api.whatsapp.com/send?phone=%2B50582412272&text=NECESITO%20AYUDA%20CON%20MOVITV' class='social-link'>WA</a>                </div>
                
                <div class='warning'>
                  <strong>INFORMACIÓN CONFIDENCIAL:</strong> Protege tus credenciales de acceso.
                </div>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </center>
</body>
</html>";

            // Reemplazar los placeholders con los datos reales
            var htmlFinal = htmlTemplate
                .Replace("{{Nombre}}", datos.Nombre ?? "")
                .Replace("{{Usuario}}", datos.Usuario ?? "")
                .Replace("{{Password}}", datos.Password ?? "")
                .Replace("{{Url}}", datos.Url ?? "");

            return htmlFinal;
        }
    }
}