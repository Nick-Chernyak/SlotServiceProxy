using SlotServiceProxy.Domain.Shared;

namespace SlotServiceProxy.Domain.Rules.ReserveSlot;

public class SlotForReservationCanNotBeInPast : IBusinessRule
{
    private readonly DailyTimeRange _dailyTimeRange;
    
    public SlotForReservationCanNotBeInPast(DailyTimeRange dailyTimeRange)
        => _dailyTimeRange = dailyTimeRange;
    
    public bool IsBroken() 
        => _dailyTimeRange.Start.Date < DateTime.Now.Date;
    
    public string Message => "It is not possible to book slot in the past.";
}