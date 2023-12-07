using SlotServiceProxy.Domain.Shared;

namespace SlotServiceProxy.Domain.Slots;

/// <summary>
/// Represent a single day in a doctor's timetable with related available slots.
/// </summary>
public record DayInTimetable : IComparable<DayInTimetable>
{
    private const int HighestHourInDay = 23;
    private const int LowestHourInDay = 0;
    
    private readonly HashSet<DailyTimeRange> _slots;
    
    /// <summary>
    /// Date in the yyyy-MM-dd format (without time).
    /// </summary>
    public DateTime Date { get; }
    
    public TimeSpan Start { get; }
    
    public TimeSpan End { get; }
    
    public IReadOnlyCollection<DailyTimeRange> Slots => _slots;
    
    public DayInTimetable(DateTime date, TimeSpan start, TimeSpan end)
    {
        if (start > end || start == end)
            throw new ArgumentException("Start time must be less than end time");

        if (end - start > TimeSpan.FromDays(1))
            throw new ArgumentException("Day duration must be less than 24 hours");

        if (!IsHoursInDayBorders(start) || !IsHoursInDayBorders(end))
            throw new ArgumentException("Start / end time must be in day borders (0h-23h)");
        
        Date = date.Date;
        Start = start;
        End = end;
        _slots = new HashSet<DailyTimeRange>();
    }

    public void AddSlots(IEnumerable<DailyTimeRange> slots)
    {
        foreach (var slot in slots)
            AddSlot(slot);
    }

    public void CutOffSlotsBefore(DateTime dateTime)
    {
        if (dateTime.Date != Date)
            throw new ArgumentException("Cut off date must be the same as the day date");

        _slots.RemoveWhere(s => s.End < dateTime);
    }

    public int CompareTo(DayInTimetable? obj) 
        => Date.CompareTo(obj?.Date);
    
    private void AddSlot(DailyTimeRange dailyTimeRange)
    {
        if (dailyTimeRange.Start.Date != Date)
            throw new ArgumentException("Slot date must be the same as the day date");
        if (dailyTimeRange.Start.TimeOfDay < Start)
            throw new ArgumentException("Slot start time must be greater than the day start time");
        if (dailyTimeRange.End.TimeOfDay > End)
            throw new ArgumentException("Slot end time must be less than the day end time");
        
        _slots.Add(dailyTimeRange);
    }

    private static bool IsHoursInDayBorders(TimeSpan timeSpan)
        => timeSpan.TotalHours is <= HighestHourInDay and >= LowestHourInDay;
}