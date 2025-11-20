using ApiHerramientaWeb.Modelos.Cobranza.Recibe;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System.IO;
using System.Linq;

namespace ApiHerramientaWeb.Services
{
    public interface IPdfGeneratorService
    {
        byte[] GenerarReciboEntregaPdf(RecibeEntregaRequest request);
    }

    public class PdfGeneratorService : IPdfGeneratorService
    {
        public byte[] GenerarReciboEntregaPdf(RecibeEntregaRequest request)
        {
            MemoryStream memoryStream = new MemoryStream();
            Document document = null;
            PdfWriter writer = null;

            try
            {
                // Formato carta vertical para mejor distribución en una sola página
                document = new Document(PageSize.LETTER, 20, 20, 15, 15);
                writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Fuentes optimizadas para una página
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(44, 62, 80));
                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, new BaseColor(33, 97, 140));
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(52, 73, 94));
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, new BaseColor(44, 62, 80));
                var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.DARK_GRAY);
                var signatureFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, new BaseColor(52, 73, 94));

                // ENCABEZADO COMPACTO
                var headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100;
                headerTable.SpacingAfter = 8f;

                var companyCell = new PdfPCell(new Phrase("CASAVISION", headerFont));
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.HorizontalAlignment = Element.ALIGN_CENTER;
                companyCell.PaddingBottom = 3f;

                var titleCell = new PdfPCell(new Phrase("COMPROBANTE DE ENTREGA COLECTORES", titleFont));
                titleCell.Border = Rectangle.NO_BORDER;
                titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                titleCell.PaddingBottom = 5f;

                headerTable.AddCell(companyCell);
                headerTable.AddCell(titleCell);
                document.Add(headerTable);

                // TOTAL PRINCIPAL COMPACTO
                var totalHeaderTable = new PdfPTable(1);
                totalHeaderTable.WidthPercentage = 70;
                totalHeaderTable.HorizontalAlignment = Element.ALIGN_CENTER;
                totalHeaderTable.SpacingAfter = 12f;

                var totalLabelCell = new PdfPCell(new Phrase("TOTAL DE ENTREGA", boldFont));
                totalLabelCell.Border = Rectangle.NO_BORDER;
                totalLabelCell.HorizontalAlignment = Element.ALIGN_CENTER;
                totalLabelCell.BackgroundColor = new BaseColor(248, 249, 250);
                totalLabelCell.Padding = 5f;

                var totalValueCell = new PdfPCell(new Phrase(FormatCurrency(request.TotalCanceladas), totalFont));
                totalValueCell.Border = Rectangle.NO_BORDER;
                totalValueCell.HorizontalAlignment = Element.ALIGN_CENTER;
                totalValueCell.Padding = 8f;
                totalValueCell.BackgroundColor = new BaseColor(240, 247, 255);

                totalHeaderTable.AddCell(totalLabelCell);
                totalHeaderTable.AddCell(totalValueCell);
                document.Add(totalHeaderTable);

                // INFORMACIÓN BÁSICA COMPACTA
                var infoTable = new PdfPTable(4);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 25, 25, 25, 25 });
                infoTable.SpacingAfter = 12f;

                infoTable.AddCell(CreateCompactCell("Nº ENTREGA", boldFont, true, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateCompactCell("FECHA", boldFont, true, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateCompactCell("ENTREGADO POR", boldFont, true, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateCompactCell("RECIBIDO POR", boldFont, true, Element.ALIGN_CENTER));

                string nombreUsuario = $"{request.User?.FirstName} {request.User?.LastName}".Trim();
                infoTable.AddCell(CreateCompactCell($"#{request.Entrega.IDEENTCOL}", normalFont, false, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateCompactCell(request.Entrega.FCHENT.ToString("dd/MM/yyyy"), normalFont, false, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateCompactCell(nombreUsuario, normalFont, false, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateCompactCell(request.Agente, normalFont, false, Element.ALIGN_CENTER));

                document.Add(infoTable);

                // CONTENIDO PRINCIPAL EN UNA SOLA FILA
                var mainContentTable = new PdfPTable(2);
                mainContentTable.WidthPercentage = 100;
                mainContentTable.SetWidths(new float[] { 50, 50 });
                mainContentTable.SpacingAfter = 15f;

                // COLUMNA IZQUIERDA: DENOMINACIONES
                var leftCell = new PdfPCell();
                leftCell.Border = Rectangle.NO_BORDER;
                leftCell.Padding = 3;

                var denominacionesTitle = new Paragraph("ARQUEO DE EFECTIVO", titleFont);
                denominacionesTitle.Alignment = Element.ALIGN_CENTER;
                denominacionesTitle.SpacingAfter = 5f;
                leftCell.AddElement(denominacionesTitle);

                var denominacionesTable = CreateCompactDenominacionesTable(request, boldFont, normalFont);
                leftCell.AddElement(denominacionesTable);

                // Agregar diferencia
                var diferencia = new Paragraph($"Diferencia: {FormatCurrency(System.Math.Abs(request.Diferencia))} " +
                    $"{(request.Diferencia == 0 ? "CUADRADO" : (request.Diferencia > 0 ? "SOBRANTE" : "FALTANTE"))}",
                    new Font(FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8,
                        request.Diferencia == 0 ? BaseColor.GREEN :
                        request.Diferencia > 0 ? BaseColor.ORANGE : BaseColor.RED)));
                diferencia.Alignment = Element.ALIGN_CENTER;
                diferencia.SpacingBefore = 5f;
                leftCell.AddElement(diferencia);

                mainContentTable.AddCell(leftCell);

                // COLUMNA DERECHA: SERVICIOS
                var rightCell = new PdfPCell();
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.Padding = 3;

                if (request.Costos != null && request.Costos.Any())
                {
                    var serviciosTitle = new Paragraph("SERVICIOS", titleFont);
                    serviciosTitle.Alignment = Element.ALIGN_CENTER;
                    serviciosTitle.SpacingAfter = 5f;
                    rightCell.AddElement(serviciosTitle);

                    var serviciosTable = CreateCompactServiciosTable(request, boldFont, normalFont);
                    rightCell.AddElement(serviciosTable);
                }
                else
                {
                    var noServicios = new Paragraph("NO HAY SERVICIOS", smallFont);
                    noServicios.Alignment = Element.ALIGN_CENTER;
                    rightCell.AddElement(noServicios);
                }

                mainContentTable.AddCell(rightCell);
                document.Add(mainContentTable);

                // FIRMAS COMPACTAS
                var firmasTable = new PdfPTable(2);
                firmasTable.WidthPercentage = 100;
                firmasTable.SetWidths(new float[] { 50, 50 });
                firmasTable.SpacingBefore = 20f;

                firmasTable.AddCell(CreateCompactSignatureCell("ENTREGADO POR", signatureFont));
                firmasTable.AddCell(CreateCompactSignatureCell("RECIBIDO POR", signatureFont));

                document.Add(firmasTable);

                // PIE DE PÁGINA COMPACTO
                var footer = new Paragraph($"Documento generado electrónicamente el {System.DateTime.Now:dd/MM/yyyy HH:mm} - Válido como comprobante de entrega", smallFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 10f;
                document.Add(footer);

                document.Close();
                writer.Close();

                var pdfBytes = memoryStream.ToArray();
                return pdfBytes ?? throw new System.InvalidOperationException("El PDF generado está vacío");
            }
            catch (System.Exception ex)
            {
                writer?.Close();
                document?.Close();
                throw new System.Exception($"Error al generar PDF: {ex.Message}", ex);
            }
            finally
            {
                memoryStream?.Dispose();
            }
        }

        private PdfPTable CreateCompactDenominacionesTable(RecibeEntregaRequest request, Font boldFont, Font normalFont)
        {
            var table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 45, 25, 30 });

            table.AddCell(CreateCompactCell("DENOMINACIÓN", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateCompactCell("CANTIDAD", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateCompactCell("TOTAL", boldFont, true, Element.ALIGN_RIGHT));

            var denominacionesPredefinidas = new[]
            {
                new { Valor = 1000m, Nombre = "C$ 1,000.00" },
                new { Valor = 500m, Nombre = "C$ 500.00" },
                new { Valor = 200m, Nombre = "C$ 200.00" },
                new { Valor = 100m, Nombre = "C$ 100.00" },
                new { Valor = 50m, Nombre = "C$ 50.00" },
                new { Valor = 20m, Nombre = "C$ 20.00" },
                new { Valor = 10m, Nombre = "C$ 10.00" },
                new { Valor = 5m, Nombre = "C$ 5.00" },
                new { Valor = 1m, Nombre = "C$ 1.00" }
            };

            decimal totalDenominaciones = 0;
            foreach (var denom in denominacionesPredefinidas)
            {
                var denominacionExistente = request.Denominaciones?.FirstOrDefault(d => d.Valor == denom.Valor);
                var cantidad = denominacionExistente?.Cantidad ?? 0;
                var total = denominacionExistente?.Total ?? 0;

                table.AddCell(CreateCompactCell(denom.Nombre, normalFont, false, Element.ALIGN_LEFT));
                table.AddCell(CreateCompactCell(cantidad.ToString("N0"), normalFont, false, Element.ALIGN_CENTER));
                table.AddCell(CreateCompactCell(FormatCurrency(total), normalFont, false, Element.ALIGN_RIGHT));

                totalDenominaciones += total;
            }

            table.AddCell(CreateCompactCell("TOTAL ARQUEO", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateCompactCell("", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateCompactCell(FormatCurrency(totalDenominaciones), boldFont, true, Element.ALIGN_RIGHT));

            return table;
        }

        private PdfPTable CreateCompactServiciosTable(RecibeEntregaRequest request, Font boldFont, Font normalFont)
        {
            var table = new PdfPTable(4); // 4 columnas ahora
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 40, 20, 20, 20 }); // Ajuste de anchos para 4 columnas

            table.AddCell(CreateCompactCell("DESCRIPCIÓN", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateCompactCell("CANTIDAD", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateCompactCell("PRECIO UNIT.", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateCompactCell("TOTAL", boldFont, true, Element.ALIGN_RIGHT));

            decimal totalServicios = 0;
            foreach (var costo in request.Costos)
            {
                table.AddCell(CreateCompactCell(costo.Servicio, normalFont, false, Element.ALIGN_LEFT));
                table.AddCell(CreateCompactCell(costo.CantidadFacturas.ToString("N0"), normalFont, false, Element.ALIGN_CENTER));
                table.AddCell(CreateCompactCell(FormatCurrency(costo.PrecioUnitario), normalFont, false, Element.ALIGN_CENTER));
                table.AddCell(CreateCompactCell(FormatCurrency(costo.TotalServicio), normalFont, false, Element.ALIGN_RIGHT));
                totalServicios += costo.TotalServicio;
            }

            table.AddCell(CreateCompactCell("TOTAL SERVICIOS", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateCompactCell("", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateCompactCell("", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateCompactCell(FormatCurrency(totalServicios), boldFont, true, Element.ALIGN_RIGHT));

            return table;
        }

        private PdfPCell CreateCompactCell(string text, Font font, bool isHeader, int alignment)
        {
            return new PdfPCell(new Phrase(text ?? "", font))
            {
                Padding = 4,
                PaddingTop = 3,
                PaddingBottom = 3,
                BackgroundColor = isHeader ? new BaseColor(248, 249, 250) : BaseColor.WHITE,
                BorderWidth = 0.5f,
                BorderColor = new BaseColor(200, 200, 200),
                HorizontalAlignment = alignment,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
        }

        private PdfPCell CreateCompactSignatureCell(string text, Font font)
        {
            var cell = new PdfPCell();
            cell.Border = Rectangle.NO_BORDER;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 8;

            var innerTable = new PdfPTable(1);
            innerTable.WidthPercentage = 80;
            innerTable.HorizontalAlignment = Element.ALIGN_CENTER;

            var lineCell = new PdfPCell();
            lineCell.Border = Rectangle.NO_BORDER;
            lineCell.HorizontalAlignment = Element.ALIGN_CENTER;
            lineCell.Padding = 4;

            var line = new LineSeparator(0.5f, 80, BaseColor.BLACK, Element.ALIGN_CENTER, 0);
            lineCell.AddElement(new Chunk(line));

            var textCell = new PdfPCell(new Phrase(text, font));
            textCell.Border = Rectangle.NO_BORDER;
            textCell.HorizontalAlignment = Element.ALIGN_CENTER;
            textCell.PaddingTop = 2;

            innerTable.AddCell(lineCell);
            innerTable.AddCell(textCell);
            cell.AddElement(innerTable);

            return cell;
        }

        private string FormatCurrency(decimal value)
        {
            return $"C$ {value.ToString("N2")}";
        }
    }
}