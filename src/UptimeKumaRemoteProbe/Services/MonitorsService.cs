namespace UptimeKumaRemoteProbe.Services;

public class MonitorsService
{
    private readonly ILogger<MonitorsService> _logger;
    private readonly AppSettings _appSettings;
    private readonly OAuthTokenManager? _oAuthTokenManager;

    public MonitorsService(ILogger<MonitorsService> logger, AppSettings appSettings, OAuthTokenManager? oAuthTokenManager = null)
    {
        _logger = logger;
        _appSettings = appSettings;
        _oAuthTokenManager = oAuthTokenManager;
    }

    public async Task<List<Monitors>> GetMonitorsAsync()
    {
        try
        {
            using var socket = new SocketIOClient.SocketIO(_appSettings.Url, new SocketIOClient.SocketIOOptions
            {
                ReconnectionAttempts = 3
            });

            string authToken = _appSettings.Token ?? "";

            // Check if OAuth is configured and get OAuth token
            if (_oAuthTokenManager != null && !string.IsNullOrEmpty(_appSettings.OAuth?.ClientId))
            {
                try
                {
                    authToken = await _oAuthTokenManager.GetValidTokenAsync();
                    _logger.LogInformation("Using OAuth authentication");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get OAuth token, falling back to configured authentication");
                }
            }

            var data = new
            {
                username = string.IsNullOrEmpty(authToken) ? _appSettings.Username : "",
                password = string.IsNullOrEmpty(authToken) ? _appSettings.Password : "",
                token = authToken
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
                        _logger.LogError("Uptime Kuma authentication failure");
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
            _logger.LogError("Error trying to get monitors");
            return null;
        }
    }
}
