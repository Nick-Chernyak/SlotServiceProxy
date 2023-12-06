using System.Collections.Immutable;
using System.Dynamic;

namespace SlotServiceProxy.Domain.Slots;

public class DoctorCalendar
{
    public ImmutableSortedSet<DayInTimetable> Days { get; private set; }
    
    public static DoctorCalendar Create(ImmutableSortedSet<DayInTimetable> days) => new(days);
    
    private DoctorCalendar(ImmutableSortedSet<DayInTimetable> days) => Days = days;
    
    public void AddDay(DayInTimetable day)
    {
        if (Days.Contains(day))
            throw new ArgumentException("Day already exists in doctor's calendar!");
        
        //There is no problem in performance, because immutable collections are optimized for sharing data between instances
        //Not much memory is used, because only references are copied
        Days = Days.Add(day);
    }
}