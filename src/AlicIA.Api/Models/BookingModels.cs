namespace AlicIA.Api.Models;

public record CreateBookingRequest(
    Guid TenantId,
    Guid ServiceId,
    string CustomerName,
    string CustomerPhone,
    DateTime ScheduledAt,
    string? CustomerEmail = null,
    decimal? TotalAmount = null
);

public static class BookingHelpers
{
    public static bool Overlaps(DateTime start, DateTime end, DateTime existingStart, DateTime existingEnd)
    {
        return start < existingEnd && existingStart < end;
    }
}
