using DraliaSlotService.SDK;
using Flurl.Http.Configuration;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace DraliaSlotService;

public class DraliaTimetableDataSource : ITimetableDataSource, IDisposable
{
    private readonly DraliaWrapper _draliaWrapper;

    public DraliaTimetableDataSource(IFlurlClientFactory flurlClientFactory)
        => _draliaWrapper = new DraliaWrapper(flurlClientFactory);

    public async Task<Result<OwnedTimetable, Problem>> GetWeekTimetableCalendarAsync(DateTime from)
    {
        var slotsAsResult = await _draliaWrapper.GetAvailableSlotsPerWeek(from);
        
        return slotsAsResult.IsSuccess
            ? BuildDoctorWithCalendar(slotsAsResult.Data, from)
            : Result<OwnedTimetable, Problem>.Failure(slotsAsResult.Problem);
    }

    public async Task<VerificationResult<Problem>> ReserveSlotAsync(
        DailyTimeRange dailyTimeRange,
        PatientInfo patientInfo,
        NotEmptyString facilityId, string? comments)
    {
        var request = ReserveSlotRequest.Build(dailyTimeRange, patientInfo, facilityId, comments);
        var result = await _draliaWrapper.TryToReserveSlot(request);
        return result;
    }
    
    public async Task<Result<DayInTimetable, Problem>> GetDayInTimetableAsync(DateTime searchDate)
    {
        var slotsAsResult = await _draliaWrapper.GetAvailableSlotsPerWeek(searchDate);

        return slotsAsResult.IsSuccess
            ? BuildSingleDayInTimetable(slotsAsResult.Data, searchDate)
            : slotsAsResult.Problem;
    }
    
    private static Timetable BuildDoctorCalendar(FacilityWeekResponse facilityWeek, DateTime mondayDate)
        => new DraliaCalendarBuilder(facilityWeek, mondayDate).To(b => b.BuildWholeCalendar());
    
    private static Result<DayInTimetable, Problem> BuildSingleDayInTimetable(FacilityWeekResponse facilityWeek, DateTime mondayDate) 
        => new DraliaCalendarBuilder(facilityWeek, mondayDate).To(b => b.BuildSingleDay());

    private static OwnedTimetable BuildDoctorWithCalendar(FacilityWeekResponse facilityWeek, DateTime from) =>
        new(new NotEmptyString(facilityWeek.Facility.FacilityId), BuildDoctorCalendar(facilityWeek, from));

    public void Dispose()
    {
        //Since Dralia wrapper builds FlurlClient in constructor, it should be disposed here
        // + DraliaWrapper is not registered in DI container, so it's not disposed automatically
        // But DraliaTimetableDataSource is registered in DI container, so it's disposed automatically
        _draliaWrapper.Dispose();
    }
}