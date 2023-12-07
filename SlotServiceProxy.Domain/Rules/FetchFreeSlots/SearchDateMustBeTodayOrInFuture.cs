namespace SlotServiceProxy.Domain.Rules.FetchFreeSlots;

public record SearchDateMustBeTodayOrInFuture : IBusinessRule
{
    private readonly DateTime _dateTime;
    
    public SearchDateMustBeTodayOrInFuture(DateTime dateTime) 
        => _dateTime = dateTime;
    
    public bool IsBroken()
        => _dateTime.Date < DateTime.UtcNow.Date;
    
    public string Message => "Search date must be today or in future.";
}