using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;

namespace DraliaSlotService.SDK;

public record ReserveSlotRequest(
    DateTime Start,
    DateTime End,
    PatientDto Patient,
    string Comments,
    string FacilityId)
{
    public static ReserveSlotRequest Build(DailyTimeRange dailyTimeRange,
        PatientInfo patientInfo,
        NotEmptyString facilityId,
        string? comments)
        => new(dailyTimeRange.Start, dailyTimeRange.End,
            PatientDto.FromPatient(patientInfo),
            comments ?? string.Empty,
            facilityId);
}

public record PatientDto(string Name, string SecondName, string Email, string Phone)
{
    public static PatientDto FromPatient(PatientInfo patientInfoDto)
        => new(patientInfoDto.Name, patientInfoDto.SecondName, patientInfoDto.Email, patientInfoDto.Phone);
}