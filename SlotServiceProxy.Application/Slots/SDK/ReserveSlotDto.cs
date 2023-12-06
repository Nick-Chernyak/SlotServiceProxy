using System.ComponentModel.DataAnnotations;

namespace SlotServiceProxy.Application.Slots.SDK;

/// <summary>
/// Request data model for reserving a slot.
/// </summary>
public record ReserveSlotDto
{
    /// <summary>
    /// Id of the facility which owns the calendar.
    /// Required.
    /// </summary>
    [Required]
    public required string FacilityId { get; init; }

    /// <summary>
    /// Slot which should be reserved within one day.
    /// </summary>
    [Required]
    public required SlotDto Slot { get; init; }

    /// <summary>
    /// Patient who wants to reserve the slot.
    /// Required.
    /// </summary>
    [Required]
    public required PatientDto Patient { get; init; }

    /// <summary>
    /// Patient's comments for the doctor.
    /// Optional.
    /// </summary>
    //Dummy value, since no real requirement is given, just basic heuristic number.
    [StringLength(200)]
    public string? Comments { get; init; }
}