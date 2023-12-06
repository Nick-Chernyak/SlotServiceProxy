using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace SlotServiceProxy.Domain.Slots;

/// <summary>
/// Diff from <see cref="Timetable"/> that it has OwnerId -> means timetable owned by org/clinic/facility.
/// Represent "owned by someone" timetable.
/// </summary>
public class OwnedTimetable
{
    /// <summary>
    /// Id of timetable owner. Can be legal entity, physical, clinic, doctor and etc.
    /// -> no specific format.
    /// </summary>
    public NotEmptyString OwnerId { get; init; }
    
    public Timetable Timetable { get; init; }
    
    public OwnedTimetable(NotEmptyString ownerId, Timetable timetable)
    {
        OwnerId = ownerId;
        Timetable = timetable;
    }
}