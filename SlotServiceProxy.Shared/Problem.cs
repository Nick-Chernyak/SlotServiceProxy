namespace SlotServiceProxy.Shared;

/// <summary>
/// Universal problem / error representation.
/// Used for communication between layers to provide non-exceptional handling of unsuccessful (but not exceptional!) operations.
/// </summary>
/// <param name="Message">Human-readable information about details of problem.</param>
/// <param name="Type"> <see cref="ProblemType"/> </param>
public record Problem(string Message, ProblemType? Type = ProblemType.Unknown);

/// <summary>
/// Type of problem, which give more strict context to unsuccessful flow.
/// </summary>
public enum ProblemType
{
    /// <summary>
    /// Default value, should be used in non-business code.
    /// Suitable for application borders (e.g. API, Web, Console...), as default value for unexpected cases.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Should be related to invalid data that provided to the application externally.
    /// </summary>
    InvalidInputData,
    /// <summary>
    /// Related to business rules validation (domain structural rules).
    /// </summary>
    BusinessRuleViolation,
    /// <summary>
    /// Related to cases, when input data is structurally correct,
    /// but it's not possible to perform operation.
    /// </summary>
    ExpectationConflict,
    /// <summary>
    /// Related to cases when application can't perform operation because of
    /// external service error / unavailability / unexpected response / behavior etc.
    /// </summary>
    ExternalServiceError,
    /// <summary>
    /// Pretty similar to <see cref="ProblemType.Unknown"/>, but more concrete (can be set inside Application layer code and near).
    /// </summary>
    InternalServerError,
}