# AlicIA - Product Definition Document

## Overview

AlicIA is a conversational operational agent designed to help small businesses automate customer interactions and transform them into structured, actionable records.

The system allows businesses to:

- Schedule services
- Reserve resources
- Register product orders
- Confirm attendance
- Manage customer interactions

AlicIA is built as a SaaS product under AyaX IT Solutions.

---

## Core Concept

AlicIA is based on the idea that:

> Every customer interaction can be transformed into a structured request.

A **Request** is the central entity of the system.

It represents an intent from a customer and can be one of the following:

- Service Booking
- Resource Reservation
- Product Order

---

## Target Audience

### Primary
- Beauty professionals (eyebrows, barbers, etc.)
- Independent service providers
- Consultants

### Secondary
- Restaurants (table reservations)
- Inns / guesthouses (room reservations)
- Small food vendors (order registration)

---

## MVP Scope

The MVP will focus on:

- Service booking via structured flow
- Tenant-based multi-client support
- PostgreSQL persistence
- Request lifecycle management
- Google Calendar integration for real-world scheduling
- Google Calendar availability lookup
- Next available slot suggestion
- Booking conflict prevention

### Google Calendar Integration (MVP)

The system must be able to:

- Connect a tenant to Google Calendar using OAuth
- Read busy slots from the tenant calendar
- Suggest next available booking slots based on business rules
- Create calendar events when a booking is confirmed
- Update events when rescheduled
- Delete or update events when cancelled

### Out of scope for MVP

- WhatsApp integration
- AI conversational flexibility
- Payment automation
- Advanced UI
- Multi-resource reservations
- Delivery logistics
- Campaign automation

---

## Core Entities

### Tenant
Represents a business using AlicIA.

Fields:
- Id
- Name
- Segment
- Plan
- Status
- CreatedAt

---

### CalendarConnection
Represents a tenant's connection to an external calendar provider.

Fields:
- Id
- TenantId
- Provider
- CalendarEmail
- CalendarId
- RefreshToken
- IsActive
- ConnectedAt

---

### Service
Represents a service offered by a tenant.

Fields:
- Id
- TenantId
- Name
- DurationMinutes
- Price

---

### Customer
Represents an end-user of the tenant.

Fields:
- Id
- TenantId
- Name
- Phone
- Email

---

### Request
Represents a structured interaction.

Fields:
- Id
- TenantId
- CustomerId
- ServiceId
- Type
- Status
- ScheduledAt
- TotalAmount
- CreatedAt

---

## Request Types

- Booking
- Reservation (future)
- Order (future)

---

## Request Status

- Pending
- PendingConfirmation
- Confirmed
- Rescheduled
- Cancelled
- Completed
- NoShow

---

## System Responsibilities

### AlicIA Runtime

- Process customer interaction
- Generate structured requests
- Manage request lifecycle
- Handle confirmations
- Provide booking flow

---

### AyaX Control Plane

- Manage tenants
- Manage plans
- Manage usage
- Manage integrations
- Handle billing (future)

---

## Integration Model

Google Calendar access must use OAuth.

Important:
- Application credentials (ClientId and ClientSecret) belong to AlicIA and must be stored in environment variables.
- Tenant-specific calendar access must be stored as a separate integration entity.
- Tenant entities must not store provider secrets directly.

## Architecture

### Backend
- ASP.NET Core (.NET 8)
- Modular monolith
- REST API

### Frontend
- Next.js (future)
- Vercel hosting

### Database
- PostgreSQL (Neon)
- JSONB for future dynamic configs

---

## Non-Functional Requirements

- Low cost operation
- Easy maintainability
- Fast development cycles
- Multi-tenant support
- Scalable architecture (future)

---

## Constraints

- Single developer project
- Limited budget
- Must reach MVP quickly
- Avoid overengineering

---

## Development Approach

- Vertical slices
- Incremental delivery
- Minimal abstractions
- Working software over perfect design

---

## Future Features

- WhatsApp integration
- AI-driven conversations
- Payment processing
- Subscription plans
- Automated campaigns
- Resource-based reservations
- Order management system

---

## Success Criteria (MVP)

- Able to create a tenant
- Able to create a service
- Able to create a customer
- Able to create a request
- Able to list requests
- System running in production environment

---

## Philosophy

AlicIA is not just a chatbot.

It is an operational system that:

- Reduces manual work
- Increases revenue potential
- Structures customer interaction
- Enables small businesses to scale

---

## Important Notes for AI Assistants

- Keep the design simple
- Do not introduce unnecessary abstractions
- Do not use microservices
- Avoid complex patterns unless required
- Prefer clarity over cleverness
- Focus on working code
