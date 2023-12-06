namespace SlotServiceProxy.Shared;

public record Problem(string Message, ProblemType? Type = ProblemType.Unknown);

public enum ProblemType
{ 
    Unknown = 0,
    InvalidInputData,
    BusinessRuleViolation,
    ExpectationConflict,
    ExternalServiceError,
    InternalServerError,
}