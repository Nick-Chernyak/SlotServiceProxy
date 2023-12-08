using Microsoft.AspNetCore.Mvc;
using SlotServiceProxy.Shared;
using Swashbuckle.AspNetCore.Filters;

namespace SlotServiceProxy.Swagger;

public record ProblemDetailsExamples : IExamplesProvider<ProblemDetails>
{
    public ProblemDetails GetExamples()
        => new()
        {
            Type = ProblemType.InvalidInputData.ToString(),
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "One or more validation errors occurred.",
        };
}