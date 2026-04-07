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

## Tech Stack
- **Backend**: ASP.NET Core 8.0
- **Database**: PostgreSQL (Neon)
- **ORM**: Entity Framework Core 8.0
- **API Style**: REST with minimal abstractions

## Core Entities

### Tenant
Business account using AlicIA
- Name, Segment, Plan, Status
- Collections: Services, Customers, Requests, BusinessHours, CalendarConnections

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

### BusinessHours
Operating hours per day of week
- DayOfWeek, StartTime, EndTime
- IsActive flag
- Enables availability calculations

## API Endpoints

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
  AlicIA.Api/             # REST API & endpoints
    Controllers/
    Models/
```

## Database Schema

- `tenants` - Business accounts
- `services` - Service catalog
- `customers` - Customer records
- `requests` - Booking requests
- `calendar_connections` - Google Calendar integrations
- `business_hours` - Operating hours

All tables support cascade deletes where appropriate and include UTC timestamps.

## Next Steps

- Add payment processing
- Build customer-facing UI (Next.js)
- Add multi-language support
- WhatsApp integration
- Implement notification system
