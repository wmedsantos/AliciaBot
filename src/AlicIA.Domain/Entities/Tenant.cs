namespace AlicIA.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Segment { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<CalendarConnection> CalendarConnections { get; set; } = new List<CalendarConnection>();
    public ICollection<BusinessHours> BusinessHours { get; set; } = new List<BusinessHours>();
}
