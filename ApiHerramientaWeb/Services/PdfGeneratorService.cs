using ApiHerramientaWeb.Modelos.Cobranza.Recibe;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System.IO;

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
                // Formato carta horizontal con márgenes optimizados
                document = new Document(PageSize.LETTER.Rotate(), 25, 25, 20, 20);
                writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Fuentes profesionales
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, new BaseColor(44, 62, 80));
                var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(33, 97, 140));
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(52, 73, 94));
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(44, 62, 80));
                var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.DARK_GRAY);
                var signatureFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(52, 73, 94));

                // ENCABEZADO PROFESIONAL
                var headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100;
                headerTable.SpacingAfter = 15f;

                var companyCell = new PdfPCell(new Phrase("CASAVISION", headerFont));
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.HorizontalAlignment = Element.ALIGN_CENTER;
                companyCell.PaddingBottom = 5f;

                var titleCell = new PdfPCell(new Phrase("COMPROBANTE DE ENTREGA COLECTORES", titleFont));
                titleCell.Border = Rectangle.NO_BORDER;
                titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                titleCell.PaddingBottom = 10f;

                headerTable.AddCell(companyCell);
                headerTable.AddCell(titleCell);
                document.Add(headerTable);

                // ENCABEZADO CON TOTAL PRINCIPAL DESTACADO
                var totalHeaderTable = new PdfPTable(1);
                totalHeaderTable.WidthPercentage = 80;
                totalHeaderTable.HorizontalAlignment = Element.ALIGN_CENTER;
                totalHeaderTable.SpacingAfter = 20f;

                var totalLabelCell = new PdfPCell(new Phrase("TOTAL DE ENTREGA", boldFont));
                totalLabelCell.Border = Rectangle.NO_BORDER;
                totalLabelCell.HorizontalAlignment = Element.ALIGN_CENTER;
                totalLabelCell.BackgroundColor = new BaseColor(248, 249, 250);
                totalLabelCell.Padding = 8f;

                var totalValueCell = new PdfPCell(new Phrase(FormatCurrency(request.TotalCanceladas), totalFont));
                totalValueCell.Border = Rectangle.NO_BORDER;
                totalValueCell.HorizontalAlignment = Element.ALIGN_CENTER;
                totalValueCell.Padding = 12f;
                totalValueCell.BackgroundColor = new BaseColor(240, 247, 255);

                totalHeaderTable.AddCell(totalLabelCell);
                totalHeaderTable.AddCell(totalValueCell);
                document.Add(totalHeaderTable);

                // INFORMACIÓN BÁSICA
                var infoTable = new PdfPTable(4);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 25, 25, 25, 25 });
                infoTable.SpacingAfter = 20f;

                infoTable.AddCell(CreateProfessionalCell("Nº ENTREGA", boldFont, true, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateProfessionalCell("FECHA", boldFont, true, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateProfessionalCell("ENTREGADO POR", boldFont, true, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateProfessionalCell("RECIBIDO POR", boldFont, true, Element.ALIGN_CENTER));

                infoTable.AddCell(CreateProfessionalCell($"#{request.Entrega.IDEENTCOL}", normalFont, false, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateProfessionalCell(request.Entrega.FCHENT.ToString("dd/MM/yyyy"), normalFont, false, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateProfessionalCell($"{request.User.FirstName} {request.User.LastName}", normalFont, false, Element.ALIGN_CENTER));
                infoTable.AddCell(CreateProfessionalCell(request.Agente, normalFont, false, Element.ALIGN_CENTER));

                document.Add(infoTable);

                // TABLA PRINCIPAL CON 2 COLUMNAS
                var mainTable = new PdfPTable(2);
                mainTable.WidthPercentage = 100;
                mainTable.SetWidths(new float[] { 50, 50 });
                mainTable.SpacingAfter = 25f;

                // COLUMNA 1: DENOMINACIONES
                var denominacionesCell = new PdfPCell();
                denominacionesCell.Border = Rectangle.NO_BORDER;
                denominacionesCell.Padding = 5;

                var denominacionesTitle = new Paragraph("ARQUEO DE EFECTIVO", titleFont);
                denominacionesTitle.Alignment = Element.ALIGN_CENTER;
                denominacionesTitle.SpacingAfter = 8f;
                denominacionesCell.AddElement(denominacionesTitle);

                var denominacionesTable = CreateProfessionalDenominacionesTable(request, boldFont, normalFont);
                denominacionesCell.AddElement(denominacionesTable);
                mainTable.AddCell(denominacionesCell);

                // COLUMNA 2: SERVICIOS
                var serviciosCell = new PdfPCell();
                serviciosCell.Border = Rectangle.NO_BORDER;
                serviciosCell.Padding = 5;

                if (request.Costos != null && request.Costos.Any())
                {
                    var serviciosTitle = new Paragraph("SERVICIOS", titleFont);
                    serviciosTitle.Alignment = Element.ALIGN_CENTER;
                    serviciosTitle.SpacingAfter = 8f;
                    serviciosCell.AddElement(serviciosTitle);

                    var serviciosTable = CreateProfessionalServiciosTable(request, boldFont, normalFont);
                    serviciosCell.AddElement(serviciosTable);
                }
                else
                {
                    serviciosCell.AddElement(new Paragraph(" "));
                }

                mainTable.AddCell(serviciosCell);
                document.Add(mainTable);

                // FIRMAS PROFESIONALES
                var firmasTable = new PdfPTable(2);
                firmasTable.WidthPercentage = 100;
                firmasTable.SetWidths(new float[] { 50, 50 });
                firmasTable.SpacingBefore = 30f;

                firmasTable.AddCell(CreateProfessionalSignatureCell("ENTREGADO POR", signatureFont));
                firmasTable.AddCell(CreateProfessionalSignatureCell("RECIBIDO POR", signatureFont));

                document.Add(firmasTable);

                // PIE DE PÁGINA
                var footer = new Paragraph($"Documento generado electrónicamente el {DateTime.Now:dd/MM/yyyy HH:mm} - Válido como comprobante de entrega", smallFont);
                footer.Alignment = Element.ALIGN_CENTER;
                footer.SpacingBefore = 15f;
                document.Add(footer);

                document.Close();
                writer.Close();

                var pdfBytes = memoryStream.ToArray();
                return pdfBytes ?? throw new InvalidOperationException("El PDF generado está vacío");
            }
            catch (Exception ex)
            {
                writer?.Close();
                document?.Close();
                throw new Exception($"Error al generar PDF: {ex.Message}", ex);
            }
            finally
            {
                memoryStream?.Dispose();
            }
        }

        private PdfPTable CreateProfessionalDenominacionesTable(RecibeEntregaRequest request, Font boldFont, Font normalFont)
        {
            var table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 45, 25, 30 });

            table.AddCell(CreateProfessionalCell("DENOMINACIÓN", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateProfessionalCell("CANTIDAD", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateProfessionalCell("TOTAL", boldFont, true, Element.ALIGN_RIGHT));

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

                table.AddCell(CreateProfessionalCell(denom.Nombre, normalFont, false, Element.ALIGN_LEFT));
                table.AddCell(CreateProfessionalCell(cantidad.ToString("N0"), normalFont, false, Element.ALIGN_CENTER));
                table.AddCell(CreateProfessionalCell(FormatCurrency(total), normalFont, false, Element.ALIGN_RIGHT));

                totalDenominaciones += total;
            }

            table.AddCell(CreateProfessionalCell("TOTAL ARQUEO", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateProfessionalCell("", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateProfessionalCell(FormatCurrency(totalDenominaciones), boldFont, true, Element.ALIGN_RIGHT));

            return table;
        }

        private PdfPTable CreateProfessionalServiciosTable(RecibeEntregaRequest request, Font boldFont, Font normalFont)
        {
            var table = new PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 50, 25, 25 });

            table.AddCell(CreateProfessionalCell("DESCRIPCIÓN", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateProfessionalCell("CANTIDAD", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateProfessionalCell("TOTAL", boldFont, true, Element.ALIGN_RIGHT));

            decimal totalServicios = 0;
            foreach (var costo in request.Costos)
            {
                table.AddCell(CreateProfessionalCell(costo.Servicio, normalFont, false, Element.ALIGN_LEFT));
                table.AddCell(CreateProfessionalCell(costo.CantidadFacturas.ToString("N0"), normalFont, false, Element.ALIGN_CENTER));
                table.AddCell(CreateProfessionalCell(FormatCurrency(costo.TotalServicio), normalFont, false, Element.ALIGN_RIGHT));
                totalServicios += costo.TotalServicio;
            }

            table.AddCell(CreateProfessionalCell("TOTAL SERVICIOS", boldFont, true, Element.ALIGN_LEFT));
            table.AddCell(CreateProfessionalCell("", boldFont, true, Element.ALIGN_CENTER));
            table.AddCell(CreateProfessionalCell(FormatCurrency(totalServicios), boldFont, true, Element.ALIGN_RIGHT));

            return table;
        }

        private PdfPCell CreateProfessionalCell(string text, Font font, bool isHeader, int alignment)
        {
            return new PdfPCell(new Phrase(text ?? "", font))
            {
                Padding = 6,
                PaddingTop = 5,
                PaddingBottom = 5,
                BackgroundColor = isHeader ? new BaseColor(248, 249, 250) : BaseColor.WHITE,
                BorderWidth = 0.75f,
                BorderColor = new BaseColor(200, 200, 200),
                HorizontalAlignment = alignment,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
        }

        private PdfPCell CreateProfessionalSignatureCell(string text, Font font)
        {
            var cell = new PdfPCell();
            cell.Border = Rectangle.NO_BORDER;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 15;

            var innerTable = new PdfPTable(1);
            innerTable.WidthPercentage = 80;
            innerTable.HorizontalAlignment = Element.ALIGN_CENTER;

            var lineCell = new PdfPCell();
            lineCell.Border = Rectangle.NO_BORDER;
            lineCell.HorizontalAlignment = Element.ALIGN_CENTER;
            lineCell.Padding = 8;

            var line = new LineSeparator(1f, 100, BaseColor.BLACK, Element.ALIGN_CENTER, 0);
            lineCell.AddElement(new Chunk(line));

            var textCell = new PdfPCell(new Phrase(text, font));
            textCell.Border = Rectangle.NO_BORDER;
            textCell.HorizontalAlignment = Element.ALIGN_CENTER;
            textCell.PaddingTop = 5;

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