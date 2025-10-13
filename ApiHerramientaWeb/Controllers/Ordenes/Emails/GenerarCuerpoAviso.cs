using System.Globalization;

namespace ApiHerramientaWeb.Controllers.Ordenes.Emails
{

    public static class GenerarCuerpoAviso
    {

        public static string GenerarCuerpoAvisoVisita(string cliente, string nombreColector)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Aviso de Visita</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }}
        
        body {{
            background-color: #f5f7fa;
            padding: 20px;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
        }}
        
        .header {{
            background: linear-gradient(135deg, #005baa 0%, #003d7a 100%);
            padding: 30px 20px;
            text-align: center;
            position: relative;
        }}
        
        .logo {{
            width: 120px;
            margin-bottom: 15px;
            filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
        }}
        
        .header h1 {{
            color: white;
            font-size: 26px;
            font-weight: 600;
            margin: 10px 0;
            letter-spacing: 0.5px;
        }}
        
        .header-icon {{
            position: absolute;
            top: 20px;
            right: 20px;
            width: 50px;
            height: 50px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        
        .content {{
            padding: 40px 30px;
            color: #333333;
            line-height: 1.6;
        }}
        
        .greeting {{
            font-size: 18px;
            margin-bottom: 25px;
            color: #2d3748;
        }}
        
        .message-box {{
            background: #f8f9ff;
            border-left: 4px solid #005baa;
            padding: 20px;
            border-radius: 0 8px 8px 0;
            margin: 25px 0;
        }}
        
        .highlight {{
            color: #005baa;
            font-weight: 600;
        }}
        
        .collector-card {{
            background: linear-gradient(to right, #f8f9ff, #e6eeff);
            border-radius: 10px;
            padding: 20px;
            margin: 30px 0;
            text-align: center;
            border: 1px solid #e0e7ff;
        }}
        
        .collector-name {{
            font-size: 22px;
            font-weight: 700;
            color: #003d7a;
            margin: 10px 0;
        }}
        
        .badge {{
            display: inline-block;
            background: #005baa;
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            margin-top: 10px;
        }}
        
        .instructions {{
            margin: 25px 0;
            padding: 20px;
            background: #fff9e6;
            border-radius: 8px;
            border-left: 4px solid #ffb300;
        }}
        
        .instructions h3 {{
            color: #e6a100;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            gap: 8px;
        }}
        
        .footer {{
            background: #f0f4f9;
            padding: 25px;
            text-align: center;
            color: #5a6c85;
            font-size: 14px;
            border-top: 1px solid #e2e8f0;
        }}
        
        .contact-info {{
            margin: 15px 0;
            display: flex;
            justify-content: center;
            gap: 25px;
            flex-wrap: wrap;
        }}
        
        .contact-item {{
            display: flex;
            align-items: center;
            gap: 8px;
        }}
        
        .copyright {{
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #e2e8f0;
            color: #7b8a9c;
        }}
        
        @media (max-width: 600px) {{
            .content {{
                padding: 25px 20px;
            }}
            
            .header h1 {{
                font-size: 22px;
            }}
            
            .collector-card {{
                padding: 15px;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='header-icon'>
                <svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                    <path d='M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z'></path>
                    <circle cx='12' cy='10' r='3'></circle>
                </svg>
            </div>
            <img src='https://iili.io/FWBGKru.md.png' alt='Casavision' class='logo'>
            <h1>Visita Programada</h1>
        </div>
        
        <div class='content'>
            <p class='greeting'>Estimado(a) <span class='highlight'>{cliente}</span>,</p>
            
            <p>Le informamos que nuestro colector se dirigirá a su domicilio en el transcurso del día para realizar la gestión correspondiente.</p>
            
            <div class='message-box'>
                <p>Por favor, esté atento a nuestra visita. El colector estará en su zona durante <strong>todo el día</strong> y se presentará debidamente identificado.</p>
            </div>
            
            <div class='collector-card'>
                <h3>Su colector asignado es:</h3>
                <div class='collector-name'>{nombreColector}</div>
                <div class='badge'>Colector Certificado</div>
            </div>
            
            <div class='instructions'>
                <h3>
                    <svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 24 24' fill='none' stroke='#e6a100' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <circle cx='12' cy='12' r='10'></circle>
                        <line x1='12' y1='8' x2='12' y2='12'></line>
                        <line x1='12' y1='16' x2='12.01' y2='16'></line>
                    </svg>
                    Para agilizar el proceso:
                </h3>
                <ul style='padding-left: 20px;'>
                    <li>Tenga a mano su identificación personal</li>
                    <li>Prepare los documentos relacionados con su servicio</li>
                    <li>Manténgase disponible en el horario de 8:00 am a 6:00 pm</li>
                </ul>
            </div>
            
            <p style='margin-top: 25px;'>Si necesita reprogramar la visita o tiene alguna consulta, no dude en contactarnos.</p>
        </div>
        
        <div class='footer'>
            <div class='contact-info'>
                <div class='contact-item'>
                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='#5a6c85' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <path d='M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z'></path>
                        <polyline points='22,6 12,13 2,6'></polyline>
                    </svg>
                    https://casavision.com/
                </div>
                <div class='contact-item'>
                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='#5a6c85' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <path d='M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z'></path>
                    </svg>
                    +505 8241 2272
                </div>
                <div class='contact-item'>
                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='#5a6c85' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <path d='M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z'></path>
                        <circle cx='12' cy='10' r='3'></circle>
                    </svg>
                    Managua, Ciudad sandino
                </div>
            </div>
            
            <div class='copyright'>
                Casavision S.A &copy; {DateTime.Now.Year} - Todos los derechos reservados
            </div>
        </div>
    </div>
</body>
</html>";
        }
        public static string GenerarCuerpoAvisoCobro(
            string cliente,
            string noContrato,
            string sector,
            string mesFactura,
            decimal monto,
            string factura = "")
        {
            // Formatear el monto como moneda
            string montoFormateado = monto.ToString("C2", new CultureInfo("es-NI"));

            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Aviso de Cobro</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }}
        
        body {{
            background-color: #f5f7fa;
            padding: 20px;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
        }}
        
        .header {{
            background: linear-gradient(135deg, #005baa 0%, #003d7a 100%);
            padding: 20px;
            text-align: center;
            position: relative;
        }}
        
        .logo {{
            width: 120px;
            margin-bottom: 10px;
            filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.2));
        }}
        
        .header h1 {{
            color: white;
            font-size: 26px;
            font-weight: 600;
            margin: 5px 0;
            letter-spacing: 0.5px;
        }}

        .aviso-title {{
            text-align: center;
            padding: 15px 0;
            background: #eef5ff;
            font-size: 22px;
            font-weight: 700;
            color: #003d7a;
            border-bottom: 2px solid #005baa;
        }}
        
        .data-container {{
            padding: 25px;
        }}
        
        .data-row {{
            display: flex;
            margin-bottom: 15px;
            border-bottom: 1px solid #eee;
            padding-bottom: 10px;
        }}
        
        .data-label {{
            font-weight: 600;
            color: #003d7a;
            width: 40%;
            min-width: 120px;
        }}
        
        .data-value {{
            flex-grow: 1;
            color: #333;
        }}
        
        .monto-box {{
            background: #f8f9ff;
            border: 2px solid #005baa;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            margin: 25px 0;
        }}
        
        .monto-label {{
            font-size: 18px;
            color: #5a6c85;
        }}
        
        .monto-value {{
            font-size: 32px;
            font-weight: 700;
            color: #d32f2f;
            margin-top: 10px;
        }}
        
        .advertencia {{
            background: #fff8e1;
            border-left: 4px solid #ffb300;
            padding: 15px;
            margin: 20px 0;
            border-radius: 0 4px 4px 0;
        }}
        
        .footer {{
            background: #f0f4f9;
            padding: 25px;
            text-align: center;
            color: #5a6c85;
            font-size: 14px;
            border-top: 1px solid #e2e8f0;
        }}
        
        .contact-info {{
            margin: 15px 0;
            display: flex;
            justify-content: center;
            gap: 25px;
            flex-wrap: wrap;
        }}
        
        .contact-item {{
            display: flex;
            align-items: center;
            gap: 8px;
        }}
        
        .copyright {{
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #e2e8f0;
            color: #7b8a9c;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <img src='https://iili.io/FWBGKru.md.png' alt='Casavision' class='logo'>
            <h1>Aviso de Cobro</h1>
        </div>
        
        <div class='aviso-title'>AVISO DE COBRO</div>
        
        <div class='data-container'>
            <div class='data-row'>
                <div class='data-label'>Cliente:</div>
                <div class='data-value'>{cliente}</div>
            </div>
            
            <div class='data-row'>
                <div class='data-label'>No. Contrato:</div>
                <div class='data-value'>{noContrato}</div>
            </div>
            
            <div class='data-row'>
                <div class='data-label'>Sector:</div>
                <div class='data-value'>{sector}</div>
            </div>
            
            {(string.IsNullOrEmpty(factura) ? "" : $@"
            <div class='data-row'>
                <div class='data-label'>Factura:</div>
                <div class='data-value'>{factura}</div>
            </div>")}
            
            <div class='data-row'>
                <div class='data-label'>Mes Factura:</div>
                <div class='data-value'>{mesFactura}</div>
            </div>
            
            <div class='monto-box'>
                <div class='monto-label'>Monto a Pagar:</div>
                <div class='monto-value'>{montoFormateado}</div>
            </div>
            
            <div class='advertencia'>
                <strong>RECUERDE PAGAR A TIEMPO SU FACTURA</strong><br>
                Este documento no es válido como factura
            </div>
            
            <p style='margin-top: 20px; text-align: center;'>
                Para realizar el pago o consultar detalles adicionales, contacte a nuestro departamento de cobros.
            </p>
        </div>
        
        <div class='footer'>
            <div class='contact-info'>
                <div class='contact-item'>
                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='#5a6c85' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <path d='M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z'></path>
                        <polyline points='22,6 12,13 2,6'></polyline>
                    </svg>
                    https://casavision.com/
                </div>
                <div class='contact-item'>
                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='#5a6c85' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <path d='M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z'></path>
                    </svg>
                    +505 8241 2272
                </div>
                <div class='contact-item'>
                    <svg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='#5a6c85' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                        <path d='M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z'></path>
                        <circle cx='12' cy='10' r='3'></circle>
                    </svg>
                    Managua, Ciudad sandino
                </div>
            </div>
            
            <div class='copyright'>
                Casavision S.A &copy; {DateTime.Now.Year} - Todos los derechos reservados
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }


}
