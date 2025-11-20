using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModeloPrincipal.Entity;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiHerramientaWeb.Hubs
{
    // DTO para recibir la ubicación desde Flutter / frontend
    public class UbicacionPayload
    {
        // Puede venir como "TecnicoId" o como "Ideusr" (id de usuario enviado por el front)
        public int? TecnicoId { get; set; }
        public int? Ideusr { get; set; }

        // Acepta nombres comunes del frontend (case-insensitive durante la deserialización)
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class UbicacionHub : Hub
    {
        private readonly CVGEntities _context;
        private readonly ILogger<UbicacionHub> _logger;

        public UbicacionHub(CVGEntities context, ILogger<UbicacionHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Técnico envía ubicación como objeto JSON (puede enviar Ideusr o TecnicoId)
        public async Task EnviarUbicacionCliente(object payload)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                UbicacionPayload? ubicacion = null;

                switch (payload)
                {
                    case JsonElement jsonElement:
                        // viene como JSON bruto
                        ubicacion = JsonSerializer.Deserialize<UbicacionPayload>(jsonElement.GetRawText(), options);
                        break;
                    case string jsonString:
                        // viene como string JSON
                        ubicacion = JsonSerializer.Deserialize<UbicacionPayload>(jsonString, options);
                        break;
                    case UbicacionPayload dto:
                        ubicacion = dto;
                        break;
                }

                if (ubicacion == null)
                {
                    _logger.LogWarning("Payload inválido o nulo: {@payload}", payload);
                    return;
                }

                // Normalizar timestamp
                var timestamp = ubicacion.Timestamp == default ? DateTime.UtcNow : ubicacion.Timestamp;

                // Resolver Idetec: preferir TecnicoId si viene, sino buscar por Ideusr (iduser enviado por frontend)
                int idetec = ubicacion.TecnicoId.GetValueOrDefault(0);
                string? nombreTec = null;

                if (idetec <= 0 && ubicacion.Ideusr.GetValueOrDefault(0) > 0)
                {
                    var tecnico = await _context.Msttecs
                        .Where(t => t.Ideusr == ubicacion.Ideusr.Value)
                        .Select(t => new { t.Idetec, t.Nomtec })
                        .FirstOrDefaultAsync();

                    if (tecnico != null)
                    {
                        idetec = tecnico.Idetec;
                        nombreTec = tecnico.Nomtec;
                    }
                }

                if (idetec <= 0)
                {
                    _logger.LogWarning("No se pudo resolver Idetec (ni TecnicoId ni Ideusr válidos): {@payload}", payload);
                    return;
                }

                var data = new
                {
                    Ideusr = ubicacion.Ideusr,
                    Idetec = idetec,
                    Nombre = nombreTec,
                    Latitud = ubicacion.Latitude,
                    Longitud = ubicacion.Longitude,
                    Timestamp = timestamp
                };

                _logger.LogInformation(
                    "✅ Recibida ubicación - Ideusr: {Ideusr}, Idetec: {Idetec}: {Lat}, {Lon}",
                    ubicacion.Ideusr, idetec, ubicacion.Latitude, ubicacion.Longitude);

                await Clients.Group($"tecnico-{idetec}")
                    .SendAsync("RecibirUbicacion", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error procesando ubicación: {Payload}", payload);
            }
        }

        // Suscribirse a grupo de un técnico por Idetec (existente)
        public async Task SuscribirATecnico(string idetec)
        {
            if (!string.IsNullOrEmpty(idetec))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tecnico-{idetec}");
                _logger.LogInformation("Cliente {ConnectionId} suscrito al grupo tecnico-{Idetec}",
                    Context.ConnectionId, idetec);
            }
        }

        // Cancelar suscripción por Idetec (existente)
        public async Task DesuscribirDeTecnico(string idetec)
        {
            if (!string.IsNullOrEmpty(idetec))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tecnico-{idetec}");
                _logger.LogInformation("Cliente {ConnectionId} desuscrito del grupo tecnico-{Idetec}",
                    Context.ConnectionId, idetec);
            }
        }

        // Nuevo: Suscribirse usando Ideusr (id enviado por la app). El servidor resuelve Idetec y añade al grupo.
        public async Task SuscribirPorIdeusr(int ideusr)
        {
            if (ideusr <= 0)
            {
                _logger.LogWarning("Ideusr inválido: {Ideusr}", ideusr);
                return;
            }

            var idetec = await _context.Msttecs
                .Where(t => t.Ideusr == ideusr)
                .Select(t => t.Idetec)
                .FirstOrDefaultAsync();

            if (idetec > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tecnico-{idetec}");
                _logger.LogInformation("Cliente {ConnectionId} suscrito al grupo tecnico-{Idetec} (resuelto desde Ideusr {Ideusr})",
                    Context.ConnectionId, idetec, ideusr);
            }
            else
            {
                _logger.LogWarning("No se encontró Idetec para Ideusr {Ideusr}", ideusr);
            }
        }

        // Nuevo: Desuscribir usando Ideusr (resuelve Idetec y quita del grupo)
        public async Task DesuscribirPorIdeusr(int ideusr)
        {
            if (ideusr <= 0)
            {
                _logger.LogWarning("Ideusr inválido: {Ideusr}", ideusr);
                return;
            }

            var idetec = await _context.Msttecs
                .Where(t => t.Ideusr == ideusr)
                .Select(t => t.Idetec)
                .FirstOrDefaultAsync();

            if (idetec > 0)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tecnico-{idetec}");
                _logger.LogInformation("Cliente {ConnectionId} desuscrito del grupo tecnico-{Idetec} (resuelto desde Ideusr {Ideusr})",
                    Context.ConnectionId, idetec, ideusr);
            }
            else
            {
                _logger.LogWarning("No se encontró Idetec para Ideusr {Ideusr}", ideusr);
            }
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Cliente conectado: {ConnectionId}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation(exception, "Cliente desconectado: {ConnectionId}", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}