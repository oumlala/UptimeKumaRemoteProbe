[![Publish](https://github.com/zimbres/UptimeKumaRemoteProbe/actions/workflows/dotnet.yml/badge.svg?event=release)](https://github.com/zimbres/UptimeKumaRemoteProbe/actions/workflows/dotnet.yml) [![.CodeQL](https://github.com/zimbres/UptimeKumaRemoteProbe/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/zimbres/UptimeKumaRemoteProbe/actions/workflows/codeql-analysis.yml)

# Uptime Kuma Remote Probe / Push Agent

>Uptime Kuma repository https://github.com/louislam/uptime-kuma
>
>Pre built container https://hub.docker.com/r/zimbres/uptime-kuma-remote-probe

---
## Configuration

### Remote Probe Configuration

Services configuration is done by editing the file **appsettings.json** and restarting application.

:warning: ***Please be informed that accounts with 2FA are not supported.***

#### Example:
```json
{
    "Configurations": {
    "Url": "http://192.168.100.190:3001/",
    "Token": "REPLACE_WITH_YOUR_UPTIME_KUMA_TOKEN",
    "Username": "admin",
    "Password": "Admin123",
    "ProbeName": "Home",
    "UpDependency": "192.168.1.1",
    "Timeout": 1000,
    "Delay": 60000,
    "ConnectionStrings": {
      "PGSQL": "Host=localhost;Database=postgres;Username=postgres;Password=postgres"
    },
    "WhoisApiUrl": "https://whoisjson.com/api/v1/whois?domain=keep.this&format=json",
    "WhoisApiToken": "whoisjsonApiToken"
    }
}
```
>- **Token**: If set the probe will use it to authenticate and if not it falls back to Username/Password.
>- **UpDependency**: Should be a trustable IP in your network, your ISP gateway for example. In case of this IP is not available, no other checks will be executed.
>- **Delay**: is the delay time between checks. It is expressed in milliseconds, in this example 1 minute = 60000 ms between each round.
>- **ProbeName**: Will help the probe identify the monitors that should be fetched from UK instance.
>- **WhoisApiToken**: 
>   - You need an account registered on [Whois Json](https://whoisjson.com/) if you want Domain check. 
>   - This service has a api call limit of 500 per month, which would be enough since this check will run once a day only, or at the probe restart. 
>   - By default if the domain expiration date is < 30 days, probe will not push to UK and generate an alert.
---

### Monitors configuration 

**Please Note** : From version > 3.0 services to be executed on the probe will be auto discovered by tags set in UK.

:warning: ***Tags and Values are case sensitive.***

|       Tag Name        | Tag Value Example |                           Description                                     |
|-----------------------|-------------------|---------------------------------------------------------------------------|
| Probe                 | House             | Must match the value set in appsettings.json "Configurations.ProbeName"   |
| Type                  | Ping              | Type of check to perform                                                  |
| Address               | 1.1.1.1           | Target address to check                                                   |
| Domain                | domain.com        | Domain name for DNS/certificate checks                                    |
| Method                | GET               | HTTP method for web checks                                                |
| CertificateExpiration | 7                 | Days before certificate expiration warning                                |
| IgnoreSSL             | False             | Whether to bypass SSL verification                                        |
| Keyword               | Success           | Need to be set when Type=Http if you want to detect specific keyword      |

![image](https://github.com/zimbres/UptimeKumaRemoteProbe/assets/29772043/a4a9fd07-4f33-4f4f-9c27-24b59be42b28)

#### Available monitor types are:
- **Ping**
- **Http**
- **Tcp**
- **Certificate**
- **Database**
- **Domain**

---

## OAuth 2.0 Configuration

The application now supports OAuth 2.0 authentication to secure communications with the central Uptime Kuma instance.

### Configuration in appsettings.json

```json
{
  "Configurations": {
    "OAuth": {
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "TokenUrl": "https://your-auth-server/oauth/token",
      "Scope": "monitor:write",
      "GrantType": "client_credentials"
    }
  }
}
```

### Environment Variables

OAuth parameters can also be configured using environment variables:

- `OAuthClientId`: OAuth client ID
- `OAuthClientSecret`: OAuth client secret
- `OAuthTokenUrl`: Authorization server URL
- `OAuthScope`: Permission scope (optional)
- `OAuthGrantType`: OAuth grant type (default: "client_credentials")

### Security

- Never store OAuth secrets directly in code or version control
- Use environment variables or secure secrets in production
- Regularly check the validity and rotation of your OAuth credentials

### Authentication Flow

1. The probe requests an access token using client credentials
2. The token is cached and automatically renewed before expiration
3. All API requests include the Bearer token in the Authorization header
4. Failed authentication attempts are automatically retried with a new token

For more details about OAuth implementation, please refer to the [OAuth 2.0 specification](https://oauth.net/2/).

---

Pré compiled package is available for Windows and Linux. It requires .Net Runtime 9.x.

[Download .NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
