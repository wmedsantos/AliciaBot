# Day 3 Checklist

## Goal
Implement availability rules and next available slot calculation.

## Domain modeling
- [ ] Create BusinessHours entity
- [ ] Add BusinessHours to DbContext
- [ ] Configure relationship with Tenant
- [ ] Create migration for BusinessHours
- [ ] Apply migration

## API endpoints
- [ ] Create POST /business-hours
- [ ] Create GET /business-hours
- [ ] Create GET /availability/next-slots

## Business rules
- [ ] Filter by tenant business hours
- [ ] Exclude occupied scheduled slots from requests
- [ ] Return next available slots based on service duration

## Validation
- [ ] Validate business hours persistence
- [ ] Validate slot generation
- [ ] Validate occupied slots are excluded

## Docs and commit
- [ ] Update README with Day 3 progress
- [ ] Commit all changes
- [ ] Push to GitHub