using System.Collections.Immutable;
using DraliaSlotService.SDK;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace DraliaSlotService;

/// <summary>
/// Builds <see cref="Timetable"/> from <see cref="FacilityWeekResponse"/>.
/// One instance per one <see cref="FacilityWeekResponse"/> and <see cref="DateTime"/> pair.
/// Designed like that to have a possibility store state (more clean building algorithm method chain inside).
/// </summary>
public class DraliaCalendarBuilder
{
    private readonly DateTime _searchDate;
    private readonly FacilityWeekResponse _facilityWeek;
    private readonly TimeSpan _slotDurationInMinutes;
    
    /// <summary>
    /// Dummy constant for Sunday edge case (see usage below) and primitive calculations.
    /// </summary>
    private const int NumberOfDaysInWeek = 7;

    private static Problem DayIsNotPresentInTimetable(DateTime dateTime)
        => new($"Chosen day ({dateTime.DayOfWeek} {dateTime.Date})  is not present in timetable.", ProblemType.ExpectationConflict);

    public DraliaCalendarBuilder(FacilityWeekResponse facilityWeek, DateTime searchDate)
    {
        _searchDate = searchDate;
        _facilityWeek = facilityWeek;
        _slotDurationInMinutes = TimeSpan.FromMinutes(facilityWeek.SlotDurationMinutes);
    }
    
    public Timetable BuildWholeCalendar() 
        => _facilityWeek.Days.Where(SameAsSearchDayOrLaterInWeek)
            .Select(BuildDayInTimeTable)
            .ToImmutableSortedSet()
            .To(Timetable.Create);

    public Result<DayInTimetable, Problem> BuildSingleDay()
        => _facilityWeek.Days.SingleOrDefault(SameAsSearchDayInWeek) switch
        {
            { } dayWithDayOfWeek => BuildDayInTimeTable(dayWithDayOfWeek),
            _ => Result<DayInTimetable, Problem>.Failure(_searchDate.To(DayIsNotPresentInTimetable))
        };

    private DayInTimetable BuildDayInTimeTable(DayWithDayOfWeek dayWithDayOfWeek)
    {
        var actualDate = _searchDate.GetDateTimeBaseOnDayOfWeek(dayWithDayOfWeek.DayOfWeek);

        var currentStart = dayWithDayOfWeek.Day!.WorkPeriod.StarHourAsDate(actualDate);
        var endDate = dayWithDayOfWeek.Day!.WorkPeriod.EndHourAsDate(actualDate);
        
        var day = new DayInTimetable(actualDate, currentStart.TimeOfDay, endDate.TimeOfDay);

        foreach (var busySlot in OrderedBusySlotsWithLunch(dayWithDayOfWeek.Day!, actualDate))
        {
            if (busySlot.Start > currentStart)
                day.AddSlots(GetSlotsBetween(currentStart, busySlot.Start));
            
            currentStart = busySlot.End;
        }

        day.AddSlots(GetSlotsBetween(currentStart, endDate));

        return day;
    }

    private IEnumerable<DailyTimeRange> GetSlotsBetween(DateTime start, DateTime end)
    {
        while (start + _slotDurationInMinutes <= end)
        {
            yield return new DailyTimeRange { Start = start, End = start + _slotDurationInMinutes };
            start += _slotDurationInMinutes;
        }
    }

    private static ImmutableSortedSet<DailyTimeRange> OrderedBusySlotsWithLunch(FacilityDay facilityDay, DateTime actualDate)
    {
        var lunchPeriod = new DailyTimeRange(
            facilityDay.WorkPeriod.LunchStartHourAsDate(actualDate),
            facilityDay.WorkPeriod.LunchEndHourAsDate(actualDate))
            .AsArray();

        var areThereAnyBusySlots = facilityDay.BusySlots is not null && facilityDay.BusySlots.Count > 0;
        
        return areThereAnyBusySlots
            ? facilityDay.BusySlots!.Concat(lunchPeriod).OrderBy(slot => slot.Start).ToImmutableSortedSet()
            : lunchPeriod.ToImmutableSortedSet();
    }

    private bool SameAsSearchDayOrLaterInWeek(DayWithDayOfWeek dayWithDayOfWeek)
    {
        //Found edge cases thanks tests.
        //DayOfWeek enum started with Sunday, not Monday -> we need to check it separately.
        if (dayWithDayOfWeek.DayOfWeek is DayOfWeek.Sunday)
            return NumberOfDaysInWeek >= (int)_searchDate.DayOfWeek;
        
        return dayWithDayOfWeek.DayOfWeek >= _searchDate.DayOfWeek;
    }

    private bool SameAsSearchDayInWeek(DayWithDayOfWeek dayWithDayOfWeek)
        => dayWithDayOfWeek.DayOfWeek == _searchDate.DayOfWeek;
}