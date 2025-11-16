# Automated Deployment - Implementation Guide

## Overview

This document provides a comprehensive guide for setting up automated deployment for the Warranty Management System. The system uses .NET Aspire for orchestration and can be deployed to multiple cloud platforms.

## Deployment Options

### Option 1: Azure Container Apps (Recommended for Aspire)

Azure Container Apps provides native support for .NET Aspire applications with:
- Automatic container orchestration
- Built-in scaling and load balancing
- Azure SQL Database integration
- Managed identities for secrets
- Easy Aspire dashboard deployment

### Option 2: Azure App Service

Traditional Azure App Service deployment with:
- Direct .NET 8 runtime support
- Azure SQL Database connection
- Application Insights integration
- Deployment slots for blue-green deployment

### Option 3: Docker + Any Cloud Platform

Containerized deployment for flexibility:
- Build Docker images from the application
- Deploy to AWS ECS, Google Cloud Run, or any Kubernetes cluster
- Use managed databases from cloud providers
- Maximum portability

### Option 4: On-Premises with Docker Compose

Self-hosted deployment with:
- Docker Compose for service orchestration
- SQL Server in container
- Nginx reverse proxy
- SSL with Let's Encrypt

## Prerequisites

### Azure Deployment
- Azure subscription
- Azure CLI installed (`az login`)
- Resource group created
- Service principal for GitHub Actions

### Docker Deployment
- Docker Desktop or Docker Engine
- Docker Hub or Azure Container Registry account
- SSH access to deployment server (for on-premises)

### GitHub Secrets Required

For Azure deployment, configure these secrets in GitHub repository settings:

```
AZURE_CREDENTIALS          # Service principal JSON
AZURE_SUBSCRIPTION_ID      # Azure subscription ID
AZURE_RESOURCE_GROUP       # Resource group name
ACR_LOGIN_SERVER           # Azure Container Registry URL
ACR_USERNAME               # ACR username
ACR_PASSWORD               # ACR password
SQL_CONNECTION_STRING      # Azure SQL connection string (encrypted)
OPENAI_API_KEY            # OpenAI API key
SMTP_HOST                 # Email SMTP host
SMTP_PORT                 # Email SMTP port  
SMTP_USERNAME             # Email account
SMTP_PASSWORD             # Email password
TWILIO_ACCOUNT_SID        # Twilio account SID
TWILIO_AUTH_TOKEN         # Twilio auth token
TWILIO_PHONE_NUMBER       # Twilio phone number
JWT_SECRET                # JWT signing key (strong random string)
```

## GitHub Actions Deployment Workflows

### 1. Azure Container Apps Deployment

File: `.github/workflows/deploy-azure-container-apps.yml`

**Features:**
- Builds and pushes Docker image to Azure Container Registry
- Deploys to Azure Container Apps
- Configures environment variables and secrets
- Sets up Aspire dashboard
- Automatic rollback on failure

**Trigger:**
- Push to `main` branch
- Manual workflow dispatch
- Tag creation (`v*.*.*`)

### 2. Azure App Service Deployment

File: `.github/workflows/deploy-azure-app-service.yml`

**Features:**
- Builds .NET application
- Publishes to Azure App Service
- Runs database migrations
- Zero-downtime deployment with slots

**Trigger:**
- Push to `main` branch
- Manual workflow dispatch

### 3. Docker Image Build and Push

File: `.github/workflows/docker-build-push.yml`

**Features:**
- Multi-architecture builds (amd64, arm64)
- Pushes to Docker Hub and GitHub Container Registry
- Tags images with version and commit SHA
- Automated security scanning

**Trigger:**
- Push to `main` branch
- Tag creation
- Manual workflow dispatch

## Deployment Steps

### Azure Container Apps Setup

1. **Create Azure Resources**
```bash
# Login to Azure
az login

# Create resource group
az group create --name warranty-app-rg --location eastus

# Create Azure Container Registry
az acr create --resource-group warranty-app-rg \
  --name warrantyappcr --sku Basic

# Create Container Apps environment
az containerapp env create \
  --name warranty-app-env \
  --resource-group warranty-app-rg \
  --location eastus

# Create Azure SQL Database
az sql server create \
  --name warranty-sql-server \
  --resource-group warranty-app-rg \
  --location eastus \
  --admin-user sqladmin \
  --admin-password <strong-password>

az sql db create \
  --resource-group warranty-app-rg \
  --server warranty-sql-server \
  --name WarrantyDB \
  --service-objective S0
```

2. **Configure GitHub Service Principal**
```bash
# Create service principal for GitHub Actions
az ad sp create-for-rbac \
  --name "warranty-app-github" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/warranty-app-rg \
  --sdk-auth
```

3. **Add GitHub Secrets**
- Navigate to repository Settings > Secrets and variables > Actions
- Add each required secret listed above

4. **Trigger Deployment**
- Push to `main` branch, or
- Manually trigger workflow from Actions tab

### Docker Deployment

1. **Build Docker Image**
```bash
cd MyAspireSolution
docker build -t warrantyapp:latest -f MyApi/Dockerfile .
```

2. **Run with Docker Compose**
```bash
docker-compose up -d
```

3. **Configure Environment Variables**
Edit `.env` file:
```env
CONNECTION_STRING=Server=sqlserver;Database=WarrantyDB;User=sa;Password=<password>
OPENAI_API_KEY=<your-key>
JWT_SECRET=<strong-random-string>
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=<email>
SMTP_PASSWORD=<app-password>
```

## Database Migrations

### Automatic Migrations (Development)
The application automatically applies migrations on startup when running through Aspire.

### Manual Migrations (Production)
```bash
cd MyApi
dotnet ef database update --connection "<production-connection-string>"
```

### Migration in CI/CD
The deployment workflow includes a migration step:
```yaml
- name: Run database migrations
  run: |
    dotnet tool install --global dotnet-ef
    dotnet ef database update --project MyApi --connection "${{ secrets.SQL_CONNECTION_STRING }}"
```

## Monitoring and Health Checks

### Application Insights (Azure)
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Check Endpoints
- `/health` - Overall health status
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### Aspire Dashboard (Production)
Deploy Aspire dashboard separately for production monitoring:
```bash
az containerapp create \
  --name aspire-dashboard \
  --resource-group warranty-app-rg \
  --environment warranty-app-env \
  --image mcr.microsoft.com/dotnet/aspire-dashboard:latest \
  --ingress external \
  --target-port 18888
```

## Rollback Strategy

### Azure Container Apps
```bash
# List revisions
az containerapp revision list \
  --name warranty-api \
  --resource-group warranty-app-rg

# Activate previous revision
az containerapp revision activate \
  --name warranty-api \
  --resource-group warranty-app-rg \
  --revision <previous-revision-name>
```

### Docker
```bash
# Tag and keep previous version
docker tag warrantyapp:latest warrantyapp:previous

# Rollback
docker-compose down
docker pull warrantyapp:previous
docker tag warrantyapp:previous warrantyapp:latest
docker-compose up -d
```

## Security Considerations

### Secrets Management
- **Never commit secrets** to version control
- Use Azure Key Vault or GitHub Secrets
- Rotate credentials regularly
- Use managed identities where possible

### SSL/TLS
- Azure Container Apps provides automatic HTTPS
- For custom domains, configure SSL certificates
- Use Azure Front Door or Application Gateway for advanced security

### Network Security
- Configure virtual networks for Azure resources
- Use private endpoints for databases
- Implement firewall rules
- Enable DDoS protection

## Cost Optimization

### Azure Container Apps
- Use consumption plan for variable workloads
- Configure auto-scaling rules (min 0, max 10 instances)
- Use Azure SQL serverless for development/staging

### Docker Self-Hosted
- Use smaller base images (Alpine Linux)
- Implement container resource limits
- Schedule non-critical tasks for off-peak hours

## Disaster Recovery

### Backup Strategy
1. **Database Backups**
   - Azure SQL automated backups (35 days retention)
   - Manual backups before major changes
   
2. **Application Backups**
   - Git repository is source of truth
   - Docker images in registry
   - Configuration in infrastructure as code

3. **File Storage Backups**
   - Receipt files in Azure Blob Storage (geo-redundant)
   - Regular backup jobs to cold storage

### Recovery Plan
1. Restore database from backup
2. Deploy previous working container image
3. Restore file storage from backup
4. Update DNS if needed

## Testing Deployment

### Staging Environment
Create a separate staging environment:
```bash
az containerapp create \
  --name warranty-api-staging \
  --environment warranty-app-env-staging \
  --resource-group warranty-app-rg
```

### Smoke Tests
Run automated smoke tests after deployment:
```bash
# Health check
curl https://warranty-api.azurecontainerapps.io/health

# Authentication test
curl -X POST https://warranty-api.azurecontainerapps.io/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@example.com","password":"Test123!"}'
```

## Troubleshooting

### Common Issues

**Issue**: Container fails to start
```bash
# Check logs
az containerapp logs show \
  --name warranty-api \
  --resource-group warranty-app-rg \
  --tail 100
```

**Issue**: Database connection fails
- Verify connection string in secrets
- Check firewall rules allow Container Apps IP
- Test connection from Azure Cloud Shell

**Issue**: OCR not working
- Verify OpenAI API key is set
- Check API quota limits
- Review application logs for errors

## Continuous Improvement

### Metrics to Monitor
- Deployment success rate
- Deployment duration
- Rollback frequency
- Application health score
- Response times
- Error rates

### Optimization Targets
- Deployment time < 5 minutes
- Zero-downtime deployments
- Automatic rollback on health check failure
- Infrastructure as Code for all resources

## References

- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [.NET Aspire Deployment](https://learn.microsoft.com/dotnet/aspire/deployment/)
- [GitHub Actions for Azure](https://docs.microsoft.com/azure/developer/github/github-actions)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

---

**Status**: 📋 Documentation complete | 🚧 Workflows ready to be created | ✅ CI pipeline operational

**Last Updated**: 2025-11-16
