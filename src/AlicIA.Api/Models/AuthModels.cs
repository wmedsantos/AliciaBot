namespace AlicIA.Api.Models;

public record LoginRequest(
    Guid TenantId,
    string Email,
    string Password
);

public record LoginResponse(
    string Token,
    string Email,
    Guid TenantId,
    string Role,
    DateTime ExpiresAt
);

public record CreateUserRequest(
    Guid TenantId,
    string Email,
    string Password,
    string Role = "Owner"
);
