using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace SlotServiceProxy.Domain.Rules.ReserveSlot;

public record SlotForReservationMustBeValidDailyTimeRange : IBusinessRule<DailyTimeRange>
{
    private readonly DateTime _slotStart;
    private readonly DateTime _slotEnd;

    private string _exceptionMessage;
    
    public string Message => _exceptionMessage;
    public DailyTimeRange CheckedValue { get; private set; }
    
    public SlotForReservationMustBeValidDailyTimeRange(DateTime slotStart, DateTime slotEnd)
    {
        _slotStart = slotStart;
        _slotEnd  = slotEnd;
        _exceptionMessage = string.Empty;
    }
    
    public bool IsBroken()
    {
        try
        {
            CheckedValue = new DailyTimeRange(_slotStart, _slotEnd);
            return false;
        }
        catch (Exception e)
        {
            _exceptionMessage = e.Message;
            return false;
        }
    }
    
}