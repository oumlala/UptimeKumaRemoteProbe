﻿namespace UptimeKumaRemoteProbe.Services;

public class MonitorsService(ILogger<MonitorsService> logger, AppSettings appSettings)
{
    public async Task<List<Monitors>> GetMonitorsAsync()
    {
        try
        {
            using var socket = new SocketIOClient.SocketIO(appSettings.Url, new SocketIOClient.SocketIOOptions
            {
                ReconnectionAttempts = 3
            });

            var data = new
            {
                username = appSettings.Username,
                password = appSettings.Password,
                token = ""
            };

            JsonElement monitorsRaw = new();

            socket.On("monitorList", response =>
            {
                monitorsRaw = response.GetValue<JsonElement>();
            });

            socket.OnConnected += async (sender, e) =>
            {
                await socket.EmitAsync("login", (ack) =>
                {
                    var result = JsonNode.Parse(ack.GetValue<JsonElement>(0).ToString());
                    if (result["ok"].ToString() != "true")
                    {
                        logger.LogError("Uptime Kuma login failure");
                    }
                }, data);
            };

            await socket.ConnectAsync();

            int round = 0;
            while (monitorsRaw.ValueKind == JsonValueKind.Undefined)
            {
                round++;
                await Task.Delay(1000);
                if (round >= 10) break;
            }

            await socket.DisconnectAsync();
            var monitors = JsonSerializer.Deserialize<Dictionary<string, Monitors>>(monitorsRaw);
            return monitors.Values.ToList();
        }
        catch
        {
            logger.LogError("Error trying to get monitors");
            return null;
        }
    }
}
