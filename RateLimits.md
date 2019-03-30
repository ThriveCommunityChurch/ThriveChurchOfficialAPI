## Rate Limiting
The _Thrive Church Official API_ uses rate limiting to prevent clients from generating too many requests within a specified time frame.

The following limits are categorized by timeframe, and will be enforced by a Http Status code of [429](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429).

### 1 Second
Any client that requests **more than 2** requests in under 1 second will be denied.

### 1 Minute
Any client that requests **more than 100** requests in under 1 minute will be denied.

### 15 Minutes
Any client that requests **more than 300** requests in under 15 minutes will be denied.

### 1 Hour
Any client that requests **more than 500** requests in under 1 hour will be denied.

### 1 Day
Any client that requests **more than 1000** requests in under 1 day will be denied.
