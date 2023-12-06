namespace SlotServiceProxy.Domain.Rules;

public interface IBusinessRule
{
    bool IsBroken();

    string Message { get; }
}

public interface IBusinessRule<out T> : IBusinessRule
{
    T CheckedValue { get; }
}