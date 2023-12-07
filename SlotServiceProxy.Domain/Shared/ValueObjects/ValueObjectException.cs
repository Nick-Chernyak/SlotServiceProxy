namespace SlotServiceProxy.Domain.Shared.ValueObjects;

/// <b>Marker interface for exception handling purpose only!</b>
public interface IValueObjectException { }

public class ValueObjectException<T> : ArgumentException, IValueObjectException
{
    private const string ErrorMessage = "{0} has invalid value. Details: {1}";
    private static string GetErrorMessage(string typeName, string details) => string.Format(ErrorMessage, typeName, details);

    public ValueObjectException(string message) : base(GetErrorMessage(typeof(T).Name, message)) {}
}