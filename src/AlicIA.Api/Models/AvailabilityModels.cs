namespace AlicIA.Api.Models;

public sealed class NextAvailableSlotsRequest
{
    public Guid TenantId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime? StartDate { get; set; }
    public int Days { get; set; } = 7;
    public int MaxSlots { get; set; } = 10;
}

public sealed class SlotResponse
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
