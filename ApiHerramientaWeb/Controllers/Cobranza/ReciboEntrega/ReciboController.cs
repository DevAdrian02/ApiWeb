using ApiHerramientaWeb.Modelos.Cobranza.Recibe;
using ApiHerramientaWeb.Modelos.Cobranza.Recibo;
using ApiHerramientaWeb.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReciboController : Controller
{
    private readonly ConfiguracionEmail _configEmail;
    private readonly IPdfGeneratorService _pdfGeneratorService;
    private readonly ILogger<ReciboController> _logger;

    public ReciboController(ConfiguracionEmail configEmail, IPdfGeneratorService pdfGeneratorService, ILogger<ReciboController> logger)
    {
        _configEmail = configEmail;
        _pdfGeneratorService = pdfGeneratorService;
        _logger = logger;
    }

    [HttpPost("EnviarReciboEntrega")]
    public async Task<IActionResult> EnviarReciboEntrega([FromBody] RecibeEntregaRequest request)
    {
        if (string.IsNullOrEmpty(request.Destinatario))
        {
            return BadRequest(new { status = 0, message = "El destinatario es obligatorio." });
        }

        try
        {
            _logger.LogInformation("Iniciando generación de recibo para entrega: {IDEENTCOL}", request.Entrega.IDEENTCOL);

            string asunto = request.Asunto ?? $"Recibo de Entrega #{request.Entrega.IDEENTCOL}";
            string cuerpoCorreo = GeneradorCuerpoCorreo.GenerarCuerpoRecibo(request);

            // Generar PDF - estilo similar al ejemplo
            byte[] pdfBytes;
            try
            {
                pdfBytes = _pdfGeneratorService.GenerarReciboEntregaPdf(request);
                _logger.LogInformation("PDF generado exitosamente. Tamaño: {Tamaño} bytes", pdfBytes.Length);
            }
            catch (Exception pdfEx)
            {
                _logger.LogError(pdfEx, "Error al generar PDF");
                return StatusCode(500, new { status = 0, message = $"Error al generar PDF: {pdfEx.Message}" });
            }

            // Guardar temporalmente para diagnóstico (como en tu ejemplo)
            try
            {
                var tempPath = Path.GetTempPath();
                var tempFile = Path.Combine(tempPath, $"ReciboEntrega_{request.Entrega.IDEENTCOL}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                await System.IO.File.WriteAllBytesAsync(tempFile, pdfBytes);
                _logger.LogInformation("PDF guardado temporalmente en: {Ruta}", tempFile);
            }
            catch (Exception tempEx)
            {
                _logger.LogWarning(tempEx, "No se pudo guardar el PDF temporalmente");
            }

            // ENVÍO CON SENDGRID - Estilo similar a tu ejemplo
            try
            {
                // Aquí usarías tu configuración de SendGrid
                var (exito, error) = await _configEmail.EnviarCorreoConPdf(
                    request.Destinatario,
                    asunto,
                    cuerpoCorreo,
                    pdfBytes,
                    $"ReciboEntrega_{request.Entrega.IDEENTCOL}.pdf"
                );

                if (exito)
                {
                    _logger.LogInformation("Recibo enviado exitosamente a: {Destinatario}", request.Destinatario);
                    return Ok(new { status = 1, message = "Recibo enviado exitosamente" });
                }
                else
                {
                    _logger.LogError("Error enviando correo: {Error}", error);
                    return Ok(new { status = 0, message = $"No se pudo enviar el recibo: {error}" });
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Error en el envío de correo");

                // Log del error similar a tu ejemplo
                // Aquí podrías guardar en tu base de datos como en tu ejemplo

                return Ok(new { status = 0, message = $"Error al enviar correo: {emailEx.Message}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completo en EnviarReciboEntrega");
            return StatusCode(500, new { status = 0, message = $"Error interno del servidor: {ex.Message}" });
        }
    }
}