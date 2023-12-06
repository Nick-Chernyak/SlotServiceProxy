namespace SlotServiceProxy.Domain.Rules;

/// <summary>
/// Represent business (domain) app rule which can be checked.
/// </summary>
public interface IBusinessRule
{
    bool IsBroken();
    
    /// <summary>
    /// Description of the rule.
    /// </summary>
    string Message { get; }
}

/// <summary>
/// Represent business (domain) app rule which can return T checked value.
/// </summary>
public interface IBusinessRule<out T> : IBusinessRule
{
    T CheckedValue { get; }
}