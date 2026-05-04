# Day 5 Checklist

## Goal
Implement full booking flow with automatic Google Calendar sync.

---

## Context
By Day 4, AlicIA can:

- [x] Manage tenants, services, customers, and requests
- [x] Calculate availability using:
  - [x] BusinessHours
  - [x] Existing requests
  - [x] Google Calendar busy slots
- [x] Connect to Google Calendar via OAuth
- [x] Create Google Calendar events manually

Implemented:

- [x] Unified booking flow
- [x] Automatic customer handling
- [x] Atomic booking + calendar sync

---

## Booking Endpoint

- [x] Create POST /bookings endpoint
- [x] Define request model:
  - [x] tenantId
  - [x] serviceId
  - [x] customerName
  - [x] customerPhone
  - [x] scheduledAt

---

## Validation

- [x] Validate tenant exists
- [x] Validate service belongs to tenant
- [x] Validate scheduledAt is not null
- [x] Validate slot is still available:
  - [x] Check BusinessHours
  - [x] Check Google busy slots
  - [x] Check existing Requests

---

## Customer Handling

- [x] Search customer by:
  - [x] tenantId
  - [x] phone
- [x] If exists:
  - [x] reuse existing customer
- [x] If not:
  - [x] create new customer

---

## Request Creation

- [x] Create Request with:
  - [x] Type = Booking
  - [x] Status = Confirmed
  - [x] ScheduledAt
  - [x] TotalAmount
- [x] Persist request

---

## Google Calendar Sync

- [x] Retrieve CalendarConnection
- [x] Call GoogleCalendarService.CreateEventAsync
- [x] Pass:
  - [x] service name
  - [x] customer name
  - [x] start time
  - [x] end time
- [x] Receive eventId
- [x] Store ExternalEventId in Request

---

## Response

- [x] Return response:
  - [x] status = confirmed
  - [x] service name
  - [x] customer name
  - [x] scheduledAt

---

## Validation (End-to-End)

- [x] Booking creates Request in DB
- [x] Booking creates event in Google Calendar
- [x] ExternalEventId is persisted
- [x] Double booking is prevented

---

## Docs and commit

- [x] Update README with Day 5 progress
- [ ] Commit all changes
- [ ] Push to GitHub
