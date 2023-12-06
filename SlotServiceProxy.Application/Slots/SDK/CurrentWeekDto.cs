namespace SlotServiceProxy.Application.Slots.SDK;

/// <summary>
/// Represent a current week with related to a given From date.
/// Means, if From date is Wednesday, then Calendar will contain available days with slots
/// for the current week starting from Wednesday until Sunday - not a full week.
/// </summary>
public record CurrentWeekDto
{
    /// <summary>
    /// Date based on which week is chosen.
    /// </summary>
    public required DateTime From { get; init; }

    /// <summary>
    /// Array of days with free slots.
    /// </summary>
    public required DayDto[] Days { get; init; }
}