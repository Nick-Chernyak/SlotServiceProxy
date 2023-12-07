using FluentAssertions;
using SlotServiceProxy.Domain.Rules.FetchFreeSlots;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Rules.FetchFreeSlots;

//Tests for rules can looks useless a bit, but the main purpose of Unit Tests at all - is tracking important code changes.
//If you will change the rule, you will see that some tests are broken and you will be able to check if it's ok or not.
//Integration tests (tests which test the whole system) can be good candidates, but they are much more slower - 
//so it's better to have some fast tests which will show you that something is broken - at least for the test project.
public class SearchDateMustBeTodayOrInFutureTests
{
    [Fact]
    public void ProvidedSearchDateIsToday_ShouldNotBeBroken()
    {
        // Arrange
        //Yes, this is very bad practice to use DateTime.Now in tests
        // (they can crush on CI/CD for example in edge cases because of timezones / day-night changes etc.)
        //, but I do it ONLY for simplicity. No chance to write it for production code.
        var now = DateTime.Now.Date;
        var rule = new SearchDateMustBeTodayOrInFuture(now);

        // Act
        var isBroken = rule.IsBroken();

        // Assert
        isBroken.Should().BeFalse("Search date is today must be valid for the rule.");
    }
    
    [Fact]
    public void ProvidedSearchDateInFuture_ShouldNotBeBroken()
    {
        // Arrange
        var searchDate = DateTime.Now.Date.AddDays(1);
        var rule = new SearchDateMustBeTodayOrInFuture(searchDate);

        // Act
        var isBroken = rule.IsBroken();

        // Assert
        isBroken.Should().BeFalse("Search date is in future must be valid for the rule.");
    }
    
    [Fact]
    public void ProvidedSearchDateInPast_ShouldBeBroken()
    {
        // Arrange
        var searchDate = DateTime.Now.Date.AddDays(-1);
        var rule = new SearchDateMustBeTodayOrInFuture(searchDate);

        // Act
        var isBroken = rule.IsBroken();

        // Assert
        isBroken.Should().BeTrue("Search date is in past must be invalid for the rule.");
    }
}