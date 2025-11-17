# Refresh Token Support

## Overview

This document describes the implementation of refresh token support for JWT authentication, allowing users to obtain new access tokens without re-authenticating with credentials.

## Architecture

### Components

1. **ApplicationUser Model** - Extended to store refresh tokens and expiry times
2. **TokenService** - Enhanced with refresh token generation and validation
3. **AuthController** - New endpoints for token refresh and revocation
4. **DTOs** - New request/response models for refresh operations

## Token Lifecycle

### Access Tokens
- **Lifespan**: 60 minutes
- **Purpose**: Authorize API requests
- **Format**: JWT with user claims

### Refresh Tokens
- **Lifespan**: 7 days
- **Purpose**: Obtain new access tokens without re-authentication
- **Format**: Cryptographically secure random 64-byte string (Base64 encoded)
- **Storage**: Stored in database associated with user account

## Database Schema Changes

Added to `ApplicationUser`:
```csharp
public string? RefreshToken { get; set; }
public DateTime? RefreshTokenExpiryTime { get; set; }
```

Migration: `AddRefreshTokenSupport`

## API Endpoints

### POST /api/Auth/refresh

Obtain new access and refresh tokens using an expired/valid access token and refresh token.

**Request Body:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123..."
}
```

**Success Response (200 OK):**
```json
{
  "token": "eyJhbGc...",
  "refreshToken": "xyz789...",
  "email": "user@example.com",
  "username": "user",
  "expiresAt": "2025-11-16T13:30:00Z",
  "refreshTokenExpiresAt": "2025-11-23T12:30:00Z"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or expired refresh token
- `400 Bad Request` - Invalid request format

### POST /api/Auth/revoke

Revoke the current user's refresh token (requires authentication).

**Headers:**
```
Authorization: Bearer {access-token}
```

**Success Response (200 OK):**
```json
{
  "message": "Refresh token revoked successfully"
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing access token

## Security Features

### Token Validation
1. **Access Token Verification**: Validates signature, issuer, and audience (ignores expiration for refresh)
2. **Refresh Token Matching**: Compares provided refresh token with stored hash
3. **Expiry Check**: Validates refresh token hasn't expired
4. **User Validation**: Ensures user still exists in the system

### Refresh Token Generation
- Uses `RandomNumberGenerator` for cryptographic randomness
- Generates 64-byte random value
- Base64 encoded for transmission

### Token Rotation
- Each refresh operation generates a new refresh token
- Old refresh token is immediately invalidated
- Prevents token replay attacks

## Implementation Details

### TokenService Methods

```csharp
string GenerateRefreshToken()
```
Generates a cryptographically secure random refresh token.

```csharp
ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
```
Extracts claims from an expired access token for refresh validation.

### Auth Flow Integration

Both `register` and `login` endpoints now return:
- Access token (JWT)
- Refresh token
- Access token expiry time
- Refresh token expiry time

## Usage Examples

### Login Flow
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Response includes both tokens.

### Refresh Flow
When access token expires (after 60 minutes):

```http
POST /api/Auth/refresh
Content-Type: application/json

{
  "accessToken": "{expired-jwt}",
  "refreshToken": "{refresh-token-from-login}"
}
```

### Revoke Flow
When user logs out or security requires token revocation:

```http
POST /api/Auth/revoke
Authorization: Bearer {access-token}
```

## Client Integration

### Recommended Client-Side Flow

1. **Initial Authentication**
   - Call `/api/Auth/login`
   - Store both tokens securely (e.g., HTTP-only cookies, secure storage)

2. **API Requests**
   - Include access token in `Authorization: Bearer {token}` header
   - Handle 401 responses

3. **Token Refresh**
   - On 401 response, check if refresh token is available
   - Call `/api/Auth/refresh` with both tokens
   - Update stored tokens with new values
   - Retry original request with new access token

4. **Logout**
   - Call `/api/Auth/revoke` to invalidate refresh token
   - Clear stored tokens
   - Redirect to login

### Error Handling

If refresh fails (401):
- Clear stored tokens
- Redirect user to login
- Refresh token may have expired (7 days)

## Configuration

### Token Expiry Times

Access Token: `JwtSettings.ExpiryInMinutes` (default: 60)
Refresh Token: Hardcoded 7 days (can be made configurable)

To change refresh token expiry, update in:
- `AuthController.Register()` line with `AddDays(7)`
- `AuthController.Login()` line with `AddDays(7)`
- `AuthController.Refresh()` line with `AddDays(7)`

## Security Considerations

### Best Practices

1. **Storage**: 
   - Store refresh tokens securely on client
   - Never expose refresh tokens in URLs or logs
   - Use HTTP-only cookies for web clients

2. **Transport**:
   - Always use HTTPS in production
   - Refresh tokens are sensitive credentials

3. **Rotation**:
   - Tokens are rotated on each refresh
   - Old tokens are immediately invalidated

4. **Revocation**:
   - Implement logout to revoke refresh tokens
   - Consider periodic cleanup of expired tokens

### Attack Mitigation

- **Token Replay**: Mitigated by token rotation
- **Token Theft**: Limited by 7-day expiry and revocation
- **MITM**: Prevented by HTTPS requirement
- **XSS**: Store tokens securely (not in localStorage for web)

## Testing

### Manual Testing with Swagger

1. Register or login to get tokens
2. Wait for access token to expire (or use expired token)
3. Use refresh endpoint with both tokens
4. Verify new tokens are returned
5. Test revoke endpoint

### Automated Testing Scenarios

- Valid refresh with expired access token
- Invalid refresh token
- Expired refresh token
- Refresh with valid access token (should still work)
- Revoke and attempt refresh
- Refresh after user deleted

## Monitoring and Logging

Key events logged:
- Token refresh success
- Token refresh failure (with reason)
- Token revocation

Log messages include user email for audit trail.

## Future Enhancements

1. **Configurable Expiry**: Make refresh token lifetime configurable
2. **Token Families**: Track token lineage to detect theft
3. **Device Tracking**: Associate refresh tokens with specific devices
4. **Sliding Expiration**: Extend refresh token lifetime on use
5. **Admin Revocation**: API for admins to revoke user tokens
6. **Token Cleanup**: Background job to remove expired tokens

## Related Documentation

- [04 - Authentication & Authorization](./04-authentication-authorization.md)
- [12 - User Profile Management](./12-user-profile-management.md)

## References

- [RFC 6749 - OAuth 2.0 Authorization Framework](https://tools.ietf.org/html/rfc6749)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
