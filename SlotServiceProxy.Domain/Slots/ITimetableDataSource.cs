﻿using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Domain.Slots;

/// <summary>
/// Define the contract for the timetable data source (actually repository pattern).
/// </summary>
public interface ITimetableDataSource
{
    Task<Result<OwnedTimetable, Problem>> GetWeekTimetableCalendarAsync(DateTime from);

    Task<Result<DayInTimetable, Problem>> GetDayInTimetableAsync(DateTime searchDate);
    
    Task<VerificationResult<Problem>> ReserveSlotAsync(DailyTimeRange dailyTimeRange,
        PatientInfo patientInfo,
        NotEmptyString facilityId, 
        string? comments);
}