namespace SlotServiceProxy.Domain.Shared.ValueObjects;

public record NotEmptyString : IValueObject<string>
{
    public string Value { get; init; }
    
    public NotEmptyString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValueObjectException<NotEmptyString>("The value was supposed to be a not empty string");

        Value = value;
    }
    
    public static implicit operator string(NotEmptyString notEmptyString) => notEmptyString.Value;
    public static explicit operator NotEmptyString(string value) => new(value);
    public override string ToString() => Value;
}
