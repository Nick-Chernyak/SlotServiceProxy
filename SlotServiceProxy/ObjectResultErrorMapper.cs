using Microsoft.AspNetCore.Mvc;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy;

/// <summary>
/// Extension for <see cref="Problem"/> to map it to <see cref="ObjectResult"/> with corresponding status code.
/// Similar to in-built BadRequest, Ok, etc. default ASP.NET methods.
/// </summary>
public static class ObjectResultErrorMapper
{
    public static ObjectResult BadGateway(this Problem problem)
        => BuildStatusCodeWithProblemDetails(problem, StatusCodes.Status502BadGateway);
    
    public static ObjectResult InternalServerError(this Problem problem)
        => BuildStatusCodeWithProblemDetails(problem, StatusCodes.Status500InternalServerError);
    
    private static ObjectResult BuildStatusCodeWithProblemDetails(Problem problem, int statusCode)
        => problem.To(ErrorMapper.ToProblemDetails)
            .Do(details => details.Status = statusCode)
            .To(details => new ObjectResult(details) {StatusCode = statusCode});
}

/// <summary>
/// Small extension for mapping Application layer Problem <see cref="Problem"/> to <see cref="ObjectResult"/>
/// with corresponding status code and ProblemDetails <see cref="ProblemDetails"/> in case of error.
/// </summary>
public static class ErrorMapper
{
    //Mapping is not reflect to related RFC rules, but it is enough for this project.
    public static ProblemDetails ToProblemDetails(this Problem problem)
        => new()
        {
            Title = problem.Type.ToString(),
            Detail = problem.Message
        };
}