using SlotServiceProxy.Domain.Rules;
using SlotServiceProxy.Infrastructure.Application;

namespace SlotServiceProxy.Application;

/// <summary>
/// Base class for handlers which check business domain rules.
/// </summary>
public abstract class BaseRulesCheckerHandler
{
    
    /// <summary>
    /// Check provided <see cref="IBusinessRule"/> business rule
    /// and throw <see cref="BusinessRuleValidationException"/> if it is broken. 
    /// </summary>
    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }

    /// <summary>
    /// Check provided <see cref="IBusinessRule{T}"/> business rule:
    /// - if it is broken, throw <see cref="BusinessRuleValidationException"/>
    /// - if it is not broken, return verified by rule <see cref="IBusinessRule{T}.CheckedValue"/>
    /// </summary>
    protected static T CheckRuleAndGetValue<T>(IBusinessRule<T> rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);

        return rule.CheckedValue;
    }
}