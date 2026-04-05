namespace AlicIA.Domain.Entities;

public class CalendarConnection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public string Provider { get; set; } = "Google";
    public string CalendarEmail { get; set; } = string.Empty;
    public string CalendarId { get; set; } = "primary";

    public string RefreshToken { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}