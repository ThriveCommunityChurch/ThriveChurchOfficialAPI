# Rate Limiting
The _Thrive Church Official API_ uses rate limiting based on client IP to prevent clients from generating too many requests within a specified time frame.

The following limits are categorized by timeframe, and will be enforced by a Http Status code of [429](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429).

### 1 Minute
Any client that makes **more than 100** requests in under 1 minute will be denied.

### 15 Minutes
Any client that makes **more than 300** requests in under 15 minutes will be denied.

### 1 Hour
Any client that makes **more than 500** requests in under 1 hour will be denied.

### 1 Day
Any client that makes **more than 1000** requests in under 1 day will be denied.

## Example Configuration
Within your _appsettings.json_ you will need the following configuration.

You can adjust the settings within this object to suit your needs. To enforce the rules for ALL clients, simply remove the elements from the `WhiteList` arrays, so they remain empty. 
```json
"IpRateLimiting": {
  "EnableEndpointRateLimiting": false,
  "StackBlockedRequests": true,
  "RealIpHeader": "X-Real-IP",
  "ClientIdHeader": "X-ClientId",
  "HttpStatusCode": 429,
  "IpWhitelist": [ "127.0.0.1", "::1/10", "192.168.0.0/24" ],
  "EndpointWhitelist": [ "get:/api/license", "*:/api/status" ],
  "ClientWhitelist": [ "dev-id-1", "dev-id-2" ],
  "GeneralRules": [
    {
      "Endpoint": "*",
      "Period": "15m",
      "Limit": 100
    },
    {
      "Endpoint": "*",
      "Period": "12h",
      "Limit": 1000
    },
    {
      "Endpoint": "*",
      "Period": "7d",
      "Limit": 10000
    }
  ]
}
```
