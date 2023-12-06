using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Domain;

//TODO: already bad name, should be renamed.
/// <summary>
/// Define the contract for the slots data source (actually repository pattern).
/// </summary>
public interface ISlotsDataSource
{
    Task<Result<Doctor, ErrorData>> GetDoctorWithWeekCalendarAsync(DateTime from);

    Task<Result<DayInTimetable, Problem>> GetDayInTimetableAsync(DateTime searchDate);
    
    Task<VerificationResult<Problem>> ReserveSlotAsync(DailyTimeRange dailyTimeRange,
        Patient patient,
        NotEmptyString facilityId, 
        string? comments);
}