# Thrive Church Official API
This API is primarily used by the Thrive Church Official App for serving users with content for both the Bible and our recorded / live sermons.

## Stack
- .NET 8
- MongoDB
- [ESV API](https://api.esv.org/)
- Docker (for debugging and deployment)

## API Documentation & Swagger UI
Visiting `~/swagger/index.html` in your browser will allow you to view the Swagger UI for the API and easily send API requests as well as view response objects.

## Caching
*Note: In general, the Caching that this application uses will expire thirty (30) seconds after an initial request. However, the persistent cache is set to expire after 48 hours from the initial request.*

If you wish to poll this API for whether or not the `LiveSermon` object is still active, the response will contain the Expiration Time (in **UTC**) for the `LiveSermon` object. Using this you will be able to gague how much longer the stream will be active. This will prevent having to poll the route in a loop to determine if right now the time has passed.

## Rate Limiting
The _Thrive Church Official API_ takes advantage of Rate Limiting on all requests, and will count rejected requests towards subsequent limits.

More information about how this API handles rate limiting can be found within [RateLimits.md](https://github.com/ThriveCommunityChurch/ThriveChurchOfficialAPI/blob/master/RateLimits.md)

## Docker Development Setup

The API supports Docker-based debugging in Visual Studio, allowing you to debug the application running in a Linux container.

### Prerequisites
- Docker Desktop for Windows
- Visual Studio with Docker support
- .NET 8 SDK

### Quick Start

1. **Open the solution** in Visual Studio
2. **Select "Docker" profile** from the debug dropdown
3. **Press F5** to start debugging

The API will be available at:
- **Swagger UI:** http://localhost:8080/swagger
- **API Base:** http://localhost:8080

### Configuration

#### Port Configuration
- **Host Port:** 8080
- **Container Port:** 8080
- **Mapping:** `8080:8080`

#### Environment Variables
The Docker debug configuration requires the same environment variables as local development. Visual Studio will automatically mount your source code and configuration files into the container.

**Required environment variables** (set via User Secrets or environment):
- `MongoConnectionString` - MongoDB connection string
- `JWT__SecretKey` - JWT secret key for authentication
- `JWT__Issuer` - JWT token issuer
- `JWT__Audience` - JWT token audience
- `S3__AccessKey` - AWS S3 access key (if using S3)
- `S3__SecretKey` - AWS S3 secret key (if using S3)

#### Docker Files
- **Dockerfile:** `API/ThriveChurchOfficialAPI/ThriveChurchOfficialAPI/Dockerfile`
- **Launch Settings:** `API/ThriveChurchOfficialAPI/ThriveChurchOfficialAPI/Properties/launchSettings.json`

### Debugging Features
- ✅ Full breakpoint support
- ✅ Hot reload enabled
- ✅ Source code mounted for live editing
- ✅ Visual Studio remote debugger (vsdbg)
- ✅ Swagger UI available during debugging

### Troubleshooting

**Port 8080 already in use?**
```powershell
# Check what's using port 8080
netstat -ano | findstr ":8080"

# Stop any Docker containers using the port
docker ps -a --filter "publish=8080" --format "{{.ID}}" | ForEach-Object { docker rm -f $_ }
```

**Container won't start?**
- Ensure Docker Desktop is running
- Check Docker logs: `docker logs ThriveChurchOfficialAPI`
- Verify environment variables are set correctly

**TIME_WAIT issues?**
Windows keeps ports in TIME_WAIT state for 120 seconds by default. You can reduce this to 30 seconds:
```powershell
# Requires admin privileges and reboot
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" `
    -Name "TcpTimedWaitDelay" -Value 30 -PropertyType DWord -Force
```

## Production Deployment (AWS App Runner)

The API is deployed to AWS App Runner via GitHub Actions CI/CD pipeline.

### Architecture
- **Container Registry:** Amazon ECR
- **Hosting:** AWS App Runner (auto-scaling, automatic HTTPS)
- **CI/CD:** GitHub Actions (`.github/workflows/deploy.yml`)

### Deployment Flow
1. Push to `master` branch triggers GitHub Actions
2. Docker image is built using `API/ThriveChurchOfficialAPI/Dockerfile`
3. Image is tagged with commit SHA and `latest`, then pushed to ECR
4. App Runner automatically deploys the new image

### Docker Files
| File | Purpose |
|------|---------|
| `API/ThriveChurchOfficialAPI/Dockerfile` | Production builds (CI/CD) |
| `API/ThriveChurchOfficialAPI/ThriveChurchOfficialAPI/Dockerfile` | Local debugging (Visual Studio) |

### GitHub Secrets Required
The following secrets must be configured in GitHub repository settings:

| Secret | Description |
|--------|-------------|
| `AWS_ACCESS_KEY_ID` | IAM user access key with ECR and App Runner permissions |
| `AWS_SECRET_ACCESS_KEY` | IAM user secret access key |

### App Runner Environment Variables
Configure these in the AWS App Runner console:

| Variable | Description |
|----------|-------------|
| `MongoConnectionString` | MongoDB Atlas connection string |
| `JWT__SecretKey` | JWT secret key (min 256 bits) |
| `JWT__Issuer` | JWT issuer (e.g., `ThriveChurchOfficialAPI`) |
| `JWT__Audience` | JWT audience (e.g., `ThriveChurchClients`) |
| `S3__BucketName` | AWS S3 bucket name |
| `S3__AccessKey` | AWS S3 access key |
| `S3__SecretKey` | AWS S3 secret key |
| `S3__Region` | AWS region (e.g., `us-east-2`) |
| `S3__BaseUrl` | S3 bucket base URL |
| `EsvApiKey` | ESV API key |
| `EmailPW` | Email password |

### First-Time Setup

1. **Create ECR Repository:**
   ```bash
   aws ecr create-repository --repository-name thrive-fl/thrivechurchofficialapi --region us-east-1
   ```

2. **Create IAM User** with policies:
   - `AmazonEC2ContainerRegistryPowerUser`
   - `AWSAppRunnerFullAccess`

3. **Add GitHub Secrets** (Settings → Secrets → Actions)

4. **Create App Runner Service:**
   - Source: Amazon ECR
   - Repository: Your ECR repository ARN
   - Deployment trigger: Automatic
   - Port: 8080
   - Add environment variables (see table above)

5. **Configure Custom Domain** (optional):
   - Add custom domain in App Runner console
   - Update DNS records in Route 53

## Contributing
The easiest way to get started is to use Docker debugging in Visual Studio (see Docker Development Setup above). Alternatively, you can run the API locally with .NET 8 SDK installed.

You will need to make sure you have the following settings in your `AppSettings.json`. 
  1. `EsvApiKey` - If you wish to connect to the ESV Api, you will need to request an Auth Token from [the ESV API website](https://api.esv.org/). Simply include this token without "Token" for this setting.
  2. `MongoConnectionString` - Use this for connecting to your MongoDB instance; should contain the prefix "mongodb://".
  3. `OverrideEsvApiKey` - Use this setting to override a check for your ESV API Key.
      - _Set to_ `"true"` _if you want to skip the ckeck for your API Key._
      - _Set to_ `"false"` _if you want to use the PassagesController and make requests. NOTE: If this setting is set to false and no API Key is found, the application will throw an exception._
  4. `IpRateLimiting` - Use this for setting your configurable Rate Limiting settings. See [RateLimits.md](https://github.com/ThriveCommunityChurch/ThriveChurchOfficialAPI/blob/master/RateLimits.md) for more information.
  5. `S3` - Use this if you'd like to store your audio files in an S3 bucket. This can store your AWS credentials, but its recommended to [in fact not do this](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html#update-access-keys) for long-term access (we'll likely change this later to improve this for all sensitive options in AppSettings.json).
     - `"BucketName": "bucket-name"`,
     - `"AccessKey": "AKIA..."`,
     - `"SecretKey": "..."`,
     - `"Region": "us-east-1"`,
     - `"BaseUrl": "https://bucket-name.s3.us-east-1.amazonaws.com"`,
     - `"MaxFileSizeMB": 50`,
     - `"AllowedExtensions": [ ".mp3" ]`
  6. `JWT` - **Required for Authentication** - Use this for configuring JWT token authentication for protected API endpoints. All non-GET endpoints require valid JWT authentication.
     - `"SecretKey": "YourSecureRandomKey256BitsLong"` - **CRITICAL**: Use a cryptographically secure random key in production (minimum 256 bits)
     - `"Issuer": "YourIssuer"` - Token issuer identifier
     - `"Audience": "YourAudience"` - Token audience identifier
     - `"ExpirationMinutes": 60` - JWT token expiration time in minutes
     - `"RefreshTokenExpirationDays": 7` - Refresh token expiration time in days

## Authentication & JWT Configuration

The API uses JWT (JSON Web Token) authentication for all endpoints that modify data (POST, PUT, DELETE). GET endpoints remain public for read-only access.

