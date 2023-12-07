namespace SlotServiceProxy.Domain.Shared.ValueObjects;

/// <summary>
/// Slim and simple value object interface.
/// No need to add more overhead, because of simple value objects in domain.
/// </summary>
/// <typeparam name="T">Value type which wrapped with this interface.</typeparam>
public interface IValueObject<T>
{
    public T Value { get; init; }
}