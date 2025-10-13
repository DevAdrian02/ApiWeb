using ApiHerramientaWeb.Modelos.Cobranza.Recibe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApiHerramientaWeb.Modelos.Cobranza.Recibo
{
    public static class GeneradorCuerpoCorreo
    {
        public static string GenerarCuerpoRecibo(RecibeEntregaRequest request)
        {
            string nombreUsuario = $"{request.User?.FirstName} {request.User?.LastName}".Trim();
            string fechaEntrega = request.Entrega.FCHENT.ToString("dd/MM/yyyy");
            string fechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            decimal totalServicios = request.Costos?.Sum(c => c.TotalServicio) ?? 0;
            decimal totalDenominaciones = request.Denominaciones?.Sum(d => d.Total) ?? 0;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            max-width: 700px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f8f9fa;
            color: #333;
        }}
        .container {{
            background-color: white;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            padding: 0;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
        }}
        .header {{
            background: linear-gradient(135deg, #2c3e50, #34495e);
            color: white;
            padding: 25px;
            text-align: center;
            border-radius: 8px 8px 0 0;
        }}
        .total-section {{
            background: linear-gradient(135deg, #e3f2fd, #f3f8ff);
            padding: 25px;
            text-align: center;
            border-bottom: 1px solid #e0e0e0;
        }}
        .total-label {{
            font-size: 16px;
            font-weight: 600;
            color: #2c3e50;
            margin-bottom: 8px;
        }}
        .total-amount {{
            font-size: 32px;
            font-weight: 700;
            color: #1976d2;
        }}
        .info-section {{
            padding: 25px;
        }}
        .info-grid {{
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 15px;
            margin-bottom: 25px;
        }}
        .info-item {{
            text-align: center;
            padding: 12px;
            background-color: #f8f9fa;
            border-radius: 6px;
            border: 1px solid #e9ecef;
        }}
        .info-label {{
            font-size: 12px;
            font-weight: 600;
            color: #6c757d;
            text-transform: uppercase;
            margin-bottom: 5px;
        }}
        .info-value {{
            font-size: 14px;
            font-weight: 600;
            color: #2c3e50;
        }}
        .content-grid {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 25px;
            margin-bottom: 25px;
        }}
        .section {{
            background-color: white;
            border: 1px solid #e9ecef;
            border-radius: 6px;
            padding: 20px;
        }}
        .section-title {{
            font-size: 16px;
            font-weight: 600;
            color: #2c3e50;
            text-align: center;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #e9ecef;
        }}
        .data-table {{
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
        }}
        .data-table th {{
            background-color: #495057;
            color: white;
            padding: 10px;
            text-align: left;
            font-weight: 600;
        }}
        .data-table td {{
            padding: 10px;
            border-bottom: 1px solid #e9ecef;
        }}
        .data-table tr:nth-child(even) {{
            background-color: #f8f9fa;
        }}
        .text-left {{ text-align: left; }}
        .text-center {{ text-align: center; }}
        .text-right {{ text-align: right; }}
        .total-row {{
            background-color: #e3f2fd !important;
            font-weight: 600;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #dee2e6;
            border-radius: 0 0 8px 8px;
            font-size: 12px;
            color: #6c757d;
        }}
        .status-badge {{
            display: inline-block;
            padding: 4px 12px;
            background-color: {(request.Diferencia == 0 ? "#28a745" : (request.Diferencia > 0 ? "#ffc107" : "#dc3545"))};
            color: white;
            border-radius: 20px;
            font-size: 11px;
            font-weight: 600;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0 0 5px 0; font-size: 24px;'>CASAVISION</h1>
            <p style='margin: 0; font-size: 16px; opacity: 0.9;'>COMPROBANTE DE ENTREGA COLECTORES</p>
        </div>

        <div class='total-section'>
            <div class='total-label'>TOTAL DE ENTREGA</div>
            <div class='total-amount'>{FormatCurrency(request.TotalCanceladas)}</div>
        </div>

        <div class='info-section'>
            <div class='info-grid'>
                <div class='info-item'>
                    <div class='info-label'>Nº Entrega</div>
                    <div class='info-value'>#{request.Entrega.IDEENTCOL}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>Fecha</div>
                    <div class='info-value'>{fechaEntrega}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>Entregado por</div>
                    <div class='info-value'>{nombreUsuario}</div>
                </div>
                <div class='info-item'>
                    <div class='info-label'>Recibido por</div>
                    <div class='info-value'>{request.Agente}</div>
                </div>
            </div>

            <div class='content-grid'>
                <!-- Columna 1: Arqueo -->
                <div class='section'>
                    <div class='section-title'>ARQUEO DE EFECTIVO</div>
                    <table class='data-table'>
                        <thead>
                            <tr>
                                <th class='text-left'>Denominación</th>
                                <th class='text-center'>Cantidad</th>
                                <th class='text-right'>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {GenerarFilasDenominaciones(request)}
                            <tr class='total-row'>
                                <td class='text-left'><strong>TOTAL ARQUEO</strong></td>
                                <td class='text-center'></td>
                                <td class='text-right'><strong>{FormatCurrency(totalDenominaciones)}</strong></td>
                            </tr>
                        </tbody>
                    </table>
                    <div style='margin-top: 15px; text-align: center;'>
                        <span class='status-badge'>
                            Diferencia: {FormatCurrency(Math.Abs(request.Diferencia))} 
                            {(request.Diferencia == 0 ? "CUADRADO" : (request.Diferencia > 0 ? "SOBRANTE" : "FALTANTE"))}
                        </span>
                    </div>
                </div>

                <!-- Columna 2: Servicios -->
                <div class='section'>
                    <div class='section-title'>SERVICIOS</div>
                    {(request.Costos != null && request.Costos.Any() ?
                    $@"<table class='data-table'>
                        <thead>
                            <tr>
                                <th class='text-left'>Descripción</th>
                                <th class='text-center'>Cantidad</th>
                                <th class='text-right'>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {GenerarFilasServicios(request)}
                            <tr class='total-row'>
                                <td class='text-left'><strong>TOTAL SERVICIOS</strong></td>
                                <td class='text-center'></td>
                                <td class='text-right'><strong>{FormatCurrency(totalServicios)}</strong></td>
                            </tr>
                        </tbody>
                    </table>" :
                    "<p style='text-align: center; color: #6c757d; margin: 20px 0;'>No hay servicios registrados</p>")}
                </div>
            </div>
        </div>

        <div class='footer'>
            <p style='margin: 0 0 10px 0;'><strong>Se adjunta PDF con el detalle completo de la entrega</strong></p>
            <p style='margin: 0 0 5px 0;'>Comprobante generado electrónicamente el {fechaActual}</p>
            <p style='margin: 0;'>Válido como comprobante de entrega oficial</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerarFilasDenominaciones(RecibeEntregaRequest request)
        {
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

            return string.Join("", denominacionesPredefinidas.Select(denom =>
            {
                var denominacionExistente = request.Denominaciones?.FirstOrDefault(d => d.Valor == denom.Valor);
                var cantidad = denominacionExistente?.Cantidad ?? 0;
                var total = denominacionExistente?.Total ?? 0;

                return $@"
                <tr>
                    <td class='text-left'>{denom.Nombre}</td>
                    <td class='text-center'>{cantidad.ToString("N0")}</td>
                    <td class='text-right'>{FormatCurrency(total)}</td>
                </tr>";
            }));
        }

        private static string GenerarFilasServicios(RecibeEntregaRequest request)
        {
            if (request.Costos == null || !request.Costos.Any())
                return "";

            return string.Join("", request.Costos.Select(costo => $@"
                <tr>
                    <td class='text-left'>{costo.Servicio}</td>
                    <td class='text-center'>{costo.CantidadFacturas.ToString("N0")}</td>
                    <td class='text-right'>{FormatCurrency(costo.TotalServicio)}</td>
                </tr>"));
        }

        private static string FormatCurrency(decimal value)
        {
            return $"C$ {value.ToString("N2", CultureInfo.InvariantCulture)}";
        }
    }
}