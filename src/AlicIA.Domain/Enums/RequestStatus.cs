namespace AlicIA.Domain.Enums;

public enum RequestStatus
{
    Pending = 1,
    PendingConfirmation = 2,
    Confirmed = 3,
    Rescheduled = 4,
    Cancelled = 5,
    Completed = 6,
    NoShow = 7
}
