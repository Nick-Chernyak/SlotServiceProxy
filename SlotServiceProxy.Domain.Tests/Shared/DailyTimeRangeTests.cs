using FluentAssertions;
using SlotServiceProxy.Domain.Shared;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Shared;

public class DailyTimeRangeTests
{
    //To avoid weird not clear inline data, I prefer to write similar tests separately, and give
    //them meaningful names.
    //For more complex cases it make sense to write inline data with comments, but those cases is too simple.
    
    [Fact]
    public void Ctor_StartDateIsNotEqualToEndDate_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 2, 10, 0, 0);
        
        // Act
        var act = () => new DailyTimeRange(start, end);
        
        // Assert
        act.Should().ThrowExactly<ArgumentException>();
    }
    
    [Fact]
    public void Ctor_StartDateIsGreaterThanEndDate_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTime(2021, 1, 2, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 10, 0, 0);
        
        // Act
        var act = () => new DailyTimeRange(start, end);
        
        // Assert
        act.Should().ThrowExactly<ArgumentException>();
    }
    
    [Fact]
    public void Ctor_StartDateIsEqualToEndDate_DailyTimeRangeCreated()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 11, 0, 0);
        
        // Act
        var act = new DailyTimeRange(start, end);
        
        // Assert
        act.Should().NotBeNull("DailyTomeRange with zero duration should be valid value.");
    }
    
    
    [Fact]
    public void CompareTo_StartDateIsLessThanOtherStartDate_ReturnsMinusOne()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 11, 0, 0);
        var dailyTimeRange = new DailyTimeRange(start, end);
        var other = new DailyTimeRange(start.AddHours(1), end.AddHours(1));
        
        // Act
        var act = dailyTimeRange.CompareTo(other);
        
        // Assert
        act.Should().Be(-1, "Firstly DailyTimeRange must be compared by start date.");
    }
    
    [Fact]
    public void CompareTo_StartDateIsGreaterThanOtherStartDate_ReturnsOne()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 11, 0, 0);
        var dailyTimeRange = new DailyTimeRange(start, end);
        var other = new DailyTimeRange(start.AddHours(-1), end.AddHours(-1));
        
        // Act
        var act = dailyTimeRange.CompareTo(other);
        
        // Assert
        act.Should().Be(1, "Firstly DailyTimeRange must be compared by start date.");
    }
    
    [Fact]
    public void CompareTo_EqualStartAndEnd_ReturnsZero()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 11, 0, 0);
        var dailyTimeRange = new DailyTimeRange(start, end);
        var other = new DailyTimeRange(start, end);
        
        // Act
        var act = dailyTimeRange.CompareTo(other);
        
        // Assert
        act.Should().Be(0, "DailyTimeRange compared by start date and end date");
    }
    
    [Fact]
    public void CompareTo_DurationsEqual_StartAnEndDifferent_ReturnsNonZero()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 11, 0, 0);
        var dailyTimeRange = new DailyTimeRange(start, end);
        var other = dailyTimeRange with {Start = start.AddHours(1), End = end.AddHours(1)};
        
        // Act
        var act = dailyTimeRange.CompareTo(other);
        
        // Assert
        act.Should().NotBe(0, "DailyTimeRange compared by start date and end date, not by duration.");
    }
    
}