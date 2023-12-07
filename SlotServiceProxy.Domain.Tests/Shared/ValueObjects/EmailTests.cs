using FluentAssertions;
using SlotServiceProxy.Domain.Shared.ValueObjects;
using Xunit;

namespace SlotServiceProxy.Domain.Tests.Shared.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("john.doe@example.com")] //Simple case
    [InlineData("alice_smith123@example.co.uk")] //With numbers and underscore and complex domain
    [InlineData("info@company.eu")]//With short domain
    [InlineData("user+name@domain.org")]//With plus sign
    [InlineData("UpperCase@Domai.ORG")]//With upper case letters in local part and domain
    public void ValidEmail_ShouldCreateInstance(string emailValue)
    {
        // Act
        var createValidEmail = () => new Email(emailValue);

        // Assert
        createValidEmail.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid_email")]//Without domain
    [InlineData("user@missingtld.")]//With missing top level domain
    [InlineData("user@.missingtld")]//With missing top level domain
    [InlineData("user@inva@lid.com")]//With two @
    [InlineData("user@inva lid.com")]//With space
    public void InvalidEmail_ShouldThrowException(string invalidEmailValue)
    {
        // Act
        var createInvalidEmail = () => new Email(invalidEmailValue);

        // Assert
        createInvalidEmail.Should().Throw<ValueObjectException<Email>>();
    }
}