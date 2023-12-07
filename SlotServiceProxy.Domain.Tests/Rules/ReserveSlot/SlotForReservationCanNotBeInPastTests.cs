using FluentAssertions;
using SlotServiceProxy.Domain.Rules.ReserveSlot;
using SlotServiceProxy.Domain.Shared;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Rules.ReserveSlot;

//Tests for rules can looks useless a bit, but the main purpose of Unit Tests at all - is tracking important code changes.
//If you will change the rule, you will see that some tests are broken and you will be able to check if it's ok or not.
//Integration tests (tests which test the whole system) can be good candidates, but they are much more slower - 
//so it's better to have some fast tests which will show you that something is broken - at least for the test project.
public class SlotForReservationCanNotBeInPastTests
{
    [Fact]
    public void SlotInFuture_ShouldNotBeBroken()
    {
        // Arrange
        var now = DateTime.Now.Date.AddDays(1);
        var futureTimeRange = new DailyTimeRange(now.AddHours(1), now.AddHours(2));
        var rule = new SlotForReservationCanNotBeInPast(futureTimeRange);

        // Act
        var isBroken = rule.IsBroken();

        // Assert
        isBroken.Should().BeFalse("Slot in future must be valid for the rule.");
    }

    [Fact]
    public void SlotInPast_ShouldBeBroken()
    {
        // Arrange
        var now = DateTime.Now.Date.AddDays(-1);
        var pastTimeRange = new DailyTimeRange(now.AddHours(-2), now.AddHours(-1));
        var rule = new SlotForReservationCanNotBeInPast(pastTimeRange);

        // Act
        var isBroken = rule.IsBroken();

        // Assert
        isBroken.Should().BeTrue("Slot in past must be invalid for the rule.");
    }

    [Fact]
    public void SlotStartingNow_ShouldNotBeBroken()
    {
        // Arrange
        var now = DateTime.Now;
        var currentSlot = new DailyTimeRange(now, now.AddHours(1));
        var rule = new SlotForReservationCanNotBeInPast(currentSlot);

        // Act
        var isBroken = rule.IsBroken();

        // Assert
        isBroken.Should().BeFalse("Slot starting now must be valid for the rule.");
    }
}