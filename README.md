# AlicIA - Conversational Operational Agent

AlicIA is a SaaS product designed to help small businesses automate customer interactions and transform them into structured, actionable requests.

## Day 1: Domain & Database
- ✅ Created core domain entities (Tenant, Service, Customer, Request)
- ✅ Set up PostgreSQL database with EF Core 8.0
- ✅ Implemented CalendarConnection entity for Google Calendar integration
- ✅ Applied migrations and tested database connectivity

## Day 2: API & Request Management  
- ✅ Built REST endpoints for tenants, services, customers, and requests
- ✅ Implemented request lifecycle management (Pending → Confirmed → Completed)
- ✅ Added request status tracking (Pending, Confirmed, Rescheduled, Cancelled, NoShow, etc.)
- ✅ API running successfully with Swagger documentation

## Day 3: Availability & Scheduling
- ✅ Created BusinessHours entity for tenant scheduling
- ✅ Implemented POST /business-hours to configure business hours
- ✅ Implemented GET /business-hours to retrieve tenant hours
- ✅ Implemented GET /availability/next-slots for intelligent slot calculation
- ✅ Slot calculation excludes already scheduled requests
- ✅ 15-minute slot granularity with service-aware duration

## Day 4: Google Calendar Integration
- ✅ Implemented OAuth flow for Google Calendar connection
- ✅ Created CalendarConnection entity for storing refresh tokens
- ✅ Implemented GET /oauth/google/start/{tenantId} for OAuth initiation
- ✅ Implemented GET /oauth/google/callback for token exchange
- ✅ Implemented GET /google/busy-slots to fetch calendar busy times
- ✅ Implemented GET /availability/google-next-slots for slots excluding Google events
- ✅ Implemented POST /requests/{id}/sync-google-event for booking sync
- ✅ Added ExternalEventId to Request entity for event tracking
- ✅ Full Google Calendar integration with availability and booking sync

## Day 5: Full Booking Flow
- ✅ Implemented POST /bookings endpoint for unified booking
- ✅ Automatic customer creation/reuse based on phone number
- ✅ Atomic booking with Google Calendar sync
- ✅ Prevents double booking with multi-source conflict detection
- ✅ Returns confirmation with request and event IDs

## Day 6: Security Baseline (JWT + Public/Private API)
- ✅ Created User entity with password hashing (PBKDF2-SHA256)
- ✅ Implemented JwtAuthService for token generation and validation
- ✅ Added POST /api/auth/login endpoint
- ✅ Added POST /api/auth/signup endpoint  
- ✅ Added GET /api/me protected endpoint (requires JWT)
- ✅ Configured JWT Bearer authentication middleware
- ✅ Implemented PasswordHasher with 10,000 iterations + salt
- ✅ Database migration creates users table with unique email constraint
- ✅ Tenant isolation enforced through JWT claims
- ✅ All security configuration externalized to appsettings

## Tech Stack
- **Backend**: ASP.NET Core 8.0
- **Database**: PostgreSQL (Neon)
- **ORM**: Entity Framework Core 8.0
- **Security**: JWT (HS256), PBKDF2-SHA256
- **API Style**: REST with minimal abstractions

## Core Entities

### Tenant
Business account using AlicIA
- Name, Segment, Plan, Status
- Collections: Services, Customers, Requests, BusinessHours, CalendarConnections, Users

### User
Business owner managing a tenant
- Email, PasswordHash, Role, TenantId
- Unique email per tenant
- Used for authentication and authorization

### Service
Service offered by a tenant
- Name, DurationMinutes, Price
- Linked to Tenant with cascading delete

### Customer
End-user of a tenant
- Name, Phone, Email
- Linked to Tenant

### Request
Structured booking request
- Type (Booking, Reservation, Order)
- Status tracking (Pending → Completed)
- ScheduledAt timestamp
- TotalAmount
- ExternalEventId for Google Calendar sync

### BusinessHours
Operating hours per day of week
- DayOfWeek, StartTime, EndTime
- IsActive flag
- Enables availability calculations

## API Endpoints

### Authentication (Public)
- `POST /api/auth/signup` - Create new user account
- `POST /api/auth/login` - Login with email and password (returns JWT)

### Protected Endpoints (Require JWT)
- `GET /api/me` - Get current user info

### Tenants
- `POST /tenants` - Create tenant
- `GET /tenants` - List all tenants

### Services
- `POST /services` - Create service
- `GET /services?tenantId={id}` - List services

### Customers
- `POST /customers` - Create customer
- `GET /customers?tenantId={id}` - List customers

### Requests
- `POST /requests` - Create booking request
- `GET /requests?tenantId={id}` - List requests

### Bookings
- `POST /bookings` - Create booking with automatic sync to Google Calendar

### Business Hours
- `POST /business-hours` - Set tenant operating hours
- `GET /business-hours?tenantId={id}` - Get hours

### Availability
- `GET /availability/next-slots?tenantId={id}&serviceId={id}&days=7&maxSlots=10`
  - Returns available time slots
  - Respects business hours
  - Excludes already scheduled requests
  - 15-minute granularity

### Google Calendar Integration
- `GET /oauth/google/start/{tenantId}` - Initiate Google OAuth flow
- `GET /oauth/google/callback` - Handle OAuth callback and store tokens
- `GET /calendar-connections?tenantId={id}` - List calendar connections
- `GET /google/busy-slots?tenantId={id}` - Get busy slots from Google Calendar
- `GET /availability/google-next-slots?tenantId={id}&serviceId={id}` - Available slots excluding Google events
- `POST /requests/{id}/sync-google-event` - Sync booking to Google Calendar

## Authentication

### JWT Token Format
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "tenantId": "tenant-id",
  "role": "Owner",
  "exp": 1234567890
}
```

### Configuration
```json
{
  "Jwt": {
    "Secret": "minimum-32-characters-for-production",
    "Issuer": "AlicIA",
    "Audience": "AlicIA",
    "ExpirationMinutes": 1440
  }
}
```

### Usage
```bash
# Signup
curl -X POST http://localhost:5000/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "tenant-id",
    "email": "user@example.com",
    "password": "secure-password",
    "role": "Owner"
  }'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "tenant-id",
    "email": "user@example.com",
    "password": "secure-password"
  }'

# Protected request
curl -X GET http://localhost:5000/api/me \
  -H "Authorization: Bearer {token}"
```

## Running Locally

```bash
# Build
dotnet build

# Run migrations
export PATH="$PATH:/Users/wmedeiros/.dotnet/tools"
dotnet ef database update --project src/AlicIA.Infrastructure --startup-project src/AlicIA.Api

# Start API
dotnet run --project src/AlicIA.Api

# Access Swagger
http://localhost:5000/swagger
```

## Architecture

```
src/
  AlicIA.Domain/          # Domain entities & enums
    Entities/
    Enums/
  AlicIA.Infrastructure/  # Database & persistence
    Persistence/Migrations/
    Security/             # JWT & Password services
    Integrations/         # Google Calendar integration
  AlicIA.Api/             # REST API & endpoints
    Models/
```

## Database Schema

- `tenants` - Business accounts
- `users` - User accounts per tenant
- `services` - Service catalog
- `customers` - Customer records
- `requests` - Booking requests
- `calendar_connections` - Google Calendar integrations
- `business_hours` - Operating hours

All tables support cascade deletes where appropriate and include UTC timestamps.

## Security

- **Password Hashing**: PBKDF2-SHA256 with 10,000 iterations + random salt
- **JWT Tokens**: HS256 signature with 24-hour expiration
- **Tenant Isolation**: Claims-based filtering prevents data leakage
- **Authorization**: Bearer token validation on protected endpoints

## Next Steps

- Implement public API endpoints (/public/{slug}/services, /availability, /bookings)
- Add authorization policies for different roles
- Implement refresh tokens
- Add rate limiting to auth endpoints
- Build customer-facing UI (Next.js)
- Add multi-language support
- WhatsApp integration
- Implement notification system
- Payment processing

