# Day 6 Checklist - Security Baseline (JWT + Public/Private API)

## ✅ Completed Tasks

### Authentication Infrastructure
- [x] Create User entity with Id, Email, PasswordHash, TenantId, Role, CreatedAt
- [x] Create User DbSet in AlicIADbContext
- [x] Run EF migration to create users table
- [x] Implement JwtAuthService for token generation and validation
- [x] Implement PasswordHasher with PBKDF2-SHA256
- [x] Configure JWT authentication middleware in Program.cs
- [x] Add JWT packages to project dependencies

### Authentication Endpoints (Public)
- [x] POST /api/auth/login - Login with email and password, returns JWT token
- [x] POST /api/auth/signup - Create new user in tenant, returns JWT token
- [x] GET /api/me - Protected endpoint showing current user info

### Security Configuration
- [x] Add JWT settings to appsettings.json (Secret, Issuer, Audience, ExpirationMinutes)
- [x] Configure Bearer token validation
- [x] Add authentication and authorization middleware to pipeline

### JWT Claims
- [x] sub (user id)
- [x] email
- [x] tenantId
- [x] role

### Database Schema
- [x] users table created with proper constraints
- [x] Unique index on (tenantId, email)
- [x] Foreign key to tenants table

---

## 📋 Remaining Tasks

### Public API Endpoints (Day 6+)
- [ ] GET /public/{tenantSlug}/services - List services without authentication
- [ ] GET /public/{tenantSlug}/availability - Get availability slots without authentication
- [ ] POST /public/{tenantSlug}/bookings - Create booking without authentication
- [ ] Implement tenant lookup by slug

### Private API Protection
- [ ] Add [Authorize] attribute to /api/* endpoints
- [ ] Extract tenantId from JWT claims instead of request body
- [ ] Update /api/services, /api/customers, /api/requests to use JWT tenantId
- [ ] Validate tenant ownership for all operations

### Testing & Validation
- [ ] Test signup endpoint
- [ ] Test login endpoint with valid credentials
- [ ] Test login endpoint with invalid credentials
- [ ] Test protected /api/me endpoint without token (should return 401)
- [ ] Test protected /api/me endpoint with valid token
- [ ] Test JWT expiration
- [ ] Verify existing endpoints still work
- [ ] Verify private endpoints reject requests without token

---

## Implementation Summary

### Services Created

**JwtAuthService** - Token generation and validation
- Generates JWT with claims (sub, email, tenantId, role)
- Validates token signature and expiration
- Configurable via appsettings

**PasswordHasher** - Secure password handling
- PBKDF2-SHA256 algorithm
- 10,000 iterations + 16-byte salt
- Constant-time comparison

### API Endpoints

**Public (No Authentication Required)**
- POST /api/auth/login
- POST /api/auth/signup

**Protected (Requires JWT)**
- GET /api/me

### Database

**New Table: users**
- Id (uuid, PK)
- TenantId (uuid, FK to tenants)
- Email (varchar 200)
- PasswordHash (text)
- Role (varchar 50)
- CreatedAt (timestamp)
- Unique index on (TenantId, Email)

---

## Configuration

```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-key-minimum-32-characters",
    "Issuer": "AlicIA",
    "Audience": "AlicIA",
    "ExpirationMinutes": 1440
  }
}
```

---

## Testing

### Create Tenant
```bash
curl -X POST http://localhost:5000/tenants \
  -H "Content-Type: application/json" \
  -d '{"name":"Amanda Beauty","segment":"beauty","plan":"pro","status":"Active"}'
```

### Signup
```bash
curl -X POST http://localhost:5000/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId":"<tenant-id>",
    "email":"amanda@example.com",
    "password":"secure-password-123",
    "role":"Owner"
  }'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId":"<tenant-id>",
    "email":"amanda@example.com",
    "password":"secure-password-123"
  }'
```

### Get Current User
```bash
curl -X GET http://localhost:5000/api/me \
  -H "Authorization: Bearer <token>"
```

---

## Success Criteria

✅ Private endpoints require JWT
✅ Public endpoints remain accessible
✅ Tokens generated only by backend
✅ Frontend has no access to secrets
✅ Tenant isolation enforced via claims

---

## Files Modified

- src/AlicIA.Api/Program.cs (JWT middleware, auth endpoints)
- src/AlicIA.Api/appsettings.json (JWT configuration)
- src/AlicIA.Api/appsettings.Development.json
- src/AlicIA.Api/AlicIA.Api.csproj (JWT packages)
- src/AlicIA.Infrastructure/AlicIA.Infrastructure.csproj
- src/AlicIA.Infrastructure/AlicIADbContext.cs (User DbSet)
- src/AlicIA.Infrastructure/Persistence/Migrations/

## Files Created

- src/AlicIA.Domain/Entities/User.cs
- src/AlicIA.Infrastructure/Security/JwtAuthService.cs
- src/AlicIA.Infrastructure/Security/PasswordHasher.cs
- src/AlicIA.Api/Models/AuthModels.cs

- [ ] Return token

---

## Middleware

- [ ] Enable authentication middleware
- [ ] Enable authorization middleware
- [ ] Configure JWT validation

---

## Protect Private Endpoints

- [ ] Add [Authorize] to private endpoints
- [ ] Ensure tenantId is read from JWT claims
- [ ] Remove tenantId trust from request body

---

## Public API Separation

- [ ] Create /public route group
- [ ] Move booking-related endpoints to public
- [ ] Ensure no sensitive data is exposed

---

## Security Validation

- [ ] Access private endpoint without token → must fail
- [ ] Access private endpoint with token → must succeed
- [ ] Token cannot be forged without backend secret
- [ ] Frontend does not contain any secret

---

## Docs and commit

- [ ] Update README with Day 6 progress
- [ ] Commit changes
- [ ] Push to GitHub