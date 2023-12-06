using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace SlotServiceProxy.Domain.Slots;

/// <summary>
/// Very slim model of a doctor. It is needed since we need to provide the doctor id in response to the client.
/// to be able to make a reservation. Maybe not best name and format, draft.
/// </summary>
public class Doctor
{
    public NotEmptyString Id { get; init; }
    
    public DoctorCalendar Calendar { get; init; }
    
    public Doctor(NotEmptyString id, DoctorCalendar calendar)
    {
        Id = id;
        Calendar = calendar;
    }
}