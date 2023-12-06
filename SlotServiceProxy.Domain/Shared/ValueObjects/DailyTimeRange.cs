namespace SlotServiceProxy.Domain.Shared.ValueObjects;

/// <summary>
/// Simple struct to represent a time range inside one day.
/// Only basic validation is provided.
/// Designed as a struct since it has value nature.
/// Can have duration of 0.
/// </summary>
public readonly struct DailyTimeRange : IComparable<DailyTimeRange>
{
    public DateTime Start { get; init; }
    
    public DateTime End { get; init; }
    
    public DailyTimeRange(DateTime start, DateTime end)
    {
        if (start.Date != end.Date)
            throw new ArgumentException("Start date and end date must be on the same day");
        if (start > end)
            throw new ArgumentException("Start date must be less than end date");
        
        Start = start;
        End = end;
    }
    
    public TimeSpan Duration => End - Start;
    
    public int CompareTo(DailyTimeRange other)
    {
        var startComparison = Start.CompareTo(other.Start);
        return startComparison != 0 ? startComparison : End.CompareTo(other.End);
    }
}