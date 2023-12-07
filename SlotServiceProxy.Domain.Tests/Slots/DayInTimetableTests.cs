using SlotServiceProxy.Domain.Slots;
using FluentAssertions;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Shared;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Slots;

public class DayInTimetableTests
{
    [Theory]
    [InlineData(8, 17)] //Normal day.
    [InlineData(0, 23)] //Whole day.
    [InlineData(8, 9)] //1h day.
    public void Ctor_CreateDayWithValidParameters_ShouldNotThrowException(int startHour, int endHour)
    {
        // Arrange
        var date = DateTime.Now.Date;
        var start = new TimeSpan(startHour, 0, 0);
        var end = new TimeSpan(endHour, 0, 0);

        // Act
        var createDay = () => new DayInTimetable(date, start, end);

        // Assert
        createDay.Should().NotThrow();
    }
    
    [Theory]
    [InlineData(10, 0, 8, 0)]  // Edge case: End before start
    [InlineData(12, 0, 12, 0)] // Edge case: Start and end at the same time
    [InlineData(0, 0, 23, 1)] //Duration a bit more than 24h.
    [InlineData(10, 0, 24, 0)] //0-23 hours border bit.
    [InlineData(-8, 0, 17, 0)] //Negative start hour.
    [InlineData(8, 0, -17, 0)] //Negative end hour.
    public void Ctor_CreateDayWithInvalidTimeRange_ShouldThrowArgumentException(int startHour, int startMinute,
        int endHour, int endMinute)
    {
        // Arrange
        var date = DateTime.Now.Date;
        var start = new TimeSpan(startHour, startMinute, 0);
        var end = new TimeSpan(endHour, endMinute, 0);

        // Act
        var createDay = () => new DayInTimetable(date, start, end);
        
        //Assert
        createDay.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void AddValidSlotToDay_ShouldAddSlot()
    {
        // Arrange
        var date = DateTime.Now.Date;
        var start = new TimeSpan(8, 0, 0);
        var end = new TimeSpan(17, 0, 0);
        var day = new DayInTimetable(date, start, end);
        var validSlot = new DailyTimeRange(date.AddHours(10), date.AddHours(11));

        // Act
        var addSlot = () => day.AddSlots(validSlot.AsArray());

        // Assert
        addSlot.Should().NotThrow();
        day.Slots.Should().Contain(validSlot);
    }
    
    [Fact]
    public void CutOffSlotsBeforeValidDateTime_ShouldRemoveSlots()
    {
        // Arrange
        var date = DateTime.Now.Date;
        var start = new TimeSpan(8, 0, 0);
        var end = new TimeSpan(17, 0, 0);
        var day = new DayInTimetable(date, start, end);
        var slotToCutOff = new DailyTimeRange(date.AddHours(10), date.AddHours(11));
        day.AddSlots(new List<DailyTimeRange> { slotToCutOff });

        // Act
        var cutOffSlots = () => day.CutOffSlotsBefore(date.AddHours(11).AddMinutes(1));

        // Assert
        cutOffSlots.Should().NotThrow();
        day.Slots.Should().NotContain(slotToCutOff);
    }

    [Theory]
    [InlineData(1)]//In future.
    [InlineData(-1)]//Yesterday.
    public void CutOffSlotsBeforeInvalidDateTime_ShouldThrowArgumentException(int addDaysToNow)
    {
        // Arrange
        var date = DateTime.Now.Date.AddDays(addDaysToNow);
        var start = new TimeSpan(8, 0, 0);
        var end = new TimeSpan(17, 0, 0);
        var day = new DayInTimetable(date, start, end);

        // Act
        var cutOffInvalidSlots = () => day.CutOffSlotsBefore(date.AddDays(1).AddHours(11));

        // Assert
        cutOffInvalidSlots.Should().Throw<ArgumentException>();
    }
}

