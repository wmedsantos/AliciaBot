using AlicIA.Api.Models;
using AlicIA.Domain.Entities;
using AlicIA.Domain.Enums;
using AlicIA.Infrastructure.Persistence;
using AlicIA.Infrastructure.Integrations;
using AlicIA.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using AlicIA.Api.Swagger;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<TimeSpanStringSchemaFilter>();
});

builder.Services.AddDbContext<AlicIADbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<GoogleCalendarService>();

// Add security services
builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Configure JWT authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AlicIA";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AlicIA";

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

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

// Authentication endpoints (public)
app.MapPost("/api/auth/login", async (
    LoginRequest request,
    IPasswordHasher passwordHasher,
    IJwtAuthService jwtService,
    AlicIADbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { error = "Email and password are required." });

    var user = await db.Users.FirstOrDefaultAsync(x => 
        x.TenantId == request.TenantId && x.Email == request.Email.ToLowerInvariant());

    if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = jwtService.GenerateToken(user.Id, user.Email, user.TenantId, user.Role);
    var expiresAt = DateTime.UtcNow.AddMinutes(1440); // Configurable

    return Results.Ok(new LoginResponse(
        Token: token,
        Email: user.Email,
        TenantId: user.TenantId,
        Role: user.Role,
        ExpiresAt: expiresAt
    ));
}).WithName("Login").WithOpenApi();

app.MapPost("/api/auth/signup", async (
    CreateUserRequest request,
    IPasswordHasher passwordHasher,
    IJwtAuthService jwtService,
    AlicIADbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { error = "Email and password are required." });

    if (request.Password.Length < 8)
        return Results.BadRequest(new { error = "Password must be at least 8 characters." });

    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == request.TenantId);
    if (tenant is null)
        return Results.NotFound(new { error = "Tenant not found." });

    var existingUser = await db.Users.FirstOrDefaultAsync(x =>
        x.TenantId == request.TenantId && x.Email == request.Email.ToLowerInvariant());

    if (existingUser is not null)
        return Results.BadRequest(new { error = "User already exists for this tenant." });

    var user = new User
    {
        TenantId = request.TenantId,
        Email = request.Email.ToLowerInvariant(),
        PasswordHash = passwordHasher.HashPassword(request.Password),
        Role = request.Role
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    var token = jwtService.GenerateToken(user.Id, user.Email, user.TenantId, user.Role);
    var expiresAt = DateTime.UtcNow.AddMinutes(1440);

    return Results.Created($"/api/users/{user.Id}", new LoginResponse(
        Token: token,
        Email: user.Email,
        TenantId: user.TenantId,
        Role: user.Role,
        ExpiresAt: expiresAt
    ));
}).WithName("Signup").WithOpenApi();

// Protected endpoint - Get current user info
app.MapGet("/api/me", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var tenantIdClaim = httpContext.User.FindFirst("tenantId")?.Value;

    if (!Guid.TryParse(userIdClaim, out var userId) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        return Results.Unauthorized();

    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId);
    if (user is null)
        return Results.NotFound();

    return Results.Ok(new
    {
        user.Id,
        user.Email,
        user.TenantId,
        user.Role,
        user.CreatedAt
    });
}).RequireAuthorization().WithName("GetCurrentUser").WithOpenApi();

app.MapPost("/tenants", async (CreateTenantRequest request, AlicIADbContext db) =>
{
    var slug = NormalizeSlug(request.Slug ?? request.Name);
    if (string.IsNullOrWhiteSpace(slug))
        return Results.BadRequest(new { error = "Tenant slug could not be generated." });

    var slugExists = await db.Tenants.AnyAsync(x => x.Slug == slug);
    if (slugExists)
        return Results.BadRequest(new { error = "Tenant slug already exists." });

    var tenant = new Tenant
    {
        Name = request.Name,
        Slug = slug,
        Segment = request.Segment,
        Plan = request.Plan,
        Status = request.Status
    };

    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();

    return Results.Created($"/tenants/{tenant.Id}", tenant);
});

app.MapGet("/api/tenants", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var tenants = await db.Tenants
        .Where(x => x.Id == tenantId.Value)
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(tenants);
}).RequireAuthorization();

app.MapPost("/api/services", async (CreateServiceRequest request, HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var tenantExists = await db.Tenants.AnyAsync(x => x.Id == tenantId.Value);
    if (!tenantExists)
        return Results.BadRequest(new { error = "Tenant not found." });

    var service = new Service
    {
        TenantId = tenantId.Value,
        Name = request.Name,
        DurationMinutes = request.DurationMinutes,
        Price = request.Price
    };

    db.Services.Add(service);
    await db.SaveChangesAsync();

    return Results.Created($"/services/{service.Id}", service);
}).RequireAuthorization();

app.MapGet("/api/services", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var services = await db.Services
        .Where(x => x.TenantId == tenantId.Value)
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(services);
}).RequireAuthorization();

app.MapPost("/api/customers", async (CreateCustomerRequest request, HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var tenantExists = await db.Tenants.AnyAsync(x => x.Id == tenantId.Value);
    if (!tenantExists)
        return Results.BadRequest(new { error = "Tenant not found." });

    var customer = new Customer
    {
        TenantId = tenantId.Value,
        Name = request.Name,
        Phone = request.Phone,
        Email = request.Email
    };

    db.Customers.Add(customer);
    await db.SaveChangesAsync();

    return Results.Created($"/customers/{customer.Id}", customer);
}).RequireAuthorization();

app.MapGet("/api/customers", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var customers = await db.Customers
        .Where(x => x.TenantId == tenantId.Value)
        .OrderBy(x => x.Name)
        .ToListAsync();

    return Results.Ok(customers);
}).RequireAuthorization();

app.MapPost("/api/requests", async (CreateRequestRequest request, HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId.Value);
    if (tenant is null)
        return Results.BadRequest(new { error = "Tenant not found." });

    var customer = await db.Customers
        .FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.TenantId == tenantId.Value);

    if (customer is null)
        return Results.BadRequest(new { error = "Customer not found for this tenant." });

    var service = await db.Services
        .FirstOrDefaultAsync(x => x.Id == request.ServiceId && x.TenantId == tenantId.Value);

    if (service is null)
        return Results.BadRequest(new { error = "Service not found for this tenant." });

    var entity = new Request
    {
        TenantId = tenantId.Value,
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
}).RequireAuthorization();

app.MapGet("/api/requests", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var requests = await db.Requests
        .Include(x => x.Tenant)
        .Include(x => x.Customer)
        .Include(x => x.Service)
        .Where(x => x.TenantId == tenantId.Value)
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
}).RequireAuthorization();

app.MapPost("/api/requests/{requestId:guid}/sync-google-event", async (
    Guid requestId,
    HttpContext httpContext,
    GoogleCalendarService googleCalendarService,
    AlicIADbContext db,
    CancellationToken cancellationToken) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var request = await db.Requests
        .Include(x => x.Customer)
        .Include(x => x.Service)
        .FirstOrDefaultAsync(x => x.Id == requestId && x.TenantId == tenantId.Value, cancellationToken);

    if (request is null)
        return Results.NotFound(new { error = "Request not found." });

    if (request.ScheduledAt is null)
        return Results.BadRequest(new { error = "Request does not have a scheduled time." });

    var connection = await db.CalendarConnections
        .FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId && x.Provider == "Google" && x.IsActive,
            cancellationToken);

    if (connection is null)
        return Results.BadRequest(new { error = "Active Google Calendar connection not found for this tenant." });

    var startUtc = request.ScheduledAt.Value;
    var endUtc = startUtc.AddMinutes(request.Service!.DurationMinutes);

    var summary = request.Service.Name;
    var description = $"Customer: {request.Customer!.Name} | Phone: {request.Customer.Phone}";

    var eventId = await googleCalendarService.CreateEventAsync(
        connection,
        summary,
        startUtc,
        endUtc,
        description,
        cancellationToken);

    request.ExternalEventId = eventId;
    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(new
    {
        message = "Google Calendar event created successfully.",
        requestId = request.Id,
        externalEventId = eventId,
        startUtc,
        endUtc
    });
}).RequireAuthorization();

app.MapPost("/api/business-hours", async (BusinessHoursCreateRequest request, HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

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

    var tenantExists = await db.Tenants.AnyAsync(x => x.Id == tenantId.Value);
    if (!tenantExists)
    {
        return Results.NotFound(new { error = "Tenant not found." });
    }

    var businessHours = new BusinessHours
    {
        TenantId = tenantId.Value,
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
}).RequireAuthorization();

app.MapGet("/api/business-hours", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var hours = await db.BusinessHours
        .Where(x => x.TenantId == tenantId.Value && x.IsActive)
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
}).RequireAuthorization();

app.MapGet("/api/availability/next-slots", async (
    Guid serviceId,
    HttpContext httpContext,
    AlicIADbContext db,
    DateTime? startDate = null,
    int days = 7,
    int maxSlots = 10) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    if (days <= 0)
    {
        return Results.BadRequest(new { error = "Days must be greater than zero." });
    }

    if (maxSlots <= 0)
    {
        return Results.BadRequest(new { error = "MaxSlots must be greater than zero." });
    }

    var service = await db.Services.FirstOrDefaultAsync(x => x.Id == serviceId && x.TenantId == tenantId.Value);
    if (service is null)
    {
        return Results.NotFound(new { error = "Service not found for this tenant." });
    }

    var businessHours = await db.BusinessHours
        .Where(x => x.TenantId == tenantId.Value && x.IsActive)
        .ToListAsync();

    if (!businessHours.Any())
    {
        return Results.BadRequest(new { error = "No active business hours found for tenant." });
    }

    var scheduledRequests = await db.Requests
        .Include(x => x.Service)
        .Where(x => x.TenantId == tenantId.Value && x.ScheduledAt != null && x.Status != RequestStatus.Cancelled)
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
}).RequireAuthorization();

app.MapGet("/api/oauth/google/start", (HttpContext httpContext, IConfiguration config) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var clientId = config["Google:ClientId"];
    var redirectUri = config["Google:RedirectUri"];

    var url = "https://accounts.google.com/o/oauth2/v2/auth" +
        $"?client_id={Uri.EscapeDataString(clientId!)}" +
        $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
        $"&response_type=code" +
        $"&scope={Uri.EscapeDataString("https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/userinfo.email")}" +
        $"&access_type=offline" +
        $"&prompt=consent" +
        $"&state={tenantId.Value}";

    return Results.Redirect(url);
}).RequireAuthorization();

app.MapGet("/oauth/google/callback", async (
    string code,
    string state,
    IConfiguration config,
    AlicIADbContext db) =>
{
    if (!Guid.TryParse(state, out var tenantId))
        return Results.BadRequest(new { error = "Invalid tenant state." });

    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId);
    if (tenant is null)
        return Results.BadRequest(new { error = "Tenant not found." });

    var clientId = config["Google:ClientId"];
    var clientSecret = config["Google:ClientSecret"];
    var redirectUri = config["Google:RedirectUri"];

    using var http = new HttpClient();

    var tokenResponse = await http.PostAsync(
        "https://oauth2.googleapis.com/token",
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId!,
            ["client_secret"] = clientSecret!,
            ["redirect_uri"] = redirectUri!,
            ["grant_type"] = "authorization_code"
        }));

    if (!tokenResponse.IsSuccessStatusCode)
    {
        var errorContent = await tokenResponse.Content.ReadAsStringAsync();
        return Results.BadRequest(new { error = "Token exchange failed.", details = errorContent });
    }

    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
    using var tokenDoc = JsonDocument.Parse(tokenContent);

    var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();
    var refreshToken = tokenDoc.RootElement.TryGetProperty("refresh_token", out var refreshTokenElement)
        ? refreshTokenElement.GetString()
        : null;

    if (string.IsNullOrWhiteSpace(accessToken))
        return Results.BadRequest(new { error = "Access token not returned." });

    if (string.IsNullOrWhiteSpace(refreshToken))
        return Results.BadRequest(new
        {
            error = "Refresh token not returned.",
            hint = "Google may only return refresh_token on first consent. Revoke app access and try again if needed."
        });

    var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
    userInfoRequest.Headers.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

    var userInfoResponse = await http.SendAsync(userInfoRequest);

    if (!userInfoResponse.IsSuccessStatusCode)
    {
        var errorContent = await userInfoResponse.Content.ReadAsStringAsync();
        return Results.BadRequest(new { error = "Failed to get Google user info.", details = errorContent });
    }

    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
    using var userDoc = JsonDocument.Parse(userInfoContent);

    var email = userDoc.RootElement.GetProperty("email").GetString();

    if (string.IsNullOrWhiteSpace(email))
        return Results.BadRequest(new { error = "Google account email not returned." });

    var existingConnection = await db.CalendarConnections
        .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Provider == "Google" && x.CalendarEmail == email);

    if (existingConnection is null)
    {
        var connection = new CalendarConnection
        {
            TenantId = tenantId,
            Provider = "Google",
            CalendarEmail = email,
            CalendarId = "primary",
            RefreshToken = refreshToken,
            IsActive = true,
            ConnectedAt = DateTime.UtcNow
        };

        db.CalendarConnections.Add(connection);
    }
    else
    {
        existingConnection.RefreshToken = refreshToken;
        existingConnection.IsActive = true;
        existingConnection.ConnectedAt = DateTime.UtcNow;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "Google Calendar connected successfully.",
        tenantId,
        calendarEmail = email
    });
});

app.MapGet("/api/calendar-connections", async (HttpContext httpContext, AlicIADbContext db) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var items = await db.CalendarConnections
        .Where(x => x.TenantId == tenantId.Value)
        .OrderByDescending(x => x.ConnectedAt)
        .Select(x => new
        {
            x.Id,
            x.TenantId,
            x.Provider,
            x.CalendarEmail,
            x.CalendarId,
            x.IsActive,
            x.ConnectedAt
        })
        .ToListAsync();

    return Results.Ok(items);
}).RequireAuthorization();

app.MapGet("/api/google/busy-slots", async (
    HttpContext httpContext,
    GoogleCalendarService googleCalendarService,
    AlicIADbContext db,
    CancellationToken cancellationToken) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var connection = await db.CalendarConnections
        .FirstOrDefaultAsync(x => x.TenantId == tenantId.Value && x.Provider == "Google" && x.IsActive, cancellationToken);

    if (connection is null)
        return Results.BadRequest(new { error = "Active Google Calendar connection not found for this tenant." });

    var timeMin = DateTime.UtcNow;
    var timeMax = DateTime.UtcNow.AddDays(7);

    var busySlots = await googleCalendarService.GetBusySlotsAsync(
        connection,
        timeMin,
        timeMax,
        cancellationToken);

    return Results.Ok(new
    {
        tenantId = tenantId.Value,
        calendarEmail = connection.CalendarEmail,
        timeMin,
        timeMax,
        busySlots
    });
}).RequireAuthorization();

app.MapGet("/api/availability/google-next-slots", async (
    Guid serviceId,
    HttpContext httpContext,
    GoogleCalendarService googleCalendarService,
    AlicIADbContext db,
    CancellationToken cancellationToken) =>
{
    var tenantId = GetTenantIdFromClaims(httpContext);
    if (tenantId is null)
        return Results.Unauthorized();

    var service = await db.Services
        .FirstOrDefaultAsync(x => x.Id == serviceId && x.TenantId == tenantId.Value, cancellationToken);

    if (service is null)
        return Results.BadRequest(new { error = "Service not found for this tenant." });

    var connection = await db.CalendarConnections
        .FirstOrDefaultAsync(x => x.TenantId == tenantId.Value && x.Provider == "Google" && x.IsActive, cancellationToken);

    if (connection is null)
        return Results.BadRequest(new { error = "Active Google Calendar connection not found for this tenant." });

    var businessHours = await db.BusinessHours
        .Where(x => x.TenantId == tenantId.Value && x.IsActive)
        .ToListAsync(cancellationToken);

    if (!businessHours.Any())
        return Results.BadRequest(new { error = "Business hours not configured for this tenant." });

    var timeMin = DateTime.UtcNow;
    var timeMax = DateTime.UtcNow.AddDays(7);

    var googleBusySlots = await googleCalendarService.GetBusySlotsAsync(
        connection,
        timeMin,
        timeMax,
        cancellationToken);

    var requestBusySlots = await db.Requests
        .Where(x =>
            x.TenantId == tenantId.Value &&
            x.ScheduledAt != null &&
            x.Status != RequestStatus.Cancelled)
        .Join(
            db.Services,
            request => request.ServiceId,
            svc => svc.Id,
            (request, svc) => new AvailableSlot(
                request.ScheduledAt!.Value,
                request.ScheduledAt!.Value.AddMinutes(svc.DurationMinutes)))
        .ToListAsync(cancellationToken);

    var allBusySlots = googleBusySlots
        .Select(x => new AvailableSlot(x.StartUtc, x.EndUtc))
        .Concat(requestBusySlots)
        .OrderBy(x => x.StartUtc)
        .ToList();

    var slots = new List<AvailableSlot>();
    var now = DateTime.UtcNow;
    var stepMinutes = service.DurationMinutes > 0 ? service.DurationMinutes : 30;
    var duration = service.DurationMinutes;

    for (var day = 0; day < 7; day++)
    {
        var currentDate = now.Date.AddDays(day);
        var dayOfWeek = currentDate.DayOfWeek;

        var dayRules = businessHours
            .Where(x => x.DayOfWeek == dayOfWeek)
            .ToList();

        foreach (var rule in dayRules)
        {
            var windowStart = currentDate.Add(rule.StartTime);
            var windowEnd = currentDate.Add(rule.EndTime);

            if (windowEnd <= now)
                continue;

            var cursor = windowStart < now ? RoundUpToNextStep(now, stepMinutes) : windowStart;

            while (cursor.AddMinutes(duration) <= windowEnd)
            {
                var candidateStart = cursor;
                var candidateEnd = cursor.AddMinutes(duration);

                var overlaps = allBusySlots.Any(b =>
                    candidateStart < b.EndUtc && candidateEnd > b.StartUtc);

                if (!overlaps)
                {
                    slots.Add(new AvailableSlot(candidateStart, candidateEnd));
                }

                cursor = cursor.AddMinutes(stepMinutes);
            }
        }
    }

    var nextSlots = slots
        .OrderBy(x => x.StartUtc)
        .Take(20)
        .ToList();

    return Results.Ok(new
    {
        tenantId = tenantId.Value,
        serviceId,
        serviceName = service.Name,
        durationMinutes = service.DurationMinutes,
        totalFound = nextSlots.Count,
        slots = nextSlots
    });
}).RequireAuthorization();

app.MapGet("/public/{tenantSlug}/services", async (string tenantSlug, AlicIADbContext db) =>
{
    var slug = NormalizeSlug(tenantSlug);
    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == slug && x.Status == "Active");
    if (tenant is null)
        return Results.NotFound(new { error = "Tenant not found." });

    var services = await db.Services
        .Where(x => x.TenantId == tenant.Id)
        .OrderBy(x => x.Name)
        .Select(x => new
        {
            x.Id,
            x.Name,
            x.DurationMinutes,
            x.Price
        })
        .ToListAsync();

    return Results.Ok(services);
});

app.MapGet("/public/{tenantSlug}/availability", async (
    string tenantSlug,
    Guid serviceId,
    GoogleCalendarService googleCalendarService,
    AlicIADbContext db,
    CancellationToken cancellationToken,
    DateTime? startDate = null,
    int days = 7,
    int maxSlots = 10) =>
{
    if (days <= 0)
        return Results.BadRequest(new { error = "Days must be greater than zero." });

    if (maxSlots <= 0)
        return Results.BadRequest(new { error = "MaxSlots must be greater than zero." });

    var slug = NormalizeSlug(tenantSlug);
    var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == slug && x.Status == "Active", cancellationToken);
    if (tenant is null)
        return Results.NotFound(new { error = "Tenant not found." });

    var service = await db.Services.FirstOrDefaultAsync(x => x.Id == serviceId && x.TenantId == tenant.Id, cancellationToken);
    if (service is null)
        return Results.NotFound(new { error = "Service not found for this tenant." });

    var businessHours = await db.BusinessHours
        .Where(x => x.TenantId == tenant.Id && x.IsActive)
        .ToListAsync(cancellationToken);

    if (!businessHours.Any())
        return Results.BadRequest(new { error = "No active business hours found for tenant." });

    var scheduledRequests = await db.Requests
        .Include(x => x.Service)
        .Where(x => x.TenantId == tenant.Id && x.ScheduledAt != null && x.Status != RequestStatus.Cancelled)
        .ToListAsync(cancellationToken);

    var windowStart = (startDate ?? DateTime.UtcNow).ToUniversalTime();
    var searchStartDate = windowStart.Date;
    var windowEnd = searchStartDate.AddDays(days);
    var duration = TimeSpan.FromMinutes(service.DurationMinutes);
    var googleBusySlots = new List<BusySlot>();

    var connection = await db.CalendarConnections
        .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Provider == "Google" && x.IsActive, cancellationToken);

    if (connection is not null)
    {
        googleBusySlots = await googleCalendarService.GetBusySlotsAsync(
            connection,
            windowStart,
            windowEnd,
            cancellationToken);
    }

    var results = new List<SlotResponse>();

    for (var dayIndex = 0; dayIndex < days && results.Count < maxSlots; dayIndex++)
    {
        var currentDate = searchStartDate.AddDays(dayIndex);
        var dayHours = businessHours.Where(x => x.DayOfWeek == currentDate.DayOfWeek).ToList();
        if (!dayHours.Any())
            continue;

        foreach (var period in dayHours.OrderBy(x => x.StartTime))
        {
            var slotStart = currentDate.Add(period.StartTime);
            if (dayIndex == 0 && slotStart < windowStart)
                slotStart = windowStart;

            var endBoundary = currentDate.Add(period.EndTime);
            while (slotStart + duration <= endBoundary && results.Count < maxSlots)
            {
                var slotEnd = slotStart + duration;
                var hasRequestConflict = IsSlotOccupied(slotStart, slotEnd, scheduledRequests);
                var hasGoogleConflict = googleBusySlots.Any(x => BookingHelpers.Overlaps(slotStart, slotEnd, x.StartUtc, x.EndUtc));

                if (!hasRequestConflict && !hasGoogleConflict)
                    results.Add(new SlotResponse { Start = slotStart, End = slotEnd });

                slotStart = slotStart.AddMinutes(15);
            }

            if (results.Count >= maxSlots)
                break;
        }
    }

    return Results.Ok(results);
});

app.MapPost("/public/{tenantSlug}/bookings", async (
                string tenantSlug,
                CreateBookingRequest request,
                GoogleCalendarService googleCalendarService,
                AlicIADbContext db,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                    return Results.BadRequest(new { error = "CustomerName is required." });

                if (string.IsNullOrWhiteSpace(request.CustomerPhone))
                    return Results.BadRequest(new { error = "CustomerPhone is required." });

                var slug = NormalizeSlug(tenantSlug);
                var tenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == slug && x.Status == "Active", cancellationToken);
                if (tenant is null)
                    return Results.BadRequest(new { error = "Tenant not found." });

                var service = await db.Services.FirstOrDefaultAsync(x => x.Id == request.ServiceId && x.TenantId == tenant.Id, cancellationToken);
                if (service is null)
                    return Results.BadRequest(new { error = "Service not found for this tenant." });

                var scheduledAtUtc = request.ScheduledAt.ToUniversalTime();
                var scheduledEndUtc = scheduledAtUtc.AddMinutes(service.DurationMinutes);

                var businessHours = await db.BusinessHours
                    .Where(x => x.TenantId == tenant.Id && x.IsActive && x.DayOfWeek == scheduledAtUtc.DayOfWeek)
                    .ToListAsync(cancellationToken);

                if (!businessHours.Any())
                    return Results.BadRequest(new { error = "No active business hours found for the requested day." });

                var validPeriod = businessHours.Any(x => scheduledAtUtc.TimeOfDay >= x.StartTime && scheduledEndUtc.TimeOfDay <= x.EndTime);
                if (!validPeriod)
                    return Results.BadRequest(new { error = "Requested slot is outside of business hours." });

                var existingRequests = await db.Requests
                    .Include(x => x.Service)
                    .Where(x => x.TenantId == tenant.Id && x.ScheduledAt != null && x.Status != RequestStatus.Cancelled)
                    .ToListAsync(cancellationToken);

                if (existingRequests.Any(x =>
                        BookingHelpers.Overlaps(scheduledAtUtc, scheduledEndUtc, x.ScheduledAt!.Value.ToUniversalTime(), x.ScheduledAt.Value.ToUniversalTime().AddMinutes(x.Service?.DurationMinutes ?? 0))))
                {
                    return Results.BadRequest(new { error = "Requested slot is already occupied by another booking." });
                }

                var connection = await db.CalendarConnections
                    .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Provider == "Google" && x.IsActive, cancellationToken);

                if (connection is null)
                    return Results.BadRequest(new { error = "Active Google Calendar connection not found for this tenant." });

                var googleBusySlots = await googleCalendarService.GetBusySlotsAsync(connection, scheduledAtUtc, scheduledEndUtc, cancellationToken);
                if (googleBusySlots.Any(x => BookingHelpers.Overlaps(scheduledAtUtc, scheduledEndUtc, x.StartUtc, x.EndUtc)))
                {
                    return Results.BadRequest(new { error = "Requested slot is busy in Google Calendar." });
                }

                var customerPhone = request.CustomerPhone.Trim();
                var customer = await db.Customers
                    .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Phone == customerPhone, cancellationToken);

                if (customer is null)
                {
                    customer = new Customer
                    {
                        TenantId = tenant.Id,
                        Name = request.CustomerName.Trim(),
                        Phone = customerPhone,
                        Email = request.CustomerEmail?.Trim()
                    };

                    db.Customers.Add(customer);
                    await db.SaveChangesAsync(cancellationToken);
                }

                var bookingRequest = new Request
                {
                    TenantId = tenant.Id,
                    CustomerId = customer.Id,
                    ServiceId = service.Id,
                    Type = RequestType.Booking,
                    Status = RequestStatus.Confirmed,
                    ScheduledAt = scheduledAtUtc,
                    TotalAmount = request.TotalAmount ?? service.Price
                };

                db.Requests.Add(bookingRequest);
                await db.SaveChangesAsync(cancellationToken);

                try
                {
                    var eventId = await googleCalendarService.CreateEventAsync(
                        connection,
                        service.Name,
                        scheduledAtUtc,
                        scheduledEndUtc,
                        $"Customer: {customer.Name} | Phone: {customer.Phone}",
                        cancellationToken);

                    bookingRequest.ExternalEventId = eventId;
                    await db.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    db.Requests.Remove(bookingRequest);
                    await db.SaveChangesAsync(cancellationToken);
                    return Results.BadRequest(new { error = "Failed to create Google Calendar event.", details = ex.Message });
                }

                return Results.Created($"/bookings/{bookingRequest.Id}", new
                {
                    status = "confirmed",
                    service = service.Name,
                    customer = customer.Name,
                    scheduledAt = bookingRequest.ScheduledAt,
                    requestId = bookingRequest.Id,
                    externalEventId = bookingRequest.ExternalEventId
                });
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

static DateTime RoundUpToNextStep(DateTime value, int stepMinutes)
{
    var remainder = value.Minute % stepMinutes;
    var rounded = new DateTime(
        value.Year,
        value.Month,
        value.Day,
        value.Hour,
        value.Minute,
        0,
        DateTimeKind.Utc);

    if (remainder == 0 && value.Second == 0 && value.Millisecond == 0)
        return rounded;

    return rounded.AddMinutes(stepMinutes - remainder);
}

static Guid? GetTenantIdFromClaims(HttpContext httpContext)
{
    var tenantIdClaim = httpContext.User.FindFirst("tenantId")?.Value;
    return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
}

static string NormalizeSlug(string value)
{
    var normalized = value.Trim().ToLowerInvariant();
    normalized = Regex.Replace(normalized, @"[^a-z0-9]+", "-").Trim('-');
    normalized = Regex.Replace(normalized, @"-+", "-");

    return normalized.Length <= 120 ? normalized : normalized[..120].Trim('-');
}

public record CreateTenantRequest(
    string Name,
    string Segment,
    string Plan,
    string Status,
    string? Slug = null
);

public record CreateServiceRequest(
    string Name,
    int DurationMinutes,
    decimal Price,
    Guid? TenantId = null
);

public record CreateCustomerRequest(
    string Name,
    string Phone,
    string? Email,
    Guid? TenantId = null
);

public record CreateRequestRequest(
    Guid CustomerId,
    Guid ServiceId,
    RequestType Type,
    RequestStatus Status,
    DateTime? ScheduledAt,
    decimal? TotalAmount,
    Guid? TenantId = null
);

public record AvailableSlot(DateTime StartUtc, DateTime EndUtc);
