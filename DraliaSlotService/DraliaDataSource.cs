using DraliaSlotService.SDK;
using Flurl.Http.Configuration;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace DraliaSlotService;

public class DraliaSlotsDataSource : ISlotsDataSource
{
    private readonly DraliaWrapper _draliaWrapper;

    public DraliaSlotsDataSource(IFlurlClientFactory flurlClientFactory)
        => _draliaWrapper = new DraliaWrapper(flurlClientFactory);

    public async Task<Result<Doctor, ErrorData>> GetDoctorWithWeekCalendarAsync(DateTime from)
    {
        var slotsAsResult = await _draliaWrapper.GetAvailableSlotsPerWeek(from);
        
        return slotsAsResult.IsSuccess
            ? BuildDoctorWithCalendar(slotsAsResult.Data, from)
            : Result<Doctor, ErrorData>.Failure(slotsAsResult.Problem);
    }

    public async Task<VerificationResult<Problem>> ReserveSlotAsync(
        DailyTimeRange dailyTimeRange,
        Patient patient,
        NotEmptyString facilityId, string? comments)
    {
        var request = ReserveSlotRequest.FromSlot(dailyTimeRange, patient, facilityId, comments);
        var result = await _draliaWrapper.TryToReserveSlot(request);
        return result;
    }
    
    public async Task<Result<DayInTimetable, Problem>> GetDayInTimetableAsync(DateTime searchDate)
    {
        var slotsAsResult = await _draliaWrapper.GetAvailableSlotsPerWeek(searchDate);

        return slotsAsResult.IsSuccess
            ? BuildSingleDayInTimetable(slotsAsResult.Data, searchDate)
            : Result<DayInTimetable, Problem>.Failure(new Problem(slotsAsResult.Problem.Message, ProblemType.ExternalServiceError));
    }
    
    private static DoctorCalendar BuildDoctorCalendar(FacilityWeekResponse facilityWeek, DateTime mondayDate)
        => new DraliaCalendarBuilder(facilityWeek, mondayDate).To(b => b.BuildWholeCalendar());
    
    private static Result<DayInTimetable, Problem> BuildSingleDayInTimetable(FacilityWeekResponse facilityWeek, DateTime mondayDate) 
        => new DraliaCalendarBuilder(facilityWeek, mondayDate).To(b => b.BuildSingleDay());

    private static Doctor BuildDoctorWithCalendar(FacilityWeekResponse facilityWeek, DateTime from) =>
        new(new NotEmptyString(facilityWeek.Facility.FacilityId), BuildDoctorCalendar(facilityWeek, from));
}