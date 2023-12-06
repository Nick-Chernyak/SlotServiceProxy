namespace SlotServiceProxy.Application.Slots.SDK;

public record ReservedSlotDto(string FacilityId, SlotDto ReservedSlot) : IResponseDto
{
    /// <summary>
    /// Id of the facility which owns the reserved slot (from doctor perspective).
    /// </summary>
    public string FacilityId { get; init; } = FacilityId;

    /// <summary>
    /// Reserved slot.
    /// </summary>
    public SlotDto ReservedSlot { get; init; } = ReservedSlot;
}