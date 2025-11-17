# 04 - Authentication and Authorization Implementation

**Date:** November 16, 2025  
**Status:** Completed

## Overview
Implemented JWT-based authentication and authorization system using ASP.NET Core Identity with Entity Framework Core.

## Packages Added

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.11 | JWT Bearer authentication |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.11 | ASP.NET Core Identity with EF Core |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.11 | SQL Server database provider |
| Microsoft.EntityFrameworkCore.Tools | 8.0.11 | EF Core CLI tools |

## Project Structure

### New Folders and Files Created

```
MyApi/
├── Controllers/
│   └── AuthController.cs         # Authentication endpoints
├── Data/
│   └── ApplicationDbContext.cs   # EF Core DbContext
├── DTOs/
│   ├── AuthResponseDto.cs        # Login/Register response
│   ├── LoginDto.cs               # Login request
│   └── RegisterDto.cs            # Registration request
├── Models/
│   └── ApplicationUser.cs        # Extended Identity User
├── Services/
│   ├── ITokenService.cs          # Token service interface
│   ├── TokenService.cs           # JWT token generation
│   └── JwtSettings.cs            # JWT configuration model
└── Migrations/
    └── InitialCreate              # Database migration
```

## Configuration Changes

### 1. appsettings.json
Added configuration for:
- **Connection String**: SQL Server LocalDB connection
- **JWT Settings**:
  - Secret: Secure key for signing tokens
  - Issuer: Token issuer identifier
  - Audience: Token audience
  - Expiry: Token lifetime (60 minutes)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ReceiptAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "ThisIsAVerySecureSecretKeyForJWTTokenGeneration123456!",
    "Issuer": "ReceiptApp",
    "Audience": "ReceiptAppUsers",
    "ExpiryInMinutes": 60
  }
}
```

### 2. Program.cs Updates
Configured:
- **Database Context**: SQL Server with connection string
- **ASP.NET Core Identity**: Password policies and user settings
- **JWT Authentication**: Bearer token validation
- **Authorization**: Role-based access control
- **Swagger**: JWT authentication support
- **CORS**: Allow all origins (dev mode)
- **Controllers**: Added controller support

## Models

### ApplicationUser
Extends `IdentityUser` with additional properties:
- `FirstName` (string, optional)
- `LastName` (string, optional)
- `CreatedAt` (DateTime, UTC)
- `LastLoginAt` (DateTime, nullable)

## DTOs (Data Transfer Objects)

### RegisterDto
- Email (required, validated)
- Password (required, min 6 chars)
- Username (required, 3-50 chars)
- FirstName (optional)
- LastName (optional)

### LoginDto
- Email (required, validated)
- Password (required)

### AuthResponseDto
- Token (JWT string)
- Email
- Username
- ExpiresAt (DateTime)

## API Endpoints

### POST /api/auth/register
**Description:** Register a new user  
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com",
  "username": "johndoe",
  "expiresAt": "2025-11-16T10:29:00Z"
}
```

**Error Response:** 400 Bad Request (validation errors)

### POST /api/auth/login
**Description:** Authenticate and get JWT token  
**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**Response:** 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com",
  "username": "johndoe",
  "expiresAt": "2025-11-16T10:29:00Z"
}
```

**Error Response:** 401 Unauthorized (invalid credentials)

### GET /api/auth/me
**Description:** Get current user information  
**Authorization:** Bearer token required  
**Response:** 200 OK
```json
{
  "id": "user-id-guid",
  "email": "user@example.com",
  "username": "johndoe",
  "firstName": "John",
  "lastName": "Doe",
  "createdAt": "2025-11-16T09:00:00Z",
  "lastLoginAt": "2025-11-16T09:29:00Z"
}
```

**Error Response:** 401 Unauthorized (no/invalid token)

### POST /api/auth/logout
**Description:** Logout user  
**Authorization:** Bearer token required  
**Response:** 200 OK
```json
{
  "message": "Logged out successfully"
}
```

## JWT Token Service

### Token Generation
- **Claims Included:**
  - NameIdentifier (User ID)
  - Email
  - Name (Username)
  - GivenName (FirstName, if present)
  - Surname (LastName, if present)
  - Jti (Unique token ID)

- **Security:**
  - HMAC-SHA256 signing algorithm
  - Configurable expiration time
  - Issuer and audience validation

## Security Features

### Password Requirements
- Minimum length: 6 characters
- Requires digit: Yes
- Requires lowercase: Yes
- Requires uppercase: Yes
- Requires non-alphanumeric: No

### Token Validation
- Signature validation
- Issuer validation
- Audience validation
- Lifetime validation
- Zero clock skew

### Authorization
- `[Authorize]` attribute for protected endpoints
- Role-based authorization ready
- Claims-based authorization support

## Database

### Schema
The migration creates ASP.NET Core Identity tables:
- Users (customized table name)
- Roles
- UserRoles
- UserClaims
- UserLogins
- UserTokens
- RoleClaims

### Migration Created
- **Name:** InitialCreate
- **Status:** Created, not yet applied

### Applying Migration
To create the database, run:
```bash
dotnet ef database update
```

## Swagger Integration

### JWT Support in Swagger UI
1. Click "Authorize" button in Swagger UI
2. Enter: `Bearer {your-jwt-token}`
3. All subsequent requests include the token
4. Test protected endpoints directly from Swagger

### Security Scheme
- Type: API Key
- Location: Header
- Name: Authorization
- Scheme: Bearer

## Testing the Authentication

### 1. Register a New User
```bash
curl -X POST https://localhost:PORT/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "username": "testuser",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Login
```bash
curl -X POST https://localhost:PORT/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

Copy the `token` from the response.

### 3. Access Protected Endpoint
```bash
curl -X GET https://localhost:PORT/api/auth/me \
  -H "Authorization: Bearer {your-token-here}"
```

## CORS Configuration

Currently configured to allow all origins for development:
```csharp
policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
```

**Production Note:** Restrict CORS to specific origins before deployment.

## Next Steps

1. ✅ Apply database migration: `dotnet ef database update`
2. Test all authentication endpoints
3. Add role-based authorization
4. Implement refresh tokens
5. Add password reset functionality
6. Add email confirmation
7. Configure production-ready CORS
8. Add rate limiting for auth endpoints
9. Implement account lockout after failed attempts
10. Add two-factor authentication (2FA)

## Security Considerations

### For Production

**Critical changes needed before production:**
1. **JWT Secret:** Move to environment variables or Azure Key Vault
2. **CORS:** Restrict to specific frontend origins
3. **HTTPS:** Enforce HTTPS (set `RequireHttpsMetadata = true`)
4. **Connection String:** Use Azure SQL or secure database
5. **Password Policy:** Consider stricter requirements
6. **Rate Limiting:** Add authentication rate limiting
7. **Logging:** Implement security event logging
8. **Token Refresh:** Implement refresh token rotation

### Current Development Settings
- HTTPS metadata validation: Disabled
- CORS: Allow all origins
- LocalDB: For development only
- JWT Secret: Hardcoded (move to secrets)

## Troubleshooting

### Issue: Migration fails
**Solution:** Ensure SQL Server LocalDB is installed:
```bash
sqllocaldb info
```

### Issue: 401 Unauthorized on protected endpoints
**Solution:** 
1. Check token is included in Authorization header
2. Format: `Bearer {token}`
3. Verify token hasn't expired
4. Check JWT secret matches configuration

### Issue: Password doesn't meet requirements
**Solution:** Ensure password has:
- At least 6 characters
- One uppercase letter
- One lowercase letter
- One digit

## Build and Test Status

- ✅ All packages installed successfully
- ✅ Solution builds without errors
- ✅ Database migration created
- ⏳ Database migration not yet applied
- ⏳ Endpoints not yet tested

## Summary

Successfully implemented a complete authentication and authorization system with:
- JWT-based authentication
- ASP.NET Core Identity integration
- Secure password hashing
- Token-based API access
- Swagger UI authentication support
- Entity Framework Core database backend
- Registration and login endpoints
- User profile endpoint
- Comprehensive security configuration

The authentication system is ready for testing and can be extended with additional features like role-based access control, refresh tokens, and two-factor authentication.
