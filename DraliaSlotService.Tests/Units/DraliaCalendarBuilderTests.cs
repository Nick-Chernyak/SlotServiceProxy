using DraliaSlotService.SDK;
using FluentAssertions;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Shared;
using Xunit;

namespace DraliaSlotService.Tests.Units;

public class DraliaCalendarBuilderTests
{
    //Dralia Calendar Builder algorithm consists from 2 parts: 
    //1. Build correct single day (logic about finding place for free slots with specified possible duration
    //  and avoiding busy slots (including lunch) from facility calendar)
    //2. Build whole calendar (logic about finding correct days in week and build correlated days for them.)
    // => make sense to write tests for building correct single day and for building whole week with cases for
    // loop over days in week only (not checking that are slots correct in each day,
    // since same algorithm applied to each day -> we just need iterate correctly through days).
    
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
        
        foreach (var dailyTimeRange in day.Slots)
        {
            dailyTimeRange.Duration.To(s => s.Minutes).Should().Be(facilityWeek.SlotDurationMinutes, 
                "All slots must have same duration as facility slot duration!");
        }
    }

    [Fact]
    public void BuildSingleDay_AllDayIsBusy_DayWithoutFreeSlotsBuiltCorrect()
    {
        //Arrange 
        //Busy slots with duration one hour for each doctor working hour.
        var busySlotsForDefaultMonday = new List<DailyTimeRange>
        {
            new(DefaultWithHourAndMin(8), DefaultWithHourAndMin(9)),
            new(DefaultWithHourAndMin(9), DefaultWithHourAndMin(10)),
            new(DefaultWithHourAndMin(10), DefaultWithHourAndMin(11)),
            new(DefaultWithHourAndMin(11), DefaultWithHourAndMin(12)),
            new(DefaultWithHourAndMin(13), DefaultWithHourAndMin(14)),
        };
        var busyMonday = new FacilityDay(DefaultWorkPeriod, busySlotsForDefaultMonday);
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(busyMonday);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();
        
        //Assert
        dayAsResult.IsSuccess.Should().BeTrue();

        var day = dayAsResult.Data;
        day.Slots.Count.Should().Be(0, "All day is busy -> no free slots expected.");
    }
    
    [Fact]
    public void BuildSingleDay_DuplicatedBusySlots_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        var facilityWeek = FacilityWeekBuilder.EmptyWeek;
        var slotDuration = facilityWeek.SlotDurationMinutes;
        
        //All busy slots are "normal" - have same duration as possible free slot (most expected normal case).
        var busySlotsWithDuplication = new List<DailyTimeRange>
        {
            //duplication - two busy slots for 1h30min between 8h and 9h30min
            new(DefaultWithHourAndMin(8), DefaultWithHourAndMin(9, slotDuration)),
            new(DefaultWithHourAndMin(8), DefaultWithHourAndMin(9, slotDuration)),
            
            //normal single busy slot.
            new(DefaultWithHourAndMin(11), DefaultWithHourAndMin(11, slotDuration)),
            
            //one more duplication - 3 busy slots for 1h between 11h and 12h 
            new(DefaultWithHourAndMin(11, slotDuration), DefaultWithHourAndMin(12)),
            new(DefaultWithHourAndMin(11, slotDuration), DefaultWithHourAndMin(12)),
            new(DefaultWithHourAndMin(11, slotDuration), DefaultWithHourAndMin(12)),
        };
        
        var expectedFreeSlots = new List<DailyTimeRange>
        {
            // Slots between 9:00 and 11:00
            new(DefaultWithHourAndMin(9, slotDuration), DefaultWithHourAndMin(10)),
            new(DefaultWithHourAndMin(10), DefaultWithHourAndMin(10, slotDuration)),
            new(DefaultWithHourAndMin(10, slotDuration), DefaultWithHourAndMin(11)),
            // Slots after 13:00
            new(DefaultWithHourAndMin(13), DefaultWithHourAndMin(13, slotDuration)),
            // Slots between 12:00 and 13:00
            new(DefaultWithHourAndMin(13, slotDuration), DefaultWithHourAndMin(14)),
        };

        
        var dayWithNotEqualBusySlots = new FacilityDay(DefaultWorkPeriod, busySlotsWithDuplication);
        if (dayWithNotEqualBusySlots == null) throw new ArgumentNullException(nameof(dayWithNotEqualBusySlots));
        facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(dayWithNotEqualBusySlots);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();
        
        //Assert
        dayAsResult.IsSuccess.Should().BeTrue();

        var day = dayAsResult.Data;
        day.Slots.Should().HaveCount(expectedFreeSlots.Count, "Calculated slots count must be equal to expected count.");
        
        //This check unite all needed check (same count, same slots).
        day.Slots.Except(expectedFreeSlots).Count().Should().Be(0, "Calculated slots must be equal to expected slots.");
    }

    /// <summary>
    /// This test cover also the situation when available slots are not equal to slot duration.
    /// </summary>
    [Fact]
    public void BuildSingleDay_BusySlotsWithDifferentDuration_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        var notEqualBusySlots = new DailyTimeRange[]
        {
            //1hour
            new(DefaultWithHourAndMin(8), DefaultWithHourAndMin(9)),
            //10min
            new(DefaultWithHourAndMin(9, 30), DefaultWithHourAndMin(9, 40)),
            //Slot duration
            new(DefaultWithHourAndMin(10), DefaultWithHourAndMin(10, 30)),
            //23 - random prime number duration
            new(DefaultWithHourAndMin(11), DefaultWithHourAndMin(11, 23)),
            //1min
            new(DefaultWithHourAndMin(13), DefaultWithHourAndMin(13, 1)),
        };
        var expectedFreeSlots = new DailyTimeRange[]
        {
            new(DefaultWithHourAndMin(9), DefaultWithHourAndMin(9, 30)),
            new(DefaultWithHourAndMin(10, 30), DefaultWithHourAndMin(11)),
            new(DefaultWithHourAndMin(11, 23), DefaultWithHourAndMin(11, 53)),
            new(DefaultWithHourAndMin(13, 1), DefaultWithHourAndMin(13, 31)),
        };
        
        var mondayWithNotEqualBusySlots = new FacilityDay(DefaultWorkPeriod, notEqualBusySlots);
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(mondayWithNotEqualBusySlots);
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        //Act
        var dayAsResult = builder.BuildSingleDay();

        //Assert
        dayAsResult.IsSuccess.Should().BeTrue();
        var day = dayAsResult.Data;
        
        day.Slots.Except(expectedFreeSlots).Count().Should().Be(0, "Calculated slots must be equal to expected slots.");
    }
    
    [Fact]
    public void BuildSingleDay_SlotDurationIsNotDivisibleBy60_DayWithFreeSlotsBuiltCorrect()
    {
        //Arrange
        
        //Big (relatively to one hour) random prime number. > 30 to have less free slots and easier 
        //expected slots structure.
        var primeBigSlotDuration = 47;
        var notEqualBusySlots = new DailyTimeRange[]
        {
            //1hour
            new(DefaultWithHourAndMin(8), DefaultWithHourAndMin(9)),
            //10min
            new(DefaultWithHourAndMin(9, 30), DefaultWithHourAndMin(9, 40)),
            //23 - random prime number duration
            new(DefaultWithHourAndMin(11), DefaultWithHourAndMin(11, 23)),
            //1min
            new(DefaultWithHourAndMin(13), DefaultWithHourAndMin(13, 1)),
        };
        var expectedFreeSlots = new DailyTimeRange[]
        {
            new(DefaultWithHourAndMin(9, 40), DefaultWithHourAndMin(10, 27)),
            new(DefaultWithHourAndMin(13, 1), DefaultWithHourAndMin(13, 48)),
        };
        
        var mondayWithNotEqualBusySlots = new FacilityDay(DefaultWorkPeriod, notEqualBusySlots);
        var facilityWeek = FacilityWeekBuilder.EmptyWeek.WithMonday(mondayWithNotEqualBusySlots)
            with
            {
                SlotDurationMinutes = primeBigSlotDuration,
            };
        
        //Random prime number.
        var builder = new DraliaCalendarBuilder(facilityWeek, DefaultMondayDateTime);
        
        
        //Act
        var dayAsResult = builder.BuildSingleDay();

        //Assert
        dayAsResult.IsSuccess.Should().BeTrue();
        var day = dayAsResult.Data;
        
        day.Slots.Except(expectedFreeSlots).Count().Should().Be(0, "Calculated slots must be equal to expected slots.");
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
        foreach (var dailyTimeRange in calendar.Days.SelectMany(day => day.Slots))
        {
            dailyTimeRange.Duration.To(s => s.Minutes).Should().Be(facilityWeek.SlotDurationMinutes, 
                "All slots must have same duration as facility slot duration!");
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
        foreach (var dailyTimeRange in calendar.Days.SelectMany(day => day.Slots))
        {
            dailyTimeRange.Duration.To(s => s.Minutes).Should().Be(facilityWeek.SlotDurationMinutes, 
                "All slots must have same duration as facility slot duration!");
        }
    }
    
    public static IEnumerable<object[]> IterateDefaultWeekFromMonday()
    {
        for (var i = 0; i < 6; i++)
            yield return new object[] {DefaultMondayDateTime.AddDays(i)};
    }
    
    /// <summary>
    /// Useful to simplify building slots for single day tests (they always are use DefaultMondayDateTime.Date)
    /// </summary>
    private static DateTime DefaultWithHourAndMin(int hours, int min = 0)
        => DefaultMondayDateTime.Date.AddHours(hours).AddMinutes(min);
}