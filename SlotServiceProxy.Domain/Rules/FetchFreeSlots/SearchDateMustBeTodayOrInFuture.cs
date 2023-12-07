using SlotServiceProxy.Domain.Shared;

namespace SlotServiceProxy.Domain.Rules.FetchFreeSlots;

public record SearchDateMustBeTodayOrInFuture : IBusinessRule
{
    private readonly DateTime _dateTime;
    private readonly IDateTimeService _dateTimeService;

    //Not the best approach to pass service as dependency,
    //but rules shouldn't live too long (in current web request usually), so it's not a big deal.
    public SearchDateMustBeTodayOrInFuture(DateTime dateTime, IDateTimeService dateTimeService)
    {
        _dateTime = dateTime;
        _dateTimeService = dateTimeService;
    }

    public bool IsBroken()
        => _dateTime.Date < _dateTimeService.Now().Date;
    
    public string Message => "Search date must be today or in future.";
}