using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ApiHerramientaWeb.Services
{
    public class ConfiguracionEmail
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfiguracionEmail> _logger;

        public ConfiguracionEmail(IConfiguration configuration, ILogger<ConfiguracionEmail> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Método específico para PDF (mantiene compatibilidad)
        public async Task<(bool Exito, string? Error)> EnviarCorreoConPdf(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            byte[]? pdfBytes = null,
            string? nombreArchivo = null)
        {
            var adjuntos = new List<ArchivoAdjunto>();

            if (pdfBytes != null && !string.IsNullOrEmpty(nombreArchivo))
            {
                adjuntos.Add(new ArchivoAdjunto
                {
                    Contenido = pdfBytes,
                    NombreArchivo = nombreArchivo,
                    TipoMime = "application/pdf"
                });
            }

            return await EnviarCorreoSendGridSMTP(destinatario, asunto, cuerpoHtml, adjuntos);
        }

        // Método genérico para cualquier tipo de archivo
        public async Task<(bool Exito, string? Error)> EnviarCorreoConAdjuntos(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            List<ArchivoAdjunto>? adjuntos = null)
        {
            return await EnviarCorreoSendGridSMTP(destinatario, asunto, cuerpoHtml, adjuntos);
        }

        // Método sin adjuntos
        public async Task<(bool Exito, string? Error)> EnviarCorreoSimple(
            string destinatario,
            string asunto,
            string cuerpoHtml)
        {
            return await EnviarCorreoSendGridSMTP(destinatario, asunto, cuerpoHtml, null);
        }

        // Método principal corregido
        public async Task<(bool Exito, string? Error)> EnviarCorreoSendGridSMTP(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            List<ArchivoAdjunto>? adjuntos = null)
        {
            MailMessage mailMessage = null;
            SmtpClient smtpClient = null;
            List<Attachment> attachmentsList = new List<Attachment>();

            try
            {
                // Verificar configuración primero
                var emailSection = _configuration.GetSection("EmailSettings");
                if (emailSection == null)
                {
                    _logger.LogError("No se encontró la sección EmailSettings en la configuración");
                    return (false, "Configuración de email no encontrada");
                }

                var smtpServer = emailSection.GetValue<string>("SmtpServer");
                var smtpPort = emailSection.GetValue<int>("SmtpPort");
                var apiKey = emailSection.GetValue<string>("ApiKey");
                var fromEmail = emailSection.GetValue<string>("FromEmail");
                var fromName = emailSection.GetValue<string>("FromName");

                // Validar configuración requerida
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(apiKey) ||
                    string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(destinatario))
                {
                    _logger.LogError("Configuración de email incompleta. SmtpServer: {SmtpServer}, FromEmail: {FromEmail}, Destinatario: {Destinatario}",
                        smtpServer, fromEmail, destinatario);
                    return (false, "Configuración de email incompleta");
                }

                mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };

                // Agregar destinatario
                mailMessage.To.Add(new MailAddress(destinatario));

                // Adjuntar archivos si se proporcionan - CORREGIDO
                if (adjuntos != null && adjuntos.Any())
                {
                    foreach (var adjunto in adjuntos)
                    {
                        if (adjunto.Contenido != null && adjunto.Contenido.Length > 0)
                        {
                            // CORRECCIÓN: No usar using para el MemoryStream
                            // Crear el Attachment directamente desde el byte array
                            var stream = new MemoryStream(adjunto.Contenido);
                            var attachment = new Attachment(stream, adjunto.NombreArchivo, adjunto.TipoMime);

                            // Guardar referencia para disposición posterior
                            attachmentsList.Add(attachment);
                            mailMessage.Attachments.Add(attachment);

                            _logger.LogInformation("Archivo adjunto agregado: {NombreArchivo} ({Tamaño} bytes)",
                                adjunto.NombreArchivo, adjunto.Contenido.Length);
                        }
                    }
                }

                smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential("apikey", apiKey),
                    EnableSsl = true,
                    Timeout = 30000 // 30 segundos timeout
                };

                _logger.LogInformation("Enviando correo a {Destinatario} via {SmtpServer}:{SmtpPort}",
                    destinatario, smtpServer, smtpPort);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Correo enviado exitosamente a {Destinatario}", destinatario);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo a {Destinatario}: {ErrorMessage}",
                    destinatario, ex.Message);

                return (false, $"Error enviando correo: {ex.Message}");
            }
            finally
            {
                // Limpiar recursos de manera segura - CORREGIDO
                try
                {
                    // Primero disposar los attachments
                    foreach (var attachment in attachmentsList)
                    {
                        try
                        {
                            // Al disposar el attachment, también se disposará el stream
                            attachment.Dispose();
                        }
                        catch (Exception attachEx)
                        {
                            _logger.LogWarning(attachEx, "Error al disposar attachment");
                        }
                    }

                    // Luego el mail message
                    mailMessage?.Dispose();

                    // Finalmente el cliente SMTP
                    smtpClient?.Dispose();
                }
                catch (Exception disposeEx)
                {
                    _logger.LogWarning(disposeEx, "Error al limpiar recursos de email");
                }
            }
        }
    }

    public class ArchivoAdjunto
    {
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoMime { get; set; } = "application/octet-stream";

        // Constructor para facilitar la creación
        public ArchivoAdjunto() { }

        public ArchivoAdjunto(byte[] contenido, string nombreArchivo, string tipoMime = "application/octet-stream")
        {
            Contenido = contenido;
            NombreArchivo = nombreArchivo;
            TipoMime = tipoMime;
        }
    }
}