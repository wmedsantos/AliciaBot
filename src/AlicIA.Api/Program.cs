using AlicIA.Api.Models;
using AlicIA.Domain.Entities;
using AlicIA.Domain.Enums;
using AlicIA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AlicIA.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<TimeSpanStringSchemaFilter>();
});

builder.Services.AddDbContext<AlicIADbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new
{
    name = "AlicIA API",
    status = "healthy",
    utc = DateTime.UtcNow
}));

app.MapGet("/db-check", async (AlicIADbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();

    return Results.Ok(new
    {
        database = "postgres",
        connected = canConnect,
        utc = DateTime.UtcNow
    });
});

app.MapPost("/tenants", async (CreateTenantRequest request, AlicIADbContext db) =>
{
    var tenant = new Tenant
    {
        Name = request.Name,
        Segment = request.Segment,
        Plan = request.Plan,
        Status = request.Status
    };

    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();

    return Results.Created($"/tenants/{tenant.Id}", tenant);
});

app.MapGet("/tenants", async (AlicIADbContext db) =>
{
    var tenants = await db.Tenants
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(tenants);
});

app.MapPost("/services", async (CreateServiceRequest request, AlicIADbContext db) =>
{
    var tenantExists = await db.Tenants.AnyAsync(x => x.Id == request.TenantId);
    if (!tenantExists)
        return Results.BadRequest(new { error = "Tenant not found." });

    var service = new Service
    {
        TenantId = request.TenantId,
        Name = request.Name,
        DurationMinutes = request.DurationMinutes,
        Price = request.Price
    };

    db.Services.Add(service);
    await db.SaveChangesAsync();

    return Results.Created($"/services/{service.Id}", service);
});

app.MapGet("/services", async (Guid? tenantId, AlicIADbContext db) =>
{
    var query = db.Services.AsQueryable();

    if (tenantId.HasValue)
        query = query.Where(x => x.TenantId == tenantId.Value);

    var services = await query
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(services);
});

app.MapPost("/customers", async (CreateCustomerRequest request, AlicIADbContext db) =>
{
    var tenantExists = await db.Tenants.AnyAsync(x => x.Id == request.TenantId);
    if (!tenantExists)
        return Results.BadRequest(new { error = "Tenant not found." });

    var customer = new Customer
    {
        TenantId = request.TenantId,
        Name = request.Name,
        Phone = request.Phone,
        Email = request.Email
    };

    db.Customers.Add(customer);
    await db.SaveChangesAsync();

    return Results.Created($"/customers/{customer.Id}", customer);
});

app.MapGet("/customers", async (Guid? tenantId, AlicIADbContext db) =>
{
    var query = db.Customers.AsQueryable();

    if (tenantId.HasValue)
        query = query.Where(x => x.TenantId == tenantId.Value);

    var customers = await query
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(customers);
});

app.MapPost("/requests", async (CreateRequestRequest request, AlicIADbContext db) =>
{
    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == request.TenantId);
    if (tenant is null)
        return Results.BadRequest(new { error = "Tenant not found." });

    var customer = await db.Customers
        .FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.TenantId == request.TenantId);

    if (customer is null)
        return Results.BadRequest(new { error = "Customer not found for this tenant." });

    var service = await db.Services
        .FirstOrDefaultAsync(x => x.Id == request.ServiceId && x.TenantId == request.TenantId);

    if (service is null)
        return Results.BadRequest(new { error = "Service not found for this tenant." });

    var entity = new Request
    {
        TenantId = request.TenantId,
        CustomerId = request.CustomerId,
        ServiceId = request.ServiceId,
        Type = request.Type,
        Status = request.Status,
        ScheduledAt = request.ScheduledAt,
        TotalAmount = request.TotalAmount ?? service.Price
    };

    db.Requests.Add(entity);
    await db.SaveChangesAsync();

    return Results.Created($"/requests/{entity.Id}", new
    {
        entity.Id,
        entity.TenantId,
        TenantName = tenant.Name,
        entity.CustomerId,
        CustomerName = customer.Name,
        entity.ServiceId,
        ServiceName = service.Name,
        entity.Type,
        entity.Status,
        entity.ScheduledAt,
        entity.TotalAmount,
        entity.CreatedAt
    });
});

app.MapGet("/requests", async (Guid? tenantId, AlicIADbContext db) =>
{
    var query = db.Requests
        .Include(x => x.Tenant)
        .Include(x => x.Customer)
        .Include(x => x.Service)
        .AsQueryable();

    if (tenantId.HasValue)
        query = query.Where(x => x.TenantId == tenantId.Value);

    var requests = await query
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new
        {
            x.Id,
            x.TenantId,
            TenantName = x.Tenant!.Name,
            x.CustomerId,
            CustomerName = x.Customer!.Name,
            x.ServiceId,
            ServiceName = x.Service!.Name,
            x.Type,
            x.Status,
            x.ScheduledAt,
            x.TotalAmount,
            x.CreatedAt
        })
        .ToListAsync();

    return Results.Ok(requests);
});

app.MapPost("/business-hours", async (BusinessHoursCreateRequest request, AlicIADbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.StartTime) || string.IsNullOrWhiteSpace(request.EndTime))
    {
        return Results.BadRequest(new { error = "StartTime and EndTime are required as strings, e.g. '08:00:00'." });
    }

    if (!TimeSpan.TryParse(request.StartTime, out var startTime))
    {
        return Results.BadRequest(new { error = "StartTime is not a valid time span. Use format 'HH:mm:ss'." });
    }

    if (!TimeSpan.TryParse(request.EndTime, out var endTime))
    {
        return Results.BadRequest(new { error = "EndTime is not a valid time span. Use format 'HH:mm:ss'." });
    }

    if (startTime >= endTime)
    {
        return Results.BadRequest(new { error = "StartTime must be before EndTime." });
    }

    var tenantExists = await db.Tenants.AnyAsync(x => x.Id == request.TenantId);
    if (!tenantExists)
    {
        return Results.NotFound(new { error = "Tenant not found." });
    }

    var businessHours = new BusinessHours
    {
        TenantId = request.TenantId,
        DayOfWeek = request.DayOfWeek,
        StartTime = startTime,
        EndTime = endTime,
        IsActive = true
    };

    db.BusinessHours.Add(businessHours);
    await db.SaveChangesAsync();

    return Results.Created($"/business-hours/{businessHours.Id}", new BusinessHoursResponse
    {
        Id = businessHours.Id,
        TenantId = businessHours.TenantId,
        DayOfWeek = businessHours.DayOfWeek,
        StartTime = businessHours.StartTime.ToString(),
        EndTime = businessHours.EndTime.ToString(),
        IsActive = businessHours.IsActive
    });
});

app.MapGet("/business-hours", async (Guid tenantId, AlicIADbContext db) =>
{
    var hours = await db.BusinessHours
        .Where(x => x.TenantId == tenantId && x.IsActive)
        .OrderBy(x => x.DayOfWeek)
        .ThenBy(x => x.StartTime)
        .Select(x => new BusinessHoursResponse
        {
            Id = x.Id,
            TenantId = x.TenantId,
            DayOfWeek = x.DayOfWeek,
            StartTime = x.StartTime.ToString(),
            EndTime = x.EndTime.ToString(),
            IsActive = x.IsActive
        })
        .ToListAsync();

    return Results.Ok(hours);
});

app.MapGet("/availability/next-slots", async (
    Guid tenantId,
    Guid serviceId,
    AlicIADbContext db,
    DateTime? startDate = null,
    int days = 7,
    int maxSlots = 10) =>
{
    if (days <= 0)
    {
        return Results.BadRequest(new { error = "Days must be greater than zero." });
    }

    if (maxSlots <= 0)
    {
        return Results.BadRequest(new { error = "MaxSlots must be greater than zero." });
    }

    var service = await db.Services.FirstOrDefaultAsync(x => x.Id == serviceId && x.TenantId == tenantId);
    if (service is null)
    {
        return Results.NotFound(new { error = "Service not found for this tenant." });
    }

    var businessHours = await db.BusinessHours
        .Where(x => x.TenantId == tenantId && x.IsActive)
        .ToListAsync();

    if (!businessHours.Any())
    {
        return Results.BadRequest(new { error = "No active business hours found for tenant." });
    }

    var scheduledRequests = await db.Requests
        .Include(x => x.Service)
        .Where(x => x.TenantId == tenantId && x.ScheduledAt != null && x.Status != RequestStatus.Cancelled)
        .ToListAsync();

    var windowStart = (startDate ?? DateTime.UtcNow).ToUniversalTime();
    var searchStartDate = windowStart.Date;
    var duration = TimeSpan.FromMinutes(service.DurationMinutes);
    var results = new List<SlotResponse>();
    var now = DateTime.UtcNow;

    for (var dayIndex = 0; dayIndex < days && results.Count < maxSlots; dayIndex++)
    {
        var currentDate = searchStartDate.AddDays(dayIndex);
        var dayHours = businessHours.Where(x => x.DayOfWeek == currentDate.DayOfWeek).ToList();
        if (!dayHours.Any())
        {
            continue;
        }

        foreach (var period in dayHours.OrderBy(x => x.StartTime))
        {
            var slotStart = currentDate.Add(period.StartTime);
            if (dayIndex == 0 && slotStart < windowStart)
            {
                slotStart = windowStart;
            }

            var endBoundary = currentDate.Add(period.EndTime);
            while (slotStart + duration <= endBoundary && results.Count < maxSlots)
            {
                var slotEnd = slotStart + duration;
                if (!IsSlotOccupied(slotStart, slotEnd, scheduledRequests))
                {
                    results.Add(new SlotResponse { Start = slotStart, End = slotEnd });
                }

                slotStart = slotStart.AddMinutes(15);
            }

            if (results.Count >= maxSlots)
                break;
        }
    }

    return Results.Ok(results);
});

app.Run();

static bool IsSlotOccupied(DateTime slotStart, DateTime slotEnd, List<Request> scheduledRequests)
{
    foreach (var existing in scheduledRequests)
    {
        if (!existing.ScheduledAt.HasValue)
            continue;

        var existingStart = existing.ScheduledAt.Value.ToUniversalTime();
        var existingDuration = TimeSpan.FromMinutes(existing.Service?.DurationMinutes ?? 0);
        if (existingDuration == TimeSpan.Zero)
            continue;

        var existingEnd = existingStart + existingDuration;
        if (slotStart < existingEnd && existingStart < slotEnd)
            return true;
    }

    return false;
}

public record CreateTenantRequest(
    string Name,
    string Segment,
    string Plan,
    string Status
);

public record CreateServiceRequest(
    Guid TenantId,
    string Name,
    int DurationMinutes,
    decimal Price
);

public record CreateCustomerRequest(
    Guid TenantId,
    string Name,
    string Phone,
    string? Email
);

public record CreateRequestRequest(
    Guid TenantId,
    Guid CustomerId,
    Guid ServiceId,
    RequestType Type,
    RequestStatus Status,
    DateTime? ScheduledAt,
    decimal? TotalAmount
);