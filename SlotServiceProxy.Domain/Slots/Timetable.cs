using System.Collections.Immutable;

namespace SlotServiceProxy.Domain.Slots;

/// <summary>
/// Represent a timetable with days that have can have slots for reservation.
/// </summary>
public record Timetable
{
    //Mostly all needed domain logic covered by using ImmutableSortedSet as a collection for Days:
    //1. Days cannot be changed (at least by ImmutableSortedSet contract);
    //2. No duplication (Set property);
    //3. Days place in ascending order ("Sorted").
    public ImmutableSortedSet<DayInTimetable> Days { get; private set; }
    
    public static Timetable Create(ImmutableSortedSet<DayInTimetable> days)
        => new(days);
    
    private Timetable(ImmutableSortedSet<DayInTimetable> days) => Days = days;
}