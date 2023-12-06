using SlotServiceProxy.Domain.Rules;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Domain;

/// <summary>
/// 
/// </summary>
public class BusinessRuleValidationException : Exception
{
    public IBusinessRule BrokenRule { get; }

    public Problem Problem => new(BrokenRule.Message, ProblemType.BusinessRuleViolation);

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
        => BrokenRule = brokenRule;
}