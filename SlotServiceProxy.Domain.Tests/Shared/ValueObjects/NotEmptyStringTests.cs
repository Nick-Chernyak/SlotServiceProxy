using FluentAssertions;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Shared.ValueObjects;

public class NotEmptyStringTests
{
    [Theory]
    [InlineData("valid string")]
    [InlineData("12345")]
    [InlineData("   leading and trailing spaces   ")]
    //Can be much more for sure, but I think it's enough.
    public void ValidNotEmptyString_ShouldCreateInstance(string stringValue)
    {
        // Act
        var createValidNotEmptyString = () => new NotEmptyString(stringValue);

        // Assert
        createValidNotEmptyString.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InvalidNotEmptyString_ShouldThrowException(string invalidStringValue)
    {
        // Act
        var createInvalidNotEmptyString = () => new NotEmptyString(invalidStringValue);

        // Assert
        createInvalidNotEmptyString.Should().Throw<ValueObjectException<NotEmptyString>>();
    }
}