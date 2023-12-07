using FluentAssertions;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Shared.ValueObjects;

public class PhoneNumberTests
{
    /// <summary>
    /// For more information about phone number formats see https://en.wikipedia.org/wiki/E.164
    /// </summary>
    [Theory]
    [InlineData("+123456789012")]
    [InlineData("+442071838750")]
    [InlineData("+12025550123")]
    public void ValidPhoneNumber_ShouldCreateInstance(string phoneNumberValue)
    {
        // Act
        var createValidPhoneNumber = () => new PhoneNumber(phoneNumberValue);

        // Assert
        createValidPhoneNumber.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("+")]
    [InlineData("+1")]
    [InlineData("+12345678901234567890")]
    [InlineData("+abc123")]
    public void InvalidPhoneNumber_ShouldThrowException(string invalidPhoneNumberValue)
    {
        // Act
        var createInvalidPhoneNumber = () => new PhoneNumber(invalidPhoneNumberValue);

        // Assert
        createInvalidPhoneNumber.Should().Throw<ValueObjectException<Email>>();
    }
}