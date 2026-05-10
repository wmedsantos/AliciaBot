# AlicIA Postman Collection Guide

## Overview

The `backend/AlicIA_API_Collection.postman_collection.json` file contains all API endpoints for testing the AlicIA booking system, now including **Day 7 production validation** with JWT authentication, public booking, confirmation-token lifecycle, and Google Calendar sync.

## How to Import

1. Open Postman
2. Click **Import** button (top left)
3. Select **Upload Files** tab
4. Choose `backend/AlicIA_API_Collection.postman_collection.json`
5. Click **Import**

## Environment Variables Setup

The collection uses these variables that you should set:

| Variable | Description | Example |
|----------|-------------|---------|
| `baseUrl` | API base URL | `http://localhost:5000` |
| `tenantSlug` | Public tenant slug | `ayax-consulting` |
| `tenantId` | Your tenant ID (copy from Create Tenant response) | `123e4567-e89b-12d3-a456-...` |
| `serviceId` | Service ID (copy from Create Service response) | `223e4567-e89b-12d3-a456-...` |
| `customerId` | Customer ID (copy from Create Customer response) | `323e4567-e89b-12d3-a456-...` |
| `requestId` | Request ID (copy from Create Request response) | `423e4567-e89b-12d3-a456-...` |
| `confirmationToken` | Customer booking token returned by public booking creation | `eyJ...` |
| `scheduledAt` | Initial booking date/time in UTC | `2026-05-12T13:00:00Z` |
| `rescheduledAt` | Reschedule date/time in UTC | `2026-05-12T14:00:00Z` |
| `jwtToken` | JWT token (copy from Login response) | `eyJ0eXAiOiJKV1QiLCJhbGc...` |

## Quick Start Flow

Follow these steps in order to test the complete flow:

### 1. Check Health
```
GET /health
```
Expected: 200 OK with API status

### 2. Create Tenant
```
POST /tenants
{
  "name": "Amanda Beleza",
  "segment": "Beauty",
  "plan": "Pro",
  "status": "Active"
}
```
**Save the returned `id` to `{{tenantId}}`**

### 3. Sign Up (Create User)
```
POST /api/auth/signup
{
  "tenantId": "{{tenantId}}",
  "email": "amanda@beleza.com",
  "password": "SecurePassword123",
  "role": "Owner"
}
```
**Save the returned `token` to `{{jwtToken}}`**

### 4. Login
```
POST /api/auth/login
{
  "tenantId": "{{tenantId}}",
  "email": "amanda@beleza.com",
  "password": "SecurePassword123"
}
```
**Save the returned `token` to `{{jwtToken}}`**

### 5. Get Current User (Protected)
```
GET /api/me
Headers: Authorization: Bearer {{jwtToken}}
```
Expected: User details with tenantId and role

### 6. Create Service
```
POST /services
{
  "tenantId": "{{tenantId}}",
  "name": "Alongamento de Cílios",
  "durationMinutes": 60,
  "price": 150.00
}
```
**Save the returned `id` to `{{serviceId}}`**

### 7. Set Business Hours
```
POST /business-hours
{
  "tenantId": "{{tenantId}}",
  "dayOfWeek": 1,
  "startTime": "09:00:00",
  "endTime": "18:00:00"
}
```
Set for multiple days (Monday=1, Saturday=6)

### 8. Public Services by Slug
```
GET /public/{{tenantSlug}}/services
```
Expected: customer-safe service list.

### 9. Public Availability by Slug
```
GET /public/{{tenantSlug}}/availability?serviceId={{serviceId}}&days=7&maxSlots=10
```
Expected: customer-safe availability slots.

### 10. Public Booking
```
POST /public/{{tenantSlug}}/bookings
{
  "serviceId": "{{serviceId}}",
  "customerName": "Maria Silva",
  "customerPhone": "21999999999",
  "scheduledAt": "{{scheduledAt}}",
  "customerEmail": "maria@example.com"
}
```
**Save the returned `confirmationToken` to `{{confirmationToken}}`**

### 11. Customer Reschedule by Confirmation Token
```
POST /public/bookings/{{confirmationToken}}/reschedule
{
  "scheduledAt": "{{rescheduledAt}}"
}
```

### 12. Customer Cancel by Confirmation Token
```
POST /public/bookings/{{confirmationToken}}/cancel
```

### Legacy Internal Availability
```
GET /availability/next-slots?tenantId={{tenantId}}&serviceId={{serviceId}}&days=7&maxSlots=10
```
Returns available time slots

### Legacy Internal Booking
```
POST /bookings
{
  "tenantId": "{{tenantId}}",
  "serviceId": "{{serviceId}}",
  "customerName": "Maria Silva",
  "customerPhone": "21999999999",
  "scheduledAt": "2026-04-12T10:00:00Z",
  "customerEmail": "maria@example.com",
  "totalAmount": 150.00
}
```

## Collection Organization

### ★ Day 6: Authentication (JWT)
New Day 6 authentication endpoints:
- **Sign Up** - Create new user account
- **Login** - Authenticate and get JWT token
- **Get Current User** - Protected endpoint requiring JWT

### ★ Day 7: Production Validation
Day 7 validation endpoints:
- **Public - List Services by Tenant Slug**
- **Public - Get Availability by Tenant Slug**
- **Public - Create Booking with Confirmation Token**
- **Customer - Reschedule by Confirmation Token**
- **Customer - Cancel by Confirmation Token**
- **Private - Reschedule Request**
- **Private - Cancel Request**

### Health & Setup
- **Health Check** - Verify API is running
- **Database Connection Check** - Verify DB connectivity

### Tenants
- **Create Tenant** - Create new business account
- **List Tenants** - Get all tenants

### Services
- **Create Service** - Add service to tenant
- **List Services** - Get services by tenant

### Customers
- **Create Customer** - Add customer
- **List Customers** - Get customers by tenant

### Business Hours
- **Set Business Hours** - Configure operating hours per day
- **Get Business Hours** - Retrieve configured hours

### Google Calendar Integration
- **Start OAuth Flow** - Initiate Google auth
- **OAuth Callback** - Complete Google auth
- **List Calendar Connections** - Get connected calendars
- **Get Google Busy Slots** - Fetch busy times from Google

### Availability
- **Get Next Available Slots** - Get available time slots
- **Get Google Calendar Next Slots** - Slots excluding Google events

### Requests
- **Create Request** - Create booking request manually
- **List Requests** - Get all requests by tenant
- **Sync Request to Google Calendar** - Sync booking to Google

### ★ Full Booking Flow (Day 5)
- **Complete Booking with Google Sync** - End-to-end booking with Google Calendar sync

### Day 7 Test Target
- Use `https://api.ayax.com.br` as `baseUrl` for production validation.
- Use the tenant slug created for AyaX in Neon.
- Use the Google Calendar account `contato@ayax.com.br` for production calendar sync validation.
- Keep Render, Neon, Google, and JWT secrets out of Postman collection files. Store sensitive values only in your local Postman environment.

## Authentication Flow (Day 6)

### Without JWT (Public Endpoints)
Endpoints that don't require authentication:
- POST /api/auth/login
- POST /api/auth/signup
- GET /api/health
- GET /db-check
- GET /public/{tenantSlug}/services
- GET /public/{tenantSlug}/availability
- POST /public/{tenantSlug}/bookings
- POST /public/bookings/{confirmationToken}/cancel
- POST /public/bookings/{confirmationToken}/reschedule
- POST /tenants
- GET /tenants
- POST /services
- GET /services
- POST /customers
- GET /customers
- (and other non-protected endpoints)

### With JWT (Protected Endpoints)
Protected endpoints require `Authorization: Bearer {token}` header:
- GET /api/me
- POST /api/requests/{requestId}/cancel
- POST /api/requests/{requestId}/reschedule

### How to Set Authorization

**Option 1: Manual Header**
1. Go to a protected endpoint (e.g., GET /api/me)
2. Click **Headers** tab
3. Add header:
   - Key: `Authorization`
   - Value: `Bearer {{jwtToken}}`

**Option 2: Collection-Level Auth (Recommended)**
1. Right-click collection name → **Edit**
2. Go to **Authorization** tab
3. Select **Bearer Token**
4. Set Token: `{{jwtToken}}`
5. Click **Save**

## Testing Tips

### Save Response to Variables
After each request, save important values:

1. Click **Tests** tab in request
2. Add script:
```javascript
if (pm.response.code === 200 || pm.response.code === 201) {
  var jsonData = pm.response.json();
  pm.environment.set("tenantId", jsonData.id);
  pm.environment.set("jwtToken", jsonData.token);
  // etc.
}
```

### Run Collection in Sequence
1. Click collection name
2. Click **Run**
3. Select requests in order
4. Click **Run AlicIA API Collection**

### Debug Responses
- Click **Tests** tab to see response codes
- Click **Response** to see full response body
- Look at timestamps for timing issues

## Error Codes

| Code | Meaning | Solution |
|------|---------|----------|
| 400 | Bad Request | Check request body format and required fields |
| 401 | Unauthorized | Login first and add JWT token to Authorization header |
| 404 | Not Found | Check IDs are correct (tenantId, serviceId, etc.) |
| 500 | Server Error | Check server logs, restart if needed |

## Common Issues

### "Invalid JWT Token"
- **Solution**: Copy full token from Login response (including `Bearer ` prefix if present)
- Check token hasn't expired (24 hours default)

### "Tenant not found"
- **Solution**: Create tenant first, save ID to `{{tenantId}}`

### "Service not found for this tenant"
- **Solution**: Create service first, ensure serviceId belongs to correct tenant

### "Authorization header missing"
- **Solution**: Add `Authorization: Bearer {{jwtToken}}` header to request

## Google Calendar Setup

### Prerequisites
- Google Cloud Project with Calendar API enabled
- OAuth credentials configured in `appsettings.Development.json`
- Client ID and Client Secret

### Steps
1. Run **Start OAuth Flow** endpoint
2. Copy redirect URL
3. Paste in browser to get authorization code
4. Run **OAuth Callback** with code and tenantId
5. Calendar connection is now active

## Performance Notes

- JWT validation: < 1ms
- Password hashing: 100-200ms (intentional for security)
- Database lookups: 1-5ms
- Google Calendar API calls: 500-2000ms

## Security Notes

- ✅ JWT tokens valid for 24 hours
- ✅ Passwords hashed with PBKDF2-SHA256
- ✅ Never share JWT token - it contains your user ID
- ⚠️ Don't commit tokens to version control
- ⚠️ Use HTTPS in production
- ⚠️ Store sensitive data in Postman environments securely

## Version History

| Version | Changes |
|---------|---------|
| v1.0 | Initial collection (Days 1-4) |
| v1.1 | Added Day 5: Full Booking Flow |
| v1.2 | Added Day 6: JWT Authentication |
| v1.3 | Added Day 7: production validation and confirmation-token lifecycle |

## Support

For issues or questions:
1. Check this guide
2. Review docs in `/docs` folder
3. Check API logs: `cd backend && dotnet run --project src/AlicIA.Api`
4. Verify all variables are set correctly
