# Day 4 Checklist

## Goal
Integrate AlicIA with Google Calendar.

## Setup
- [X] Create Google Cloud project
- [X] Enable Google Calendar API
- [X] Configure OAuth consent screen
- [X] Create OAuth client (Web)

## Domain
- [X] Use CalendarConnection entity
- [X] Store RefreshToken securely
- [X] Store CalendarEmail

## OAuth Flow
- [X] Create endpoint to start OAuth flow
- [X] Create callback endpoint
- [X] Capture authorization code
- [X] Exchange for tokens
- [X] Persist RefreshToken

## Google Integration
- [X] Implement FreeBusy query
- [X] Implement GetBusySlots method
- [X] Merge busy slots with BusinessHours

## Booking Sync
- [X] Create calendar event on booking
- [X] Store external event id (future)
- [X] Validate event creation

## Validation
- [X] Validate OAuth flow works
- [X] Validate busy slots are returned
- [X] Validate event is created in calendar

## Docs and commit
- [ ] Update README
- [ ] Commit changes
- [ ] Push to GitHub