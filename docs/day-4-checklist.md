# Day 4 Checklist

## Goal
Integrate AlicIA with Google Calendar.

## Setup
- [ ] Create Google Cloud project
- [ ] Enable Google Calendar API
- [ ] Configure OAuth consent screen
- [ ] Create OAuth client (Web)

## Domain
- [ ] Use CalendarConnection entity
- [ ] Store RefreshToken securely
- [ ] Store CalendarEmail

## OAuth Flow
- [ ] Create endpoint to start OAuth flow
- [ ] Create callback endpoint
- [ ] Capture authorization code
- [ ] Exchange for tokens
- [ ] Persist RefreshToken

## Google Integration
- [ ] Implement FreeBusy query
- [ ] Implement GetBusySlots method
- [ ] Merge busy slots with BusinessHours

## Booking Sync
- [ ] Create calendar event on booking
- [ ] Store external event id (future)
- [ ] Validate event creation

## Validation
- [ ] Validate OAuth flow works
- [ ] Validate busy slots are returned
- [ ] Validate event is created in calendar

## Docs and commit
- [ ] Update README
- [ ] Commit changes
- [ ] Push to GitHub