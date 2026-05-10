using AlicIA.Domain.Enums;

namespace AlicIA.Domain.Entities;

public class Request
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }

    public RequestType Type { get; set; } = RequestType.Booking;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public DateTime? ScheduledAt { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? ExternalEventId { get; set; }
    public string? ConfirmationTokenHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
    public Customer? Customer { get; set; }
    public Service? Service { get; set; }
}
