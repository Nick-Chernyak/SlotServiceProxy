namespace SlotServiceProxy.Application.Slots.SDK;

/// <summary>
/// Represent a slot with start and end time in same day.
/// </summary>
public record SlotDto
{
    /// <summary>
    /// Start time of the slot.
    /// </summary>
    public required DateTime Start { get; init; }
    
    /// <summary>
    /// End time of the slot.
    /// </summary>
    public required DateTime End { get; init; }
};