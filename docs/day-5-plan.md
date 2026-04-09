# Day 5 Plan - Full Booking Flow

## Goal

Implement the full booking flow from user interaction to confirmed appointment with Google Calendar sync.

This is the first end-to-end business flow of AlicIA.

---

## Context

Up to Day 4, AlicIA can:

- Manage tenants, services, customers, and requests
- Calculate availability based on:
  - BusinessHours
  - Existing requests
  - Google Calendar busy slots
- Connect to Google Calendar via OAuth
- Create events manually

What is missing:

- A unified booking flow
- Automatic customer handling
- Atomic booking + calendar sync

---

## Objective

Create a single endpoint that:

1. Receives booking intent
2. Validates availability
3. Creates or reuses customer
4. Creates request
5. Creates Google Calendar event
6. Persists ExternalEventId
7. Returns confirmation

---

## Endpoint Design

### POST /bookings

Request:

```json
{
  "tenantId": "guid",
  "serviceId": "guid",
  "customerName": "string",
  "customerPhone": "string",
  "scheduledAt": "datetime"
}
```
Response Design:
```json
{
  "status": "confirmed",
  "service": "Design de Sobrancelhas",
  "customer": "Maria Silva",
  "scheduledAt": "2026-04-11T10:00:00Z"
}
```
### Business Rules
#### Tenant validation
- Tenant must exist
#### Service validation
- Service must belong to tenant
#### Availability validation
- Slot must still be available at booking time
- Must check:
-- BusinessHours
-- Google busy slots
-- Existing requests
#### Customer handling
-- If customer exists (same phone + tenant):
→ reuse
-- Else:
→ create new
#### Request creation
-- Type = Booking
-- Status = Confirmed
-- ScheduledAt = provided slot
#### Google Calendar sync
-- Create event
-- Use:
--- Service name as summary
--- Customer name in description
-- Store ExternalEventId
## Flow (Atomic)
1. Validate tenant
2. Validate service
3. Validate slot availability
4. Get or create customer
5. Create request (without ExternalEventId)
6. Create Google Calendar event
7. Update request with ExternalEventId
8. Return confirmation response

## Failure Strategy (MVP)
-- If Google event creation fails:
→ return error
→ do not confirm booking

(No retry logic for now)

---

## Out of Scope (Day 5)

- Payment integration
- WhatsApp integration
- AI conversation
- Rescheduling flow
- Cancellation flow
- Retry logic
- Transaction management

## Success Criteria
- Booking endpoint works end-to-end
- Prevents double booking
- Creates Google Calendar event
- Persists request correctly
- Returns confirmation

## Philosophy
- Keep it simple
- One endpoint = one business action
- void overengineering
- Focus on working system