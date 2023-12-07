using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Domain.Rules;

/// <summary>
/// Represent exception which should be thrown when business rule is broken and handled accordingly of this nature.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    public IBusinessRule BrokenRule { get; }

    public Problem Problem => new(BrokenRule.Message, ProblemType.BusinessRuleViolation);

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
        => BrokenRule = brokenRule;
}