using System.Text.RegularExpressions;

namespace SlotServiceProxy.Domain.Shared.ValueObjects;

public record PhoneNumber : IValueObject<string>
{
    public string Value { get; init; }

    //Validating phone number according to E.164 standard.
    public const string PhoneRegex = @"^\+[1-9]\d{1,14}$";

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, PhoneRegex))
            throw new ValueObjectException<Email>($"Unacceptable phone number format. Value: {value}");

        Value = value;
    }

    public static implicit operator string(PhoneNumber notEmptyString) => notEmptyString.Value;
    public static explicit operator PhoneNumber(string value) => new(value);
    public override string ToString() => Value;
}