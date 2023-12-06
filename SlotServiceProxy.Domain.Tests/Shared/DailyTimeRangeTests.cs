using FluentAssertions;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Shared;

public class DailyTimeRangeTests
{
    
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
        act.Should().NotBeNull();
    }
    
    [Fact]
    public void Duration_StartDateIsEqualToEndDate_DurationIsZero()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 10, 0, 0);
        var dailyTimeRange = new DailyTimeRange(start, end);
        
        // Act
        var act = dailyTimeRange.Duration;
        
        // Assert
        act.Should().Be(TimeSpan.Zero);
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
        act.Should().Be(-1);
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
        act.Should().Be(1);
    }
    
    [Fact]
    public void CompareTo_StartDateIsEqualToOtherStartDate_ReturnsZero()
    {
        // Arrange
        var start = new DateTime(2021, 1, 1, 10, 0, 0);
        var end = new DateTime(2021, 1, 1, 11, 0, 0);
        var dailyTimeRange = new DailyTimeRange(start, end);
        var other = new DailyTimeRange(start, end);
        
        // Act
        var act = dailyTimeRange.CompareTo(other);
        
        // Assert
        act.Should().Be(0);
    }
    
    
}