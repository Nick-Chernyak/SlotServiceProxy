namespace SlotServiceProxy.Application.Slots.SDK;

/// <summary>
/// Simple day with with corresponding date and free slots.
/// </summary>
public record DayDto
{
    /// <summary>
    /// Date of the day.
    /// </summary>
   public required DateTime Date { get; init; }
    
    /// <summary>
    /// List of free slots for the day, which can be reserved.
    /// </summary>
    public required SlotDto[] FreeSlots { get; init; }
}