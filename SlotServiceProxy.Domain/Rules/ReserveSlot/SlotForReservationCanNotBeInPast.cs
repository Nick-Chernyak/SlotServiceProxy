using SlotServiceProxy.Domain.Shared;

namespace SlotServiceProxy.Domain.Rules.ReserveSlot;

public class SlotForReservationCanNotBeInPast : IBusinessRule
{
    private readonly DailyTimeRange _dailyTimeRange;
    private readonly IDateTimeService _dateTimeService;

    //Not the best approach to pass service as dependency,
    //but rules shouldn't live too long (in current web request usually), so it's not a big deal.
    public SlotForReservationCanNotBeInPast(DailyTimeRange dailyTimeRange, IDateTimeService dateTimeService)
    {
        _dailyTimeRange = dailyTimeRange;
        _dateTimeService = dateTimeService;
    }

    public bool IsBroken()
        => _dailyTimeRange.Start.Date < _dateTimeService.Now();
    
    public string Message => "It is not possible to book slot in the past.";
}