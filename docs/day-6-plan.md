# Day 6 Plan - Security Baseline (JWT + Public/Private API)

## Goal

Implement a minimal and secure authentication layer that:

- Protects private endpoints
- Ensures tokens are generated only by the backend
- Prevents frontend from accessing secrets
- Enables safe production usage

---

## Context

Current state:

- All endpoints are open
- No authentication or authorization
- Any user can call APIs via Postman
- No tenant isolation enforcement via identity

Risk:

- Unauthorized data access
- Abuse of booking endpoints
- Exposure of business data

---

## Security Model

The system will be divided into two areas:

### Public API

Used by end customers (e.g. Maria)

Characteristics:

- No authentication required
- Limited scope
- Tenant-scoped via URL or key
- Protected by validation and rate limiting

Examples:

- GET /public/{tenantSlug}/services
- GET /public/{tenantSlug}/availability
- POST /public/{tenantSlug}/bookings

---

### Private API

Used by business owner (e.g. Amanda)

Characteristics:

- Requires authentication (JWT)
- Tenant-scoped via claims
- Full access to management features

Examples:

- POST /api/auth/login
- GET /api/me
- POST /api/services
- GET /api/requests

---

## Authentication Strategy

Use JWT (JSON Web Token)

### Token generation

- Generated ONLY in backend
- Signed with a secret stored in server environment
- Never exposed to frontend

### Token usage

Frontend sends:

Authorization: Bearer {token}

---

## Claims

JWT must contain:

- sub (user id)
- email
- tenantId
- role

---

## Authorization Rules

- Every private endpoint must require JWT
- TenantId must be derived from token, not from request body
- Queries must filter by tenantId from claims

---

## Key Security Rule

Frontend MUST NOT:

- Know JWT signing secret
- Generate tokens
- Access sensitive credentials

Backend MUST:

- Generate and validate tokens
- Store secrets in environment variables

---

## Data Model (Minimal)

### User

- Id
- Email
- PasswordHash
- TenantId
- Role
- CreatedAt

---

## Flow

### Login

1. User sends email + password
2. Backend validates credentials
3. Backend generates JWT
4. Returns token

---

### Authenticated request

1. Frontend sends Bearer token
2. Backend validates signature
3. Extracts claims
4. Applies tenant filtering

---

## Out of Scope (Day 6)

- OAuth login for users
- Multi-user roles per tenant
- Password recovery
- Email verification
- Refresh tokens
- Rate limiting (can be added later)
- API gateway / WAF

---

## Success Criteria

- Private endpoints require JWT
- Public endpoints remain accessible
- Tokens are generated only by backend
- Frontend has no access to secrets
- Tenant isolation is enforced via claims

---

## Philosophy

- Security must be simple and correct
- Avoid overengineering
- Protect what matters first
- Build a safe baseline before scaling