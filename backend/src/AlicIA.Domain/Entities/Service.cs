namespace AlicIA.Domain.Entities;

public class Service
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
