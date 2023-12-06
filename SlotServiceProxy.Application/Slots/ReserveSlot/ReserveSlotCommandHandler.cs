using MediatR;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Domain.Rules.ReserveSlot;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Application.Slots.ReserveSlot;

public class ReserveSlotCommandHandler : BaseRulesCheckerHandler, IRequestHandler<ReserveSlotCommand, Result<ReservedSlotDto, Problem>>
{
    private readonly ISlotsDataSource _slotsDataSource;

    public ReserveSlotCommandHandler(ISlotsDataSource slotsDataSource)
        => _slotsDataSource = slotsDataSource;

    public async Task<Result<ReservedSlotDto, Problem>> Handle(ReserveSlotCommand reserveSlotCommand,
        CancellationToken cancellationToken)
    {
        var slotForReservation = CheckRulesAndGetSlotOrThrow(reserveSlotCommand);
        
        var dayAsResult = await _slotsDataSource.GetDayInTimetableAsync(slotForReservation.Start.Date);

        if (dayAsResult.IsFailure)
            return dayAsResult.Problem;
        var day = dayAsResult.Data;

        var slotVerificationResult = VerifyExternalSlotExpectations(day, slotForReservation);
        if (slotVerificationResult.IsFailure)
            return slotVerificationResult.Problem;
        
        var reservationResult = await _slotsDataSource.ReserveSlotAsync(slotForReservation, 
            reserveSlotCommand.Patient.To(ToPatient), 
            reserveSlotCommand.FacilityId, 
            reserveSlotCommand.Comments);

        return reservationResult switch
        {
            _ when reservationResult.IsSuccess => reserveSlotCommand.Slot.To(reserveSlotCommand.FacilityId, ToReserveSlotResponseDto),
            _ => reservationResult.Problem,
        };
    }

    /// <summary>
    /// If asked for reservation is not valid from fetched day from timetable,
    /// then some recommendations should be built and returned.
    /// No exceptions should be thrown, since this is not business rules of app domain, but requirements of the external slot data service.
    /// </summary>
    private static VerificationResult<Problem> VerifyExternalSlotExpectations(DayInTimetable day, DailyTimeRange slotForReservation)
    {
        var problem = (day, slotForReservation) switch
        {
            //Order is important -> going from the generic (and structural)
            //to more concrete (slot looks correct, but not available).
            _ when day.Slots.Count == 0 => NoSlotsAvailableOnTheDay(day),
            
            _ when day.Start > slotForReservation.Start.TimeOfDay => SlotStartMustBeGreaterThenDayStart(day, slotForReservation),
            
            _ when day.End < slotForReservation.End.TimeOfDay => SlotEndMustBeLessThenDayEnd(day, slotForReservation),
            
            _ when slotForReservation.Duration != day.Slots.First().Duration => SlotDurationIsIncorrect(day, slotForReservation),
            
            _ when !day.Slots.Contains(slotForReservation) => SlotIsUnavailable,
            _ => null
        };

        return problem is not null
            ? VerificationResult<Problem>.Failure(problem)
            : VerificationResult<Problem>.Ok();
    }

    #region External slot expectations

    private static readonly Problem SlotIsUnavailable = new("Slot looks correct, but this time range is unavailable.", 
        ProblemType.ExpectationConflict);
    
    private static Problem NoSlotsAvailableOnTheDay(DayInTimetable day)
        => new($"No slots available on the {day.Date}.", ProblemType.ExpectationConflict);

    private static Problem SlotStartMustBeGreaterThenDayStart(DayInTimetable day, DailyTimeRange slot)
        => new($"Slot start time must be greater than the doctor work day start. Slot starts at {slot.Start:hh\\:mm}, " +
               $"doctor's work day starts at {day.Start:hh\\:mm}", ProblemType.ExpectationConflict);

    private static Problem SlotEndMustBeLessThenDayEnd(DayInTimetable day, DailyTimeRange slot)
        => new($"Slot end time must be less than the doctor work day end. Slot ends at {slot.End:hh\\:mm}, " +
               $"doctor's work day ends at {day.End:hh\\:mm}", ProblemType.ExpectationConflict);

    private static Problem SlotDurationIsIncorrect(DayInTimetable day, DailyTimeRange slot) => new(
        $"Slot duration is incorrect: expected {day.Slots.First().Duration:hh\\:mm}, " +
        $"but provided {slot.Duration:hh\\:mm}", ProblemType.ExpectationConflict);

    #endregion
   
    #region Validate business rules

    private static DailyTimeRange CheckRulesAndGetSlotOrThrow(ReserveSlotCommand reserveSlotCommand)
    {
        var slot = new SlotForReservationMustBeValidDailyTimeRange(
                reserveSlotCommand.Slot.Start,
                reserveSlotCommand.Slot.End)
            .To(CheckRuleAndGetValue);

        new SlotForReservationCanNotBeInPast(slot).Do(CheckRule);

        return slot;
    }

    #endregion
    
    #region Mapping

    private static Patient ToPatient(PatientDto patientDto)
    {
        return patientDto.To(patient => new Patient(new NotEmptyString(patientDto.FirstName), new NotEmptyString(patientDto.LastName),
            patientDto.Email, patientDto.PhoneNumber));
    }

    private static ReservedSlotDto ToReserveSlotResponseDto(SlotDto reservedSlot, NotEmptyString facilityId)
        => new(facilityId, reservedSlot);

    #endregion

}