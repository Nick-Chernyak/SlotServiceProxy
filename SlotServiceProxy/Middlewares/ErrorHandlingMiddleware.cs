using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SlotServiceProxy.Infrastructure.Application;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Middlewares;

/// <summary>
/// Global Web exception handler.
/// Handle exception from Application layer and map it to <see cref="ProblemDetails">ProblemDetails</see> with corresponding status code.
/// </summary>
public class ErrorHandlingMiddleware
{
   
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next) 
        => _next = next;
    
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var problemDetails = exception switch
        {
            BusinessRuleValidationException businessException => businessException.Problem
                .ToProblemDetails()
                .Do(p => p.Status = StatusCodes.Status422UnprocessableEntity),
            _ => DefaultProblemDetails
        };
        
        await WriteResponseAsync(httpContext, problemDetails);
    }

    private static async Task WriteResponseAsync(HttpContext httpContext, ProblemDetails problemDetails)
    {
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails);
    }
    
    private static ProblemDetails DefaultProblemDetails => new()
    {
        Status = StatusCodes.Status500InternalServerError,
        Title = "An error occurred while processing your request.",
        Detail = "Internal server error occurred.",
    };
}
