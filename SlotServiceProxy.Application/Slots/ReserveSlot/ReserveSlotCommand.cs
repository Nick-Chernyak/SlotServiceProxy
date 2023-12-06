using MediatR;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Application.Slots.ReserveSlot;

public record ReserveSlotCommand : IRequest<Result<ReservedSlotDto, Problem>>
{
    public NotEmptyString FacilityId { get; init; }
    
    public SlotDto Slot { get; init; }
    
    public string? Comments { get; init; }
    
    public PatientDto Patient { get; init; }
    
    public ReserveSlotCommand(ReserveSlotDto reserveSlotDto)
    {
        FacilityId = new NotEmptyString(reserveSlotDto.FacilityId);
        Slot = reserveSlotDto.Slot;
        Comments = reserveSlotDto.Comments;
        Patient = reserveSlotDto.Patient;
    }
}
