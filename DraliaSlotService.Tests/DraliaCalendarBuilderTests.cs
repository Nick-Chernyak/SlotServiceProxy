using DraliaSlotService.SDK;
using FluentAssertions;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using SlotServiceProxy.Shared;
using Xunit;

namespace DraliaSlotService.Tests;

public class DraliaCalendarBuilderTests
{
    //Dralia Calendar Builder algorithm consists from 2 parts: 
    //1. Build correct single day (logic about finding place for free slots with specified possible duration
    //  and avoiding busy slots (including lunch) from facility calendar)
    //2. Build whole calendar (logic about finding correct days in week and build correlated days for them.)
    // => It doesn't make sense create tests for facility week with all days,
    // because it's just a loop over days in week.
    // => make sense to write tests for building correct single day and for building whole week with cases for
    // loop over days in week only.
    
    /// <summary>
    /// Not big ez and ez calculated work period.
    /// </summary>
    private static readonly WorkPeriod DefaultWorkPeriod = new(8, 14, 12, 13);
    private const int DefaultCountOfFreeSlots = 10;
    private static readonly DateTime DefaultMondayDateTime = new(2023, 1, 2);

    [Fact]
    public void BuildSingleDay_NoBusySlotsExceptLunch_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        var freeMonday = new FacilityDay(DefaultWorkPeriod, Array.Empty<DailyTimeRange>());
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(freeMonday);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();

        //Assert
        dayAsResult.IsSuccess.Should().BeTrue();
        
        var day = dayAsResult.Data;
        day.Slots.Count.Should().Be(DefaultCountOfFreeSlots, "No busy slots except lunch -> default count of free slots");
        day.Slots.Should().AllSatisfy(slot => SlotHasValidDuration(slot, facilityWeek), 
            "All slot durations must be equal to facility slot duration");
    }

    [Fact]
    public void BuildSingleDay_AllDayIsBusy_DayWithoutFreeSlotsBuiltCorrect()
    {
        //Arrange 
        
        //Busy slots with duration one hour for each doctor working hour.
        var busySlotsForDefaultMonday = new List<DailyTimeRange>
        {
            new(DefaultMondayDateTime.Date.AddHours(8), DefaultMondayDateTime.Date.AddHours(9)),
            new(DefaultMondayDateTime.Date.AddHours(9), DefaultMondayDateTime.Date.AddHours(10)),
            new(DefaultMondayDateTime.Date.AddHours(10), DefaultMondayDateTime.Date.AddHours(11)),
            new(DefaultMondayDateTime.Date.AddHours(11), DefaultMondayDateTime.Date.AddHours(12)),
            new(DefaultMondayDateTime.Date.AddHours(13), DefaultMondayDateTime.Date.AddHours(14)),
        };
        var busyMonday = new FacilityDay(DefaultWorkPeriod, busySlotsForDefaultMonday);
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(busyMonday);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();
        
        //Assert
        dayAsResult.IsSuccess.Should().BeTrue("No problems expected for simple case.");

        var day = dayAsResult.Data;
        day.Slots.Count.Should().Be(0, "All day is busy -> no free slots expected.");
    }
    
    [Fact]
    public void BuildSingleDay_DuplicatedBusySlots_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        var facilityWeek = FacilityWeekBuilder.EmptyWeek;
        var slotDuration = facilityWeek.SlotDurationMinutes;
        
        var busySlotsWithDuplication = new List<DailyTimeRange>
        {
            //duplication - two busy slots for 1h30min between 8h and 9h30min
            new(DefaultMondayDateTime.Date.AddHours(8), DefaultMondayDateTime.Date.AddHours(9).AddMinutes(slotDuration)),
            new(DefaultMondayDateTime.Date.AddHours(8), DefaultMondayDateTime.Date.AddHours(9).AddMinutes(slotDuration)),
            
            new(DefaultMondayDateTime.Date.AddHours(11), DefaultMondayDateTime.Date.AddHours(11).AddMinutes(slotDuration)),
            
            //one more duplication - 3 busy slots for 1h between 11h and 12h 
            new( DefaultMondayDateTime.Date.AddHours(11).AddMinutes(slotDuration), DefaultMondayDateTime.Date.AddHours(12)),
            new( DefaultMondayDateTime.Date.AddHours(11).AddMinutes(slotDuration), DefaultMondayDateTime.Date.AddHours(12)),
            new( DefaultMondayDateTime.Date.AddHours(11).AddMinutes(slotDuration), DefaultMondayDateTime.Date.AddHours(12)),
        };
        
        var expectedFreeSlots = new List<DailyTimeRange>
        {
            // Slots between 9:00 and 11:00
            new(DefaultMondayDateTime.Date.AddHours(9).AddMinutes(slotDuration), DefaultMondayDateTime.Date.AddHours(10)),
            new(DefaultMondayDateTime.Date.AddHours(10), DefaultMondayDateTime.Date.AddHours(10).AddMinutes(slotDuration)),
            new(DefaultMondayDateTime.Date.AddHours(10).AddMinutes(slotDuration), DefaultMondayDateTime.Date.AddHours(11)),
            // Slots between 11:00 and 12:00
            new(DefaultMondayDateTime.Date.AddHours(13), DefaultMondayDateTime.Date.AddHours(13).AddMinutes(30)),
            // Slots between 12:00 and 13:00
            new(DefaultMondayDateTime.Date.AddHours(13).AddMinutes(30), DefaultMondayDateTime.Date.AddHours(14)),
        };

        
        var mondayWithNotEqualBusySlots = new FacilityDay(DefaultWorkPeriod, busySlotsWithDuplication);
        facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(mondayWithNotEqualBusySlots);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();
        
        //Assert
        dayAsResult.IsSuccess.Should().BeTrue();

        var day = dayAsResult.Data;
        day.Slots.Should().HaveCount(expectedFreeSlots.Count, "Calculated slots count must be equal to expected count.");
        day.Slots.Except(expectedFreeSlots).Count().Should().Be(0, "Calculated slots must be equal to expected slots.");
    }

    /// <summary>
    /// This test cover also the situation when available slots are not equal to slot duration.
    /// </summary>
    [Fact]
    public void BuildSingleDay_BusySlotsWithDifferentDuration_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        var notEqualBusySlots = new List<DailyTimeRange>
        {
            //1hour
            new(DefaultMondayDateTime.Date.AddHours(8), DefaultMondayDateTime.Date.AddHours(9)),
            //10min
            new(DefaultMondayDateTime.Date.AddHours(9).AddMinutes(30), DefaultMondayDateTime.Date.AddHours(9).AddMinutes(40)),
            //Slot duration
            new(DefaultMondayDateTime.Date.AddHours(10), DefaultMondayDateTime.Date.AddHours(10).AddMinutes(30)),
            //23 - random prime number duration
            new(DefaultMondayDateTime.Date.AddHours(11), DefaultMondayDateTime.Date.AddHours(11).AddMinutes(23)),
            //1min
            new(DefaultMondayDateTime.Date.AddHours(13), DefaultMondayDateTime.Date.AddHours(13).AddMinutes(1)),
        };
        var mondayWithNotEqualBusySlots = new FacilityDay(DefaultWorkPeriod, notEqualBusySlots);
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(mondayWithNotEqualBusySlots);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();

        //Assert
        dayAsResult.IsSuccess.Should().BeTrue("No problems expected.");
        var day = dayAsResult.Data;
        //TODO: add more specific assert for each complex case.
    }
    
    //TODO: Must be finished, important case.
    [Theory(Skip = "Not implemented yet.")]
    [InlineData]
    public void BuildSingleDay_SlotDurationIsNotDivisibleBy60_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        var notEqualBusySlots = new List<DailyTimeRange>
        {
            //1hour
            new(DefaultMondayDateTime.Date.AddHours(8), DefaultMondayDateTime.Date.AddHours(9)),
            //10min
            new(DefaultMondayDateTime.Date.AddHours(9).AddMinutes(30), DefaultMondayDateTime.Date.AddHours(9).AddMinutes(40)),
            //Slot duration
            new(DefaultMondayDateTime.Date.AddHours(10), DefaultMondayDateTime.Date.AddHours(10).AddMinutes(30)),
            //23 - random prime number duration
            new(DefaultMondayDateTime.Date.AddHours(11), DefaultMondayDateTime.Date.AddHours(11).AddMinutes(23)),
            //1min
            new(DefaultMondayDateTime.Date.AddHours(13), DefaultMondayDateTime.Date.AddHours(13).AddMinutes(1)),
        };
        var mondayWithNotEqualBusySlots = new FacilityDay(DefaultWorkPeriod, notEqualBusySlots);
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(mondayWithNotEqualBusySlots);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();

        //Assert
        dayAsResult.IsSuccess.Should().BeTrue("No problems expected.");
        var day = dayAsResult.Data;
        
    }
    
    [Theory]
    [MemberData(nameof(IterateDefaultWeekFromMonday))]
    public void BuildCalendar_WholeWeekWithFreeSlots_DaysWithFreeSlotsBuiltCorrect(DateTime searchDate)
    {
        //Arrange
        
        //We can don't care about work period and busy slots,
        //since we interesting in testing DraliaCalendar week loop, not free slots building.
        var facilityWeek = FacilityWeekBuilder.BuildFullWeekWithoutBusySlots(DefaultWorkPeriod);
        var builder = new DraliaCalendarBuilder(facilityWeek, searchDate);
        
        //Act
        var calendar = builder.BuildWholeCalendar();

        //Assert
        
        //Since DayOfWeek enum starts from Sunday (0) -> Monday (1),
        //we should calculate expected number of days in week from 8.
        var expectedNumberOfDays = 8 - (int) searchDate.DayOfWeek;
        calendar.Days.Count.Should().Be(expectedNumberOfDays, "For search date we expect only days from this date to end of the week!" +
                                                              $"Day week with problem: {searchDate.DayOfWeek}");
        foreach (var day in calendar.Days)
        {
            //Check dates of day as well!
            day.Slots.Should().AllSatisfy(slot => SlotHasValidDuration(slot, facilityWeek),
                "All days must have default count of free slots, since no busy slots in facility week.");
        }
    }
    
    
    /// <summary>
    /// This test scenario can looks silly a bit,
    /// but it is important to check that day loop works correct and doesn't depend on day of week,
    /// since we have a bit specific logic with DayOfWeek enum with edge cases for Sunday and
    /// Facility Week model with day of weeks as properties.
    /// </summary>
    /// <param name="absentDay">Day which is not present in Facility Week</param>
    [Theory]
    [MemberData(nameof(IterateDefaultWeekFromMonday))]
    public void BuildCalendar_SomeDayIsAbsentInFacilityCalendar_SuchDayDoesNotAppearInResult(DateTime absentDay)
    {
        //Arrange
        
        //Each test remove one day and search date is always Monday -> we expect 6 days in result.
        const int expectedDayCount = 6;
        var facilityWeek = FacilityWeekBuilder.BuildFullWeekWithoutBusySlots(DefaultWorkPeriod);
        facilityWeek = facilityWeek.WithoutDay(absentDay.DayOfWeek);
        
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var calendar = builder.BuildWholeCalendar();

        //Assert
        
        //Since DayOfWeek enum starts from Sunday (0) -> Monday (1),
        //we should calculate expected number of days in week from 8.
        calendar.Days.Count.Should().Be(expectedDayCount, "One day is must absent only, since it not present in facility calendar." +
                                                          $"Day week with problem: {absentDay.DayOfWeek}");
        foreach (var day in calendar.Days)
        {
            //Check dates of day as well!
            day.Slots.Should().AllSatisfy(slot => SlotHasValidDuration(slot, facilityWeek),
                "All days must have default count of free slots, since no busy slots in facility week.");
        }
    }

    private bool SlotHasValidDuration(DailyTimeRange slot, FacilityWeekResponse facilityWeek) 
        => slot.Duration.To(s => s.Minutes) == facilityWeek.SlotDurationMinutes;


    public static IEnumerable<object[]> IterateDefaultWeekFromMonday()
    {
        for (var i = 0; i < 6; i++)
            yield return new object[] {DefaultMondayDateTime.AddDays(i)};
    }
    

}