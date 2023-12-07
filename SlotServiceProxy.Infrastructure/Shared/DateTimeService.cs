using SlotServiceProxy.Domain.Shared;

namespace SlotServiceProxy.Infrastructure.Shared;

public class DateTimeService : IDateTimeService
{
    public DateTime Now() => DateTime.Now;
}