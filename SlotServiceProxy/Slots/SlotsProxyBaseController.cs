using Microsoft.AspNetCore.Mvc;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Slots;

/// <summary>
/// Base controller with generic logic for any Slot-related controller.
/// </summary>
public abstract class SlotsProxyBaseController : ControllerBase
{
    /// <summary>
    /// Map response from Application layer (Result<TData,Problem>) to ActionResult<TData> with corresponding status code
    /// and <see cref="ProblemDetails">ProblemDetails</see> in case of error.
    /// Problem details is default ASP.NET problem description object for HTTP API responses based
    /// on <see href="https://tools.ietf.org/html/rfc7807"/>.
    /// </summary>
    /// <param name="result">Result of flow from Application layer.</param>
    /// <typeparam name="TData">Type of returned data in case if flow finish successfully.</typeparam>
    protected ActionResult<TData> ResponseByResult<TData>(Result<TData, Problem> result)
        where TData : IResponseDto
        => result.IsSuccess 
            ? result.Data.To(data => Ok(data)) 
            : result.Problem.To(ResponseByProblemType);
    
    private ActionResult ResponseByProblemType(Problem problem)
    {
        return problem.Type switch
        {
            ProblemType.Unknown or ProblemType.InternalServerError => problem.InternalServerError(),
            ProblemType.InvalidInputData => BadRequest(problem.To(ToProblemDetails)),
            ProblemType.ExternalServiceError => problem.BadGateway(),
            ProblemType.BusinessRuleViolation or ProblemType.ExpectationConflict => UnprocessableEntity(problem.To(ToProblemDetails)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private static ProblemDetails ToProblemDetails(Problem problem)
        => new()
        {
            Title = problem.Type.ToString(),
            Detail = problem.Message
        };
}