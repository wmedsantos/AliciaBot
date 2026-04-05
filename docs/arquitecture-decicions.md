# AlicIA MVP - Architecture Decisions

## Goal
Build a low-cost, maintainable MVP for conversational booking and request handling.

## Stack
- Frontend: Next.js on Vercel
- Backend: ASP.NET Core (.NET 8) on Render
- Database: PostgreSQL on Neon
- Integrations: Google Calendar
- Dynamic configs: PostgreSQL JSONB
- Architecture style: Modular monolith

## Out of scope for MVP Day 1
- WhatsApp integration
- Advanced AI/LLM flows
- Redis
- n8n
- Automated billing
- Microservices

## Google Calendar Integration Decision

### Decision
Google Calendar integration is part of the MVP.

### Reason
AlicIA must not only create bookings, but also understand real-world availability and prevent conflicts.

### Capabilities required in MVP
- Connect tenant calendar using OAuth
- Read calendar busy slots
- Suggest next available booking slots
- Create booking events
- Update or remove events on reschedule/cancel

### Security model
- Google application credentials are stored in environment variables
- Tenant calendar tokens are stored in a separate CalendarConnection entity
- Tenant domain entities must not contain provider secrets directly