# Day 6 Checklist

## Goal
Secure AlicIA APIs using JWT authentication and public/private separation.

---

## Project Structure

- [ ] Create User entity
- [ ] Add User to DbContext
- [ ] Create migration for User
- [ ] Apply migration

---

## Password Handling

- [ ] Implement password hashing (BCrypt)
- [ ] Store PasswordHash instead of plain password

---

## JWT Setup

- [ ] Add JWT configuration in appsettings:
  - [ ] Secret
  - [ ] Issuer
  - [ ] Audience
- [ ] Move secret to environment variables

---

## Token Service

- [ ] Create JwtTokenService
- [ ] Implement token generation method
- [ ] Include claims:
  - [ ] userId
  - [ ] email
  - [ ] tenantId
  - [ ] role

---

## Auth Endpoints

- [ ] Create POST /api/auth/login
- [ ] Validate email/password
- [ ] Generate JWT
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