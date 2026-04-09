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

Missing:

- [ ] Unified booking flow
- [ ] Automatic customer handling
- [ ] Atomic booking + calendar sync

---

## Booking Endpoint

- [ ] Create POST /bookings endpoint
- [ ] Define request model:
  - [ ] tenantId
  - [ ] serviceId
  - [ ] customerName
  - [ ] customerPhone
  - [ ] scheduledAt

---

## Validation

- [ ] Validate tenant exists
- [ ] Validate service belongs to tenant
- [ ] Validate scheduledAt is not null
- [ ] Validate slot is still available:
  - [ ] Check BusinessHours
  - [ ] Check Google busy slots
  - [ ] Check existing Requests

---

## Customer Handling

- [ ] Search customer by:
  - [ ] tenantId
  - [ ] phone
- [ ] If exists:
  - [ ] reuse existing customer
- [ ] If not:
  - [ ] create new customer

---

## Request Creation

- [ ] Create Request with:
  - [ ] Type = Booking
  - [ ] Status = Confirmed
  - [ ] ScheduledAt
  - [ ] TotalAmount
- [ ] Persist request

---

## Google Calendar Sync

- [ ] Retrieve CalendarConnection
- [ ] Call GoogleCalendarService.CreateEventAsync
- [ ] Pass:
  - [ ] service name
  - [ ] customer name
  - [ ] start time
  - [ ] end time
- [ ] Receive eventId
- [ ] Store ExternalEventId in Request

---

## Response

- [ ] Return response:
  - [ ] status = confirmed
  - [ ] service name
  - [ ] customer name
  - [ ] scheduledAt

---

## Validation (End-to-End)

- [ ] Booking creates Request in DB
- [ ] Booking creates event in Google Calendar
- [ ] ExternalEventId is persisted
- [ ] Double booking is prevented

---

## Docs and commit

- [ ] Update README with Day 5 progress
- [ ] Commit all changes
- [ ] Push to GitHub