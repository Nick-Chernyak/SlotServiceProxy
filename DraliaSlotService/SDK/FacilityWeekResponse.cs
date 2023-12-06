using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace DraliaSlotService.SDK;

public record FacilityWeekResponse(
    Facility Facility,
    int SlotDurationMinutes,
    FacilityDay? Monday,
    FacilityDay? Tuesday,
    FacilityDay? Wednesday,
    FacilityDay? Thursday,
    FacilityDay? Friday,
    FacilityDay? Saturday,
    FacilityDay? Sunday) : IDraliaResponse
{
    public DayWithDayOfWeek[] Days => new[] { new DayWithDayOfWeek(Monday, DayOfWeek.Monday),
            new DayWithDayOfWeek(Tuesday, DayOfWeek.Tuesday), 
            new DayWithDayOfWeek(Wednesday, DayOfWeek.Wednesday),
            new DayWithDayOfWeek(Thursday, DayOfWeek.Thursday),
            new DayWithDayOfWeek(Friday, DayOfWeek.Friday),
            new DayWithDayOfWeek(Saturday, DayOfWeek.Saturday),
            new DayWithDayOfWeek(Sunday, DayOfWeek.Sunday)}
        .Where(day => day.Day is not null).ToArray();
}

public record Facility(string FacilityId, string Name, string Address);

public record FacilityDay(WorkPeriod WorkPeriod, IReadOnlyCollection<DailyTimeRange>? BusySlots);

public record WorkPeriod(int StartHour, int EndHour, int LunchStartHour, int LunchEndHour)
{
    public DateTime StarHourAsDate(DateTime actualDate)
        => WorkPeriodHourAsDate(actualDate, StartHour);

    public DateTime EndHourAsDate(DateTime actualDate)
        => WorkPeriodHourAsDate(actualDate, EndHour);

    public DateTime LunchStartHourAsDate(DateTime actualDate)
        => WorkPeriodHourAsDate(actualDate, LunchStartHour);

    public DateTime LunchEndHourAsDate(DateTime actualDate)
        => WorkPeriodHourAsDate(actualDate, LunchEndHour);

    private DateTime WorkPeriodHourAsDate(DateTime actualDate, int hour)
        => new(actualDate.Year, actualDate.Month, actualDate.Day, hour, 0, 0);
}

/// <summary>
/// Support data structure for <see cref="FacilityWeekResponse"/>.
/// Helps to iterate over days in a week instead of using reflection / manual switch per property.
/// </summary>
/// <param name="Day">Day model from Dralia API</param>
/// <param name="DayOfWeek">Standard .NET enum representation.</param>
public record DayWithDayOfWeek(FacilityDay? Day, DayOfWeek DayOfWeek);

