using System.Text.RegularExpressions;

namespace SlotServiceProxy.Domain.Shared.ValueObjects;

public record Email : IValueObject<string>
{
    public string Value { get; init; }
    
    //Rather simple regex for email validation. It's not perfect (according to related RFC's), but it's good enough for test task.
    public const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    
    private static readonly Regex EmailRegex = new(EmailPattern, RegexOptions.Compiled);
    
    public Email(string value)
    {
        if(string.IsNullOrWhiteSpace(value) || !EmailRegex.IsMatch(value))
            throw new ValueObjectException<Email>($"Incorrect email format. Value: {value}");
        
        Value = value;
    }
    
    public static implicit operator string(Email notEmptyString) => notEmptyString.Value;
    public static explicit operator Email(string value) => new(value);
    public override string ToString() => Value;
}