using MediatR;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Domain.Rules.FetchFreeSlots;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Application.Slots.FetchFreeSlots;

public class FetchFreeSlotsQueryHandler : BaseRulesCheckerHandler, IRequestHandler<FetchFreeSlotsQuery, Result<CalendarDto, Problem>>
{
    private readonly ISlotsDataSource _slotsDataSource;

    public FetchFreeSlotsQueryHandler(ISlotsDataSource slotsDataSource)
        => _slotsDataSource = slotsDataSource;

    public async Task<Result<CalendarDto, Problem>> Handle(FetchFreeSlotsQuery request, CancellationToken cancellationToken)
    {
        CheckRule(new SearchDateMustBeTodayOrInFuture(request.SearchDate));

        var calendarAsResult = await _slotsDataSource.GetDoctorWithWeekCalendarAsync(request.SearchDate);
        return calendarAsResult.BiMap(
                doctorCalendar =>
                {
                    doctorCalendar.Calendar.Do(CutOffNotAvailableTodaySlotsIfNeeded);
                    return ToDoctorWithCalendarDto(doctorCalendar, request.SearchDate);
                },
                error => new Problem(error.Message));
    }

    private static void CutOffNotAvailableTodaySlotsIfNeeded(DoctorCalendar doctorCalendar)
    {
        var now = DateTime.Now;
        var today = doctorCalendar.Days.SingleOrDefault(d => d.Date == now);
        today?.CutOffSlotsBefore(now);
    }

    //I prefer to avoid using 3-rd party mappers like AutoMapper, Mapster, etc, because
    //1. From my POV it is add dynamic layer in strong-typed language context (map values base on property names, not during compile-time).
    //2. Manual mapping directly in code is more readable and understandable (from my POV).
    //3. It is very easy to map simple objects like in scope of this application.
    //4. For each concrete flow (Mediator handler) you can have your own mapping logic, if needed, which placed in needed for
    //concrete flow place. Readability and maintainability is more important than DRY principle.
    
    #region Mapping

    private static CalendarDto ToDoctorWithCalendarDto(Doctor doctor, DateTime searchDate)
        => new()
        {
            Id = doctor.Id,
            CurrentWeek = new CurrentWeekDto
            {
                From = searchDate,
                Days = doctor.Calendar.Days.Select(ToDayDto).ToArray()
            }
        };

    private static DayDto ToDayDto(DayInTimetable day)
        => new()
        {
            Date = day.Date,
            FreeSlots = day.Slots.Select(ToSlotDto).ToArray()
        };

    private static SlotDto ToSlotDto(DailyTimeRange dailyTimeRange)
        => new()
        {
            Start = dailyTimeRange.Start, 
            End = dailyTimeRange.End,
        };

    #endregion
}