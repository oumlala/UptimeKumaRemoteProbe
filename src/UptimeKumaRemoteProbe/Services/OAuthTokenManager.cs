namespace UptimeKumaRemoteProbe.Services;

public class OAuthTokenManager
{
    private readonly OAuthSettings _settings;
    private string _currentToken;
    private DateTime _tokenExpiration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OAuthTokenManager> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public OAuthTokenManager(OAuthSettings settings, HttpClient httpClient, ILogger<OAuthTokenManager> logger)
    {
        _settings = settings;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetValidTokenAsync()
    {
        if (string.IsNullOrEmpty(_currentToken) || DateTime.UtcNow >= _tokenExpiration)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (string.IsNullOrEmpty(_currentToken) || DateTime.UtcNow >= _tokenExpiration)
                {
                    await RefreshTokenAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        return _currentToken;
    }

    private async Task RefreshTokenAsync()
    {
        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = _settings.GrantType,
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["scope"] = _settings.Scope
            });

            var response = await _httpClient.PostAsync(_settings.TokenUrl, content);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            _currentToken = tokenResponse.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 1 minute before expiration
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh OAuth token");
            throw;
        }
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
