using System.ComponentModel.DataAnnotations;

namespace AlicIA.Api.Models;

public sealed class BusinessHoursCreateRequest
{
    public Guid TenantId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    [DataType(DataType.Time)]
    [RegularExpression("^([0-1]\\d|2[0-3]):([0-5]\\d):([0-5]\\d)$", ErrorMessage = "Use HH:mm:ss format.")]
    public string StartTime { get; set; } = string.Empty;

    [DataType(DataType.Time)]
    [RegularExpression("^([0-1]\\d|2[0-3]):([0-5]\\d):([0-5]\\d)$", ErrorMessage = "Use HH:mm:ss format.")]
    public string EndTime { get; set; } = string.Empty;
}

public sealed class BusinessHoursResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    [DataType(DataType.Time)]
    public string StartTime { get; set; } = string.Empty;

    [DataType(DataType.Time)]
    public string EndTime { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
