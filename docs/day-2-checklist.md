# Day 2 Checklist

## Goal
Build the first vertical slice of AlicIA's core domain.

## Domain modeling
- [x] Create Tenant entity
- [x] Create Service entity
- [x] Create Customer entity
- [x] Create Request entity
- [x] Define RequestType enum
- [x] Define RequestStatus enum
- [x] Create CalendarConnection entity

## Persistence
- [x] Add DbSet<Tenant> to DbContext
- [x] Add DbSet<Service> to DbContext
- [x] Add DbSet<Customer> to DbContext
- [x] Add DbSet<Request> to DbContext
- [x] Add DbSet<CalendarConnection> to DbContext
- [x] Configure basic relationships
- [x] Create initial migration
- [x] Apply migration to PostgreSQL
- [x] Create calendar connection migration
- [x] Apply calendar connection migration

## API endpoints
- [X] Create POST /tenants
- [X] Create GET /tenants
- [X] Create POST /services
- [X] Create GET /services
- [X] Create POST /customers
- [X] Create GET /customers
- [X] Create POST /requests
- [X] Create GET /requests

## Validation
- [X] Validate request creation with valid tenant/service/customer
- [X] Validate requests are persisted
- [X] Validate requests can be listed

## Docs and commit
- [x] Update product-definition.md
- [x] Update architecture-decisions.md
- [X] Update README with Day 2 progress
- [X] Commit all changes
- [X] Push to GitHub