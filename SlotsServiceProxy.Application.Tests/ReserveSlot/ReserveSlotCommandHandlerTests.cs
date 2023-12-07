using FluentAssertions;
using Moq;
using SlotServiceProxy.Application.Slots.ReserveSlot;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Domain.Rules;
using SlotServiceProxy.Domain.Rules.ReserveSlot;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;
using Xunit;

namespace SlotsServiceProxy.Application.Tests.ReserveSlot;

public class ReserveSlotCommandHandlerTests
{
    private readonly Mock<ITimetableDataSource> _mockedTimeTableDataSource;
    private readonly Mock<IDateTimeService> _mockedDateTimeService;
    private static readonly DateTime DefaultNow = new(2023, 1, 2);
    private readonly DayInTimetable _defaultEmptyDayInTimetable = 
        new(DefaultNow, TimeSpan.FromHours(8), TimeSpan.FromHours(16));
    
    public ReserveSlotCommandHandlerTests()
    {
        _mockedTimeTableDataSource = new Mock<ITimetableDataSource>();
        _mockedDateTimeService = new Mock<IDateTimeService>();
        _mockedDateTimeService.Setup(x => x.Now()).Returns(DefaultNow);
    }
    
    [Fact]
    public async Task Handle_SlotIsIncorrect_ThrowValidateException()
    {
        //Arrange
        var incorrectSlot = new SlotDto {Start = DefaultNow, End = DefaultNow.AddHours(-1)};
        var reserveSlotDto = DefaultReserveSlotDto().To(dto => dto with {Slot = incorrectSlot});
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        
        //Act 
        var act = () => handler.Handle(command, CancellationToken.None);
        
        //Assert
        var exceptionWrapper = 
            await act.Should().ThrowAsync<BusinessRuleValidationException>("Incorrect slot should be detected - exception should be thrown.");
        
        // ReSharper disable once SuspiciousTypeConversion.Global
        (exceptionWrapper.Subject as BusinessRuleValidationException)
            ?.BrokenRule.Should()
            .BeOfType<SlotForReservationMustBeValidDailyTimeRange>($"{nameof(SlotForReservationMustBeValidDailyTimeRange)} rule should be broken.");
        
        //No calls to external services should be done.
        _mockedTimeTableDataSource.Verify(source => 
            source.GetWeekTimetableCalendarAsync(It.IsAny<DateTime>()), Times.Never);
        _mockedTimeTableDataSource.Verify(source =>
            source.ReserveSlotAsync(It.IsAny<DailyTimeRange>(), It.IsAny<PatientInfo>(), It.IsAny<NotEmptyString>(), It.IsAny<string?>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_TimetableSourceCannotGetDayAndReturnProblem_HandlerReturnsSameProblem()
    {
        //Arrange
        var problemFromDataSource = new Problem("Very very bad error, no days at all...", ProblemType.ExternalServiceError);
        var reserveSlotDto = DefaultReserveSlotDto();
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        _mockedTimeTableDataSource.Setup(source => source.GetDayInTimetableAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(problemFromDataSource);
        
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        result.IsFailure.Should().BeTrue("External service error should be returned - no success result.");
        result.Problem.Should().Be(problemFromDataSource, "External service error should be returned.");
        
        _mockedTimeTableDataSource.Verify(source =>
            source.GetDayInTimetableAsync(It.Is<DateTime>(search => search == DefaultNow)), Times.Once);
        _mockedTimeTableDataSource.Verify(source =>
            source.ReserveSlotAsync(It.IsAny<DailyTimeRange>(), It.IsAny<PatientInfo>(), It.IsAny<NotEmptyString>(), It.IsAny<string?>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_EmptyDayForSlotRelatedDate_ReturnProblem()
    {
        //Arrange
        var reserveSlotDto = DefaultReserveSlotDto();
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        _mockedTimeTableDataSource.Setup(source => source.GetDayInTimetableAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(_defaultEmptyDayInTimetable);
        
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        AssertForSlotWhichFailExpectations(result);
    }
    
    [Fact]
    public async Task Handle_SlotIsNotAvailable_ReturnProblem()
    {
        //Arrange
        var today = DefaultNow.Date;
        var todaySlots = new List<DailyTimeRange>
        {
            //Each hour from 8 to 23 = 15 slots
            new(today.AddHours(10), today.AddHours(11)),
            new(today.AddHours(11), today.AddHours(12)),
            new(today.AddHours(12), today.AddHours(13)),
            new(today.AddHours(13), today.AddHours(14)),
            new(today.AddHours(14), today.AddHours(15)),
            new(today.AddHours(15), today.AddHours(16)),
            //new(today.AddHours(16), today.AddHours(17)), - absent asked slot for reservation
            new(today.AddHours(17), today.AddHours(18)),
            new(today.AddHours(18), today.AddHours(19)),
            new(today.AddHours(19), today.AddHours(20)),
            new(today.AddHours(20), today.AddHours(21)),
            new(today.AddHours(21), today.AddHours(22)),
            new(today.AddHours(22), today.AddHours(23)),
        };
        var absentSlot = new SlotDto {Start = today.AddHours(16), End = today.AddHours(17)};
        var reserveSlotDto = DefaultReserveSlotDto().To(x => x with 
            {Slot = absentSlot});
        
        var todayInTimetable = new DayInTimetable(today, new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0));
        todayInTimetable.AddSlots(todaySlots);
        
        _mockedTimeTableDataSource.Setup(source => source.GetDayInTimetableAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(todayInTimetable);
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        AssertForSlotWhichFailExpectations(result);
    }

    [Theory]
    [MemberData(nameof(SlotsAreNotMeetDayExpectationsIterator))]
    public async Task Handle_SlotNotMeetDayExpectations_ReturnProblem(SlotDto slotForReservation)
    {
        //Arrange
        var today = DefaultNow.Date;
        var todaySlots = new List<DailyTimeRange>
        {
            //Each hour from 8 to 23 = 15 slots
            new(today.AddHours(10), today.AddHours(11)),
            new(today.AddHours(11), today.AddHours(12)),
            new(today.AddHours(12), today.AddHours(13)),
            new(today.AddHours(13), today.AddHours(14)),
            new(today.AddHours(14), today.AddHours(15)),
            new(today.AddHours(15), today.AddHours(16)),
        };
        var reserveSlotDto = DefaultReserveSlotDto().To(x => x with 
            {Slot = slotForReservation});
        
        var todayInTimetable = new DayInTimetable(today, new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0));
        todayInTimetable.AddSlots(todaySlots);
        
        _mockedTimeTableDataSource.Setup(source => source.GetDayInTimetableAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(todayInTimetable);
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        AssertForSlotWhichFailExpectations(result);
    }
    
    [Fact]
    public async Task Handle_SlotIsCorrect_TimetableSourceReturnProblemAfterReservationCall_HandlerReturnsSameProblem()
    {
        //Arrange
        var expectedProblem = new Problem("Very very bad error, no reservations are possible...", ProblemType.ExternalServiceError);
        //Valid day and valid slot for reservation must be used in this scenario.
        var (todayInTimetable, reserveSlotDto) = ValidArrangeWithCorrectAndAvailableRequestedSlot();
        var slotForReservation = reserveSlotDto.Slot;
        
        _mockedTimeTableDataSource.Setup(source => source.GetDayInTimetableAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(todayInTimetable);
        _mockedTimeTableDataSource.Setup(source => source.ReserveSlotAsync(It.IsAny<DailyTimeRange>(), It.IsAny<PatientInfo>(), It.IsAny<NotEmptyString>(), It.IsAny<string?>()))
            .ReturnsAsync(expectedProblem.To(VerificationResult<Problem>.Failure));
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        result.IsFailure.Should().BeTrue("External service error should be returned - no success result.");
        result.Problem.Should().Be(expectedProblem, "External service error should be returned.");
        
        _mockedTimeTableDataSource.Verify(source =>
            source.GetDayInTimetableAsync(It.Is<DateTime>(search => search == DefaultNow)), Times.Once);
        
        var forReservation = new DailyTimeRange(slotForReservation.Start, slotForReservation.End);
        _mockedTimeTableDataSource.Verify(source =>
            source.ReserveSlotAsync(It.Is<DailyTimeRange>(s => s.CompareTo(forReservation) == 0), It.IsAny<PatientInfo>(),
                It.IsAny<NotEmptyString>(), It.IsAny<string?>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_RequestedSlotIsCorrectAndAvailable_HandlerSuccessResult()
    {
        //Arrange
        var (todayInTimetable, reserveSlotDto) = ValidArrangeWithCorrectAndAvailableRequestedSlot();
        var slotForReservation = reserveSlotDto.Slot;
        _mockedTimeTableDataSource.Setup(source => source.GetDayInTimetableAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(todayInTimetable);
        _mockedTimeTableDataSource.Setup(source => source.ReserveSlotAsync(It.IsAny<DailyTimeRange>(), It.IsAny<PatientInfo>(), It.IsAny<NotEmptyString>(), It.IsAny<string?>()))
            .ReturnsAsync(VerificationResult<Problem>.Ok);
        var (handler, command) = CreateHandlerAndCommand(reserveSlotDto);
        
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        var reservedSlot = result.Data;
        reservedSlot.ReservedSlot.Should().BeEquivalentTo(slotForReservation, "Requested slot should be returned as reserved.");
        
        _mockedTimeTableDataSource.Verify(source =>
            source.GetDayInTimetableAsync(It.Is<DateTime>(search => search == DefaultNow)), Times.Once);
        
        var forReservation = new DailyTimeRange(slotForReservation.Start, slotForReservation.End);
        _mockedTimeTableDataSource.Verify(source =>
            source.ReserveSlotAsync(It.Is<DailyTimeRange>(s => s.CompareTo(forReservation) == 0),
                It.IsAny<PatientInfo>(),
                It.IsAny<NotEmptyString>(), It.IsAny<string?>()), Times.Once);
    }
    
    public static IEnumerable<object[]> SlotsAreNotMeetDayExpectationsIterator()
    {
        //Day start is 8:00, day end is 16:00 -> start date less
        yield return new object[] {new SlotDto {Start = DefaultNow.AddHours(6), End = DefaultNow.AddHours(12)}};
        //Day start is 8:00, day end is 16:00 -> end date bigger
        yield return new object[] {new SlotDto {Start = DefaultNow.AddHours(8), End = DefaultNow.AddHours(17)}};
        //Duration is 1 hour, but slot duration is 2 hours
        yield return new object[] {new SlotDto {Start = DefaultNow.AddHours(8), End = DefaultNow.AddHours(10)}};
    }

    private (DayInTimetable, ReserveSlotDto) ValidArrangeWithCorrectAndAvailableRequestedSlot()
    {
        var today = DefaultNow.Date;
        var todaySlots = new List<DailyTimeRange>
        {
            new(today.AddHours(10), today.AddHours(11)),
            new(today.AddHours(11), today.AddHours(12)),
            new(today.AddHours(12), today.AddHours(13)),//Slot for reservation
            new(today.AddHours(13), today.AddHours(14)),
            new(today.AddHours(14), today.AddHours(15)),
            new(today.AddHours(15), today.AddHours(16)),
        };
        var slotForReservation = new SlotDto {Start = today.AddHours(12), End = today.AddHours(13)};
        var reserveSlotDto = DefaultReserveSlotDto().To(x => x with
        {
            Slot = slotForReservation
        });
        
        var todayInTimetable = new DayInTimetable(today, new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0));
        todayInTimetable.AddSlots(todaySlots);
        
        return (todayInTimetable, reserveSlotDto);
    }

    private (ReserveSlotCommandHandler handler, ReserveSlotCommand command) CreateHandlerAndCommand(ReserveSlotDto reserveSlotDto)
    {
        var command = new ReserveSlotCommand(reserveSlotDto);
        var handler = new ReserveSlotCommandHandler(_mockedTimeTableDataSource.Object, _mockedDateTimeService.Object);
        return (handler, command);
    }
    
    private ReserveSlotDto DefaultReserveSlotDto() => new()
    {
        FacilityId = "FacilityId",
        Slot = new SlotDto
        {
            Start = DefaultNow,
            End = DefaultNow.AddHours(1),
        },
        Patient = new PatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john_doe@gmail.com",
            PhoneNumber = "+1234567890"
        },
    };

    private void AssertForSlotWhichFailExpectations(Result<ReservedSlotDto, Problem> result)
    {
        result.IsFailure.Should().BeTrue();
        result.Problem.Type.Should().Be(ProblemType.ExpectationConflict);
        _mockedTimeTableDataSource.Verify(source =>
            source.GetDayInTimetableAsync(It.Is<DateTime>(search => search == DefaultNow)), Times.Once);
        _mockedTimeTableDataSource.Verify(source =>
            source.ReserveSlotAsync(It.IsAny<DailyTimeRange>(), It.IsAny<PatientInfo>(), It.IsAny<NotEmptyString>(), It.IsAny<string?>()), Times.Never);
    }
}