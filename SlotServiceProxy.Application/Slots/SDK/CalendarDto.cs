namespace SlotServiceProxy.Application.Slots.SDK;

/// <summary>
/// Calendar with its owner identifier.
/// </summary>
public record CalendarDto : IResponseDto
{
    /// <summary>
    /// Id of the facility who owns the calendar. Needed for reserving a slot.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Current week work doctor calendar with free slots.
    /// </summary>
    public required CurrentWeekDto CurrentWeek { get; init; }
    
}