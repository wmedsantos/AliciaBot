# Day 2 Checklist

## Goal
Build the first vertical slice of AlicIA's core domain.

## Domain modeling
- [ ] Create Tenant entity
- [ ] Create Service entity
- [ ] Create Customer entity
- [ ] Create Request entity
- [ ] Define RequestType enum
- [ ] Define RequestStatus enum

## Persistence
- [ ] Add DbSet<Tenant> to DbContext
- [ ] Add DbSet<Service> to DbContext
- [ ] Add DbSet<Customer> to DbContext
- [ ] Add DbSet<Request> to DbContext
- [ ] Configure basic relationships
- [ ] Create initial migration
- [ ] Apply migration to PostgreSQL

## API endpoints
- [ ] Create POST /tenants
- [ ] Create GET /tenants
- [ ] Create POST /services
- [ ] Create GET /services
- [ ] Create POST /customers
- [ ] Create GET /customers
- [ ] Create POST /requests
- [ ] Create GET /requests

## Validation
- [ ] Validate request creation with valid tenant/service/customer
- [ ] Validate requests are persisted
- [ ] Validate requests can be listed

## Docs and commit
- [ ] Update README with Day 2 progress
- [ ] Commit all changes
- [ ] Push to GitHub