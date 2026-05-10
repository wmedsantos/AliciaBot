namespace AlicIA.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
