# Monitoring and Alerting

This document describes the comprehensive monitoring and alerting features implemented in the Receipt App API.

## Overview

The application implements health checks for all critical components and dependencies, providing detailed status information and enabling proactive monitoring and alerting.

## Health Check Endpoints

### 1. Complete Health Check (`/health`)

Returns comprehensive health status of all system components.

**Request:**
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": null,
      "duration": 45.2,
      "data": {},
      "tags": ["db", "sql"]
    },
    {
      "name": "openai",
      "status": "Healthy",
      "description": "OpenAI API is accessible and API key is valid.",
      "duration": 234.1,
      "data": {},
      "tags": ["external", "ocr"]
    },
    {
      "name": "smtp",
      "status": "Healthy",
      "description": "SMTP server smtp.gmail.com:587 is accessible.",
      "duration": 123.5,
      "data": {},
      "tags": ["external", "email"]
    },
    {
      "name": "twilio",
      "status": "Degraded",
      "description": "Twilio credentials not configured. SMS features will not work.",
      "duration": 1.2,
      "data": {},
      "tags": ["external", "sms"]
    },
    {
      "name": "filestorage",
      "status": "Healthy",
      "description": "File storage healthy. Available space: 125.43 GB",
      "duration": 8.7,
      "data": {
        "uploadPath": "E:\\dev\\WarrantyApp\\MyAspireSolution\\MyApi\\uploads\\receipts",
        "availableSpaceGB": 125.43
      },
      "tags": ["storage"]
    }
  ],
  "totalDuration": 412.7
}
```

**Health Status Values:**
- `Healthy`: All systems operational
- `Degraded`: Service operational but with issues (non-critical warnings)
- `Unhealthy`: Service failure detected

### 2. Readiness Check (`/health/ready`)

Checks if the application is ready to serve requests (database and storage only).

**Request:**
```http
GET /health/ready
```

**Response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": null,
      "duration": 42.1
    },
    {
      "name": "filestorage",
      "status": "Healthy",
      "description": "File storage healthy. Available space: 125.43 GB",
      "duration": 6.3
    }
  ]
}
```

**Use Case:** 
- Kubernetes readiness probes
- Load balancer health checks
- Determines if instance should receive traffic

### 3. Liveness Check (`/health/live`)

Basic ping endpoint to verify the application is running.

**Request:**
```http
GET /health/live
```

**Response:**
```
HTTP/1.1 200 OK
Healthy
```

**Use Case:**
- Kubernetes liveness probes
- Simple uptime monitoring
- Container orchestration health checks

## Health Check Components

### 1. Database Health Check

**Component:** `ApplicationDbContext` (Entity Framework Core)

**Checks:**
- Database connectivity
- Query execution capability
- Connection pool status

**Status Conditions:**
- ✅ **Healthy**: Database connection successful, queries execute normally
- ❌ **Unhealthy**: Cannot connect to database or queries fail

**Tags:** `db`, `sql`

### 2. OpenAI Health Check

**Component:** `OpenAiHealthCheck`

**Checks:**
- API key configuration
- API key format validation
- OpenAI API connectivity
- API authentication

**Status Conditions:**
- ✅ **Healthy**: API key valid, OpenAI API accessible
- ⚠️ **Degraded**: API key not configured, timeout connecting to API
- ❌ **Unhealthy**: Invalid API key or authentication failure

**Tags:** `external`, `ocr`

**Configuration:**
```json
{
  "OpenAI": {
    "ApiKey": "sk-..."
  }
}
```

### 3. SMTP Health Check

**Component:** `SmtpHealthCheck`

**Checks:**
- SMTP configuration validation
- SMTP server connectivity (TCP)
- Port accessibility

**Status Conditions:**
- ✅ **Healthy**: SMTP server accessible
- ⚠️ **Degraded**: SMTP not configured, connection timeout
- ❌ **Unhealthy**: Invalid configuration, connection failure

**Tags:** `external`, `email`

**Configuration:**
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587
  }
}
```

### 4. Twilio Health Check

**Component:** `TwilioHealthCheck`

**Checks:**
- Twilio credentials configuration
- Twilio API connectivity
- Account authentication

**Status Conditions:**
- ✅ **Healthy**: Credentials valid, Twilio API accessible
- ⚠️ **Degraded**: Credentials not configured, timeout connecting to API
- ❌ **Unhealthy**: Invalid credentials or authentication failure

**Tags:** `external`, `sms`

**Configuration:**
```json
{
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "..."
  }
}
```

### 5. File Storage Health Check

**Component:** `FileStorageHealthCheck`

**Checks:**
- Upload directory existence
- Write permissions
- Available disk space

**Status Conditions:**
- ✅ **Healthy**: Directory accessible, sufficient space (>1 GB)
- ⚠️ **Degraded**: Low disk space (<1 GB)
- ❌ **Unhealthy**: Directory doesn't exist, no write permissions

**Tags:** `storage`

**Data Returned:**
```json
{
  "uploadPath": "E:\\path\\to\\uploads",
  "availableSpaceGB": 125.43
}
```

## Monitoring Integration

### Aspire Dashboard

Health checks are automatically integrated with .NET Aspire Dashboard:

1. Start the application via AppHost
2. Open Aspire Dashboard
3. Navigate to "Resources" section
4. View real-time health status of all services

### Kubernetes Integration

Configure Kubernetes probes in deployment manifest:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: receipt-app-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: receipt-app-api:latest
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
```

### Load Balancer Health Checks

Configure load balancer (e.g., AWS ALB, Azure App Gateway):

- **Health Check Path:** `/health/ready`
- **Interval:** 30 seconds
- **Timeout:** 5 seconds
- **Unhealthy Threshold:** 2 consecutive failures
- **Healthy Threshold:** 2 consecutive successes

### Application Insights

Health check results are automatically logged and can be queried:

```kusto
requests
| where name contains "/health"
| summarize count(), avg(duration) by name, resultCode
| order by avg_duration desc
```

## Alerting Scenarios

### Critical Alerts (Immediate Action Required)

1. **Database Unhealthy**
   - **Trigger:** Database health check returns `Unhealthy`
   - **Impact:** Application cannot serve requests
   - **Action:** Check database connection, verify credentials, check SQL Server container/service

2. **File Storage Unhealthy**
   - **Trigger:** File storage health check returns `Unhealthy`
   - **Impact:** Cannot upload/download receipts
   - **Action:** Verify upload directory permissions, check disk space

### Warning Alerts (Action Needed Soon)

1. **Low Disk Space**
   - **Trigger:** Available space < 1 GB
   - **Impact:** Receipt uploads may fail soon
   - **Action:** Clean up old files, expand disk

2. **External Service Degraded**
   - **Trigger:** OpenAI/SMTP/Twilio returns `Degraded`
   - **Impact:** Features not working (OCR, email, SMS)
   - **Action:** Verify API keys/credentials, check service status

3. **OpenAI API Slow**
   - **Trigger:** OpenAI health check duration > 5 seconds
   - **Impact:** Slow OCR processing
   - **Action:** Monitor OpenAI service status

### Monitoring Best Practices

1. **Poll `/health` endpoint every 60 seconds**
2. **Alert on status change to `Unhealthy`**
3. **Create warning for `Degraded` status lasting > 5 minutes**
4. **Track health check duration trends**
5. **Monitor `/health/ready` for service availability**

## Health Check Response Codes

| Endpoint | Healthy | Degraded | Unhealthy |
|----------|---------|----------|-----------|
| `/health` | 200 OK | 200 OK | 503 Service Unavailable |
| `/health/ready` | 200 OK | 200 OK | 503 Service Unavailable |
| `/health/live` | 200 OK | N/A | N/A |

## Configuration

Health checks are automatically configured in `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        tags: new[] { "db", "sql" })
    .AddCheck<OpenAiHealthCheck>(
        name: "openai",
        tags: new[] { "external", "ocr" })
    .AddCheck<SmtpHealthCheck>(
        name: "smtp",
        tags: new[] { "external", "email" })
    .AddCheck<TwilioHealthCheck>(
        name: "twilio",
        tags: new[] { "external", "sms" })
    .AddCheck<FileStorageHealthCheck>(
        name: "filestorage",
        tags: new[] { "storage" });
```

## Testing Health Checks

### Using cURL

```bash
# Complete health check
curl https://localhost:7156/health | jq

# Readiness check
curl https://localhost:7156/health/ready | jq

# Liveness check
curl https://localhost:7156/health/live
```

### Using PowerShell

```powershell
# Complete health check
Invoke-RestMethod -Uri "https://localhost:7156/health" | ConvertTo-Json -Depth 10

# Readiness check
Invoke-RestMethod -Uri "https://localhost:7156/health/ready" | ConvertTo-Json -Depth 10

# Liveness check
Invoke-WebRequest -Uri "https://localhost:7156/health/live"
```

### Using Swagger

Navigate to Swagger UI and test health endpoints (note: health endpoints are not documented in Swagger by default).

## Troubleshooting

### Issue: All Health Checks Return Degraded

**Cause:** External services (OpenAI, SMTP, Twilio) not configured

**Solution:** Configure at least one external service or update alerting to ignore degraded status for unconfigured services

### Issue: Database Health Check Fails

**Cause:** Database not accessible

**Solutions:**
1. Verify SQL Server container is running: `docker ps`
2. Check connection string in configuration
3. Verify database migrations applied: `dotnet ef database update`

### Issue: File Storage Health Check Fails

**Cause:** Upload directory doesn't exist or no write permissions

**Solutions:**
1. Verify directory exists: Check `uploads/receipts` folder
2. Check permissions: Application needs read/write access
3. Check available disk space

## Performance Considerations

- Health checks run with 5-second timeout
- Database health check uses EF Core's built-in check (lightweight)
- External service checks use TCP/HTTP connectivity (no heavy operations)
- File storage check creates temporary file (minimal disk I/O)
- Total health check duration typically < 500ms

## Future Enhancements

- [ ] Add Redis cache health check (if Redis is added)
- [ ] Add custom metrics (requests per second, error rate)
- [ ] Add distributed tracing correlation
- [ ] Add health check UI dashboard
- [ ] Add Prometheus metrics endpoint
- [ ] Add historical health check data storage
- [ ] Add automated remediation for common issues
