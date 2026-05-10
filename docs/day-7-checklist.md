# Day 7 Checklist - Finish MVP Backend

## Goal

Finish the AlicIA MVP backend by completing booking lifecycle APIs, tightening production configuration, and preparing the backend for a customer-facing frontend.

---

## Context

By Day 6, AlicIA can:

- [x] Authenticate business users with JWT
- [x] Protect private API endpoints
- [x] Scope private data by tenant claims
- [x] Expose public customer booking APIs by tenant slug
- [x] Create confirmed bookings
- [x] Sync confirmed bookings to Google Calendar

What is still missing for the MVP backend:

- [x] Cancel booking flow
- [x] Reschedule booking flow
- [x] Google Calendar event update/delete support
- [ ] Runtime validation of Day 6 auth and public/private APIs
- [x] Production-safe secret handling
- [x] Render production environment variable documentation
- [x] Production API live and secure at `https://api.ayax.com.br`

---

## Day 7 Decisions

- [x] Repository is organized as `backend/`, `frontend/`, and `docs/` so frontend work can start without changing backend configuration later.
- [x] Booking cancel/reschedule is allowed only for the client who owns the booking and the tenant/business user.
- [x] Public customer cancel/reschedule will use a confirmation token, not raw tenant/customer/request IDs.
- [x] API documentation should stay lightweight because the API is consumed only by AlicIABot frontend and the AyaX admin site.
- [x] Production secrets must be rotated for the production Render/Neon environment.
- [x] Day 7 uses only the Neon production database; local Docker database is deferred to Day 8.
- [x] Tenant slugs should use a common lowercase kebab-case format, for example `ayax-consulting`.
- [x] Runtime validation must cover full auth, public booking, and booking lifecycle flows with Postman.
- [x] Google Calendar production validation will use `contato@ayax.com.br`.
- [x] README should not include production secrets or detailed production deployment instructions.
- [x] Postman collection updates are required for Day 7 validation.
- [x] Shared availability extraction is accepted as technical debt for the next days and is not a Day 7 blocker.
- [x] Day 7 is complete when production is live and the full auth/public booking/lifecycle test passes.

---

## Booking Lifecycle APIs

### Cancel Booking

- [x] Create `POST /api/requests/{requestId}/cancel` for private users
- [x] Validate request belongs to tenant from JWT claims
- [x] Reject cancellation for already completed/no-show requests
- [x] Set request status to `Cancelled`
- [x] If `ExternalEventId` exists, delete or cancel the Google Calendar event
- [x] Return cancellation confirmation response

### Reschedule Booking

- [x] Create `POST /api/requests/{requestId}/reschedule` for private users
- [x] Define request model:
  - [x] `scheduledAt`
- [x] Validate request belongs to tenant from JWT claims
- [x] Validate request is eligible for reschedule
- [x] Validate new slot is inside business hours
- [x] Validate new slot does not overlap existing requests
- [x] Validate new slot does not overlap Google Calendar busy slots
- [x] Update `ScheduledAt`
- [x] Set status to `Rescheduled` or keep `Confirmed` after successful sync
- [x] If `ExternalEventId` exists, update the Google Calendar event
- [x] If no `ExternalEventId` exists, create a Google Calendar event
- [x] Return reschedule confirmation response

### Optional Public Customer Lifecycle

- [x] Decide whether public cancel/reschedule is in MVP
- [x] Use confirmation tokens for public customer cancel/reschedule
- [x] Implement public customer cancel/reschedule with confirmation tokens
- [x] Avoid exposing raw tenant/customer/request data publicly

---

## Google Calendar Integration

- [x] Implement `GoogleCalendarService.UpdateEventAsync`
- [x] Implement `GoogleCalendarService.DeleteEventAsync`
- [x] Use `CalendarConnection.CalendarId` with safe fallback to `primary`
- [x] Handle missing/deleted Google events gracefully
- [x] Avoid duplicate event creation during reschedule
- [x] Keep `ExternalEventId` accurate after update/delete

---

## Backend Consistency

- [ ] Tech debt for next days: extract shared booking availability logic used by:
  - [ ] Public availability lookup
  - [ ] Public booking creation
  - [ ] Private reschedule
- [x] Ensure all date/time comparisons use UTC consistently
- [x] Ensure all private endpoints use tenantId from JWT claims
- [x] Ensure public endpoints only expose customer-safe fields
- [x] Keep Swagger/OpenAPI updates lightweight for internal frontend/admin usage

---

## Secrets And Environment Variables

### Local Configuration

- [x] Keep committed `appsettings.json` and `appsettings.Development.json` free of real secrets
- [x] Add documented local environment variables:
  - [x] `ConnectionStrings__DefaultConnection`
  - [x] `Jwt__Secret`
  - [x] `Jwt__Issuer`
  - [x] `Jwt__Audience`
  - [x] `Jwt__ExpirationMinutes`
  - [x] `Google__ClientId`
  - [x] `Google__ClientSecret`
  - [x] `Google__RedirectUri`
- [x] Add `.env.example` or docs section with placeholder values only

### Render Production Environment

- [x] Create the AyaX Render account
- [x] Create the production Render Web Service for the AlicIA backend
- [x] Configure Render to deploy from the `backend/` root directory
- [x] Add backend Dockerfile for Render Docker deploys
- [x] Build backend Docker image locally
- [x] Smoke-test backend Docker container with `/health`
- [x] Configure Render production environment variables:
  - [x] `ConnectionStrings__DefaultConnection`
  - [x] `Jwt__Secret`
  - [x] `Jwt__Issuer`
  - [x] `Jwt__Audience`
  - [x] `Jwt__ExpirationMinutes`
  - [x] `Google__ClientId`
  - [x] `Google__ClientSecret`
  - [x] `Google__RedirectUri`
  - [x] `ASPNETCORE_ENVIRONMENT`
- [x] Set `ASPNETCORE_ENVIRONMENT` to `Production`
- [x] Set `Google__RedirectUri` to the Render production callback URL
- [x] Add the Render production callback URL to Google Cloud OAuth authorized redirect URIs
- [x] Use GitHub only as the source repository for Render deploys; do not store runtime secrets in GitHub for now
- [x] Confirm production API is live at `https://api.ayax.com.br`
- [x] Confirm production API uses HTTPS
- [ ] Rotate production secrets in Render/Neon/Google as needed
- [x] Verify no live secrets remain in tracked files

---

## Validation

### Build And Migration

- [x] Run `dotnet build`
- [x] Run EF migration list
- [ ] Apply pending migrations to Neon production database, including confirmation-token migration
- [ ] Verify tenant slug migration works in Neon production database
- [ ] Defer local Docker database setup to Day 8

### Auth Validation

- [ ] Run full validation with Postman against `https://api.ayax.com.br`
- [ ] Create tenant
- [ ] Signup user
- [ ] Login user
- [ ] Call private endpoint without token and confirm `401`
- [ ] Call private endpoint with token and confirm success
- [ ] Confirm private endpoint cannot access another tenant's records

### Public Booking Validation

- [ ] List public services by tenant slug
- [ ] Confirm tenant slug uses lowercase kebab-case format
- [ ] Get public availability by tenant slug
- [ ] Create public booking
- [ ] Confirm customer is created or reused
- [ ] Confirm request is created as booking
- [ ] Confirm Google Calendar event is created

### Lifecycle Validation

- [ ] Cancel request and confirm status changes to `Cancelled`
- [ ] Cancel request and confirm Google Calendar event is removed/cancelled
- [ ] Reschedule request and confirm new `ScheduledAt`
- [ ] Reschedule request and confirm Google Calendar event is updated
- [ ] Attempt double booking and confirm it is rejected
- [ ] Validate Google Calendar sync with `contato@ayax.com.br`

---

## Docs

- [x] Move .NET backend solution and source under `backend/`
- [x] Create `frontend/` placeholder for Day 8 frontend work
- [x] Update README endpoint list for Day 7
- [x] Keep README production details minimal; do not document secrets or detailed Render setup
- [x] Update Postman collection for:
  - [x] Public services
  - [x] Public availability
  - [x] Public booking
  - [x] Customer cancel with confirmation token
  - [x] Customer reschedule with confirmation token
  - [x] Private cancel
  - [x] Private reschedule
- [x] Add environment variable setup instructions
- [ ] Add only high-level production notes, without sensitive Render/Neon details

---

## Success Criteria

- [x] MVP backend supports create, cancel, and reschedule booking flows
- [x] Google Calendar remains in sync with booking lifecycle changes
- [x] Private APIs are tenant-isolated through JWT claims
- [x] Public APIs expose only customer-safe data
- [x] No real secrets are committed
- [x] Render production environment uses environment variables for sensitive config
- [x] Production API is live and secure at `https://api.ayax.com.br`
- [x] Build passes
- [ ] Full auth, public booking, and lifecycle flow is validated end-to-end with Postman

---

## Docs and commit

- [ ] Update README with Day 7 progress
- [ ] Commit all changes
- [ ] Push to GitHub
