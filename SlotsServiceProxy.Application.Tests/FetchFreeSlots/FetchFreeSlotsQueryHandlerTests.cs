using System.Collections.Immutable;
using FluentAssertions;
using Moq;
using SlotServiceProxy.Application.Slots.FetchFreeSlots;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Domain.Rules;
using SlotServiceProxy.Domain.Rules.FetchFreeSlots;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Domain.Slots;
using SlotServiceProxy.Shared;
using Xunit;

namespace SlotsServiceProxy.Application.Tests.FetchFreeSlots;

public class FetchFreeSlotsQueryHandlerTests
{
    private readonly Mock<ITimetableDataSource> _mockedTimeTableDataSource;
    private readonly Mock<IDateTimeService> _mockedDateTimeService;
    private readonly DateTime _defaultNow = new(2023, 1, 2);
    private readonly OwnedTimetable _emptyTimetable = 
        new(new NotEmptyString("Id"), Timetable.Create(ImmutableSortedSet<DayInTimetable>.Empty));
    
    public FetchFreeSlotsQueryHandlerTests()
    {
        _mockedTimeTableDataSource = new Mock<ITimetableDataSource>();
        _mockedDateTimeService = new Mock<IDateTimeService>();
        _mockedDateTimeService.Setup(x => x.Now()).Returns(_defaultNow);
    }
    
    [Theory]
    [InlineData(0)]//Today
    [InlineData(1)]//Tomorrow
    public async Task Handle_SearchDateIsNorOrInFuture_TimetableDataSourceReturnOk_HandlerReturnOk(int addDays)
    {
        //Arrange
        //Empty week - we don't care about it in this scenario
        _mockedTimeTableDataSource.Setup(source => source.GetWeekTimetableCalendarAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(Result<OwnedTimetable, Problem>.Ok(_emptyTimetable));
        var searchDate = _defaultNow.AddDays(addDays);
        var query = searchDate.To(d => new FetchFreeSlotsQuery(d));
        var handler = new FetchFreeSlotsQueryHandler(_mockedTimeTableDataSource.Object, _mockedDateTimeService.Object);
        
        //Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        _mockedTimeTableDataSource.Verify(source =>
            source.GetWeekTimetableCalendarAsync(It.Is<DateTime>(search => search == searchDate)), Times.Once);
    }
    
    [Fact]
    public async Task Handle_SearchDateInPast_ThrowValidateException()
    {
        //Arrange
        var yesterday = _defaultNow.Date.AddDays(-1);
        var (handler, query) = CreateHandlerAndQuery(yesterday);
        
        //Act 
        var act = () => handler.Handle(query, CancellationToken.None);
        
        //Assert
        var exceptionWrapper = 
            await act.Should().ThrowAsync<BusinessRuleValidationException>("It is not allowed to search in past - exception should be thrown.");
        
        // ReSharper disable once SuspiciousTypeConversion.Global
        (exceptionWrapper.Subject as BusinessRuleValidationException)
            ?.BrokenRule.Should()
            .BeOfType<SearchDateMustBeTodayOrInFuture>($"{nameof(SearchDateMustBeTodayOrInFuture)} rule should be broken.");
       
        //No calls to external services should be done.
        _mockedTimeTableDataSource.Verify(source => 
            source.GetWeekTimetableCalendarAsync(It.IsAny<DateTime>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_TimetableDataSourceReturnProblem_HandlerReturnsSameProblem()
    {
        //Arrange
        var problemFromDataSource = new Problem("Very very bad error", ProblemType.ExternalServiceError);
        var (handler, query) = CreateHandlerAndQuery(_defaultNow);
       
        _mockedTimeTableDataSource.Setup(source => source.GetWeekTimetableCalendarAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(problemFromDataSource);
        
        //Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.IsFailure.Should().BeTrue("External service error should be returned - no success result.");
        result.Problem.Should().Be(problemFromDataSource, "External service error should be returned.");
        _mockedTimeTableDataSource.Verify(source =>
            source.GetWeekTimetableCalendarAsync(It.Is<DateTime>(search => search == _defaultNow)), Times.Once);
    }

    [Fact]
    public async Task Handle_TimeTableWithTodayReturned_ButSomeSlotsInPast_SlotsInPastCutted()
    {
        //Arrange
        //Setup timetable with today day with a lot of slots.
        var today = _defaultNow.Date;
        var todaySlots = new List<DailyTimeRange>
        {
            //Each hour from 8 to 23 = 15 slots
            new(today.AddHours(10), today.AddHours(11)),
            new(today.AddHours(11), today.AddHours(12)),
            new(today.AddHours(12), today.AddHours(13)),
            new(today.AddHours(13), today.AddHours(14)),
            new(today.AddHours(14), today.AddHours(15)),
            new(today.AddHours(15), today.AddHours(16)),
            new(today.AddHours(16), today.AddHours(17)),
            new(today.AddHours(17), today.AddHours(18)),
            new(today.AddHours(18), today.AddHours(19)),
            new(today.AddHours(19), today.AddHours(20)),
            new(today.AddHours(20), today.AddHours(21)),
            new(today.AddHours(21), today.AddHours(22)),
            new(today.AddHours(22), today.AddHours(23)),
        };
        var todayDay = new DayInTimetable(today, new TimeSpan(8, 0, 0), new TimeSpan(23, 0, 0));
        todayDay.AddSlots(todaySlots);
        var timetable = Timetable.Create(new List<DayInTimetable> {todayDay}.ToImmutableSortedSet());
        var ownedTimetable = new OwnedTimetable(new NotEmptyString("Id"), timetable);

        //Setup current time to 16:00
        const int rightNowHour = 16;
        //At all 15 slots, but 8 of them in past, so 7 slots should be returned.
        const int countSlotsAfterCut = 7;
        _mockedDateTimeService.Reset();
        _mockedDateTimeService.Setup(x => x.Now()).Returns(_defaultNow.AddHours(rightNowHour));
        
        _mockedTimeTableDataSource.Setup(source => source.GetWeekTimetableCalendarAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(Result<OwnedTimetable, Problem>.Ok(ownedTimetable));
        var (handler, query) = CreateHandlerAndQuery(_defaultNow);
        
        //Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.CurrentWeek.Days.Should().HaveCount(1, "Only one day in timetable from external service!");
        
        var day = result.Data.CurrentWeek.Days.First();
        day.FreeSlots.Should().HaveCount(countSlotsAfterCut, "Only slots after current time should be returned.");
        
        //They are ordered by start time, so first slot should be the first slot after current time.
        var firstSlot = day.FreeSlots.First();
        firstSlot.Should()
            .Match<SlotDto>(slot => slot.Start == today.AddHours(rightNowHour), "First slot should start from current time.");
        
        _mockedTimeTableDataSource.Verify(source =>
            source.GetWeekTimetableCalendarAsync(It.Is<DateTime>(search => search == _defaultNow)), Times.Once);
    }

    private (FetchFreeSlotsQueryHandler handler, FetchFreeSlotsQuery query) CreateHandlerAndQuery(DateTime searchDate)
    {
        var query = searchDate.To(d => new FetchFreeSlotsQuery(d));
        var handler = new FetchFreeSlotsQueryHandler(_mockedTimeTableDataSource.Object, _mockedDateTimeService.Object);
        return (handler, query);
    }
}