using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace DraliaSlotService.SDK;

public record ReserveSlotRequest(
    DateTime Start,
    DateTime End,
    PatientDto Patient,
    string Comments,
    string FacilityId)
{
    public static ReserveSlotRequest FromSlot(DailyTimeRange dailyTimeRange, Patient patient, NotEmptyString facilityId,
        string? comments)
        => new(dailyTimeRange.Start, dailyTimeRange.End, PatientDto.FromPatient(patient), comments ?? string.Empty, facilityId);
}

public record PatientDto(string Name, string SecondName, string Email, string Phone)
{
    public static PatientDto FromPatient(Patient patientDto)
        => new(patientDto.Name, patientDto.SecondName, patientDto.Email, patientDto.Phone);
}