using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlotServiceProxy.Application.Slots.FetchFreeSlots;
using SlotServiceProxy.Application.Slots.ReserveSlot;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Shared;
using SlotServiceProxy.Swagger;
using Swashbuckle.AspNetCore.Filters;

namespace SlotServiceProxy.Slots;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : SlotsProxyBaseController
{
    private readonly IMediator _mediator;
    
    public SlotsController(IMediator mediator) 
        => _mediator = mediator;
    
    /// <summary>
    /// Returns calendar for a natural week (Monday-Sunday) starting from a given date.
    /// </summary>
    /// <remarks> Returns <b>natural</b> week base on given date. It means,
    /// if given date is Wednesday, then calendar will contain slots Wednesday-Sunday. 
    /// <para>If day is unavailable for reserving at all (and never was), it will not appear in response.</para>
    /// <para>If day <b>was</b> available for reserving, but no free slots available now - day without free slots will be returned. </para>
    /// </remarks>
    /// <param name="searchDate">Base date for search in scope of current week. Format: yyyy-MM-dd</param>
    [HttpGet("Availability/CurrentWeek/{searchDate:datetime}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CalendarDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 422)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    [ProducesResponseType(typeof(ProblemDetails), 502)]
    [SwaggerResponseExample(200, typeof(CalendarDtoExamples))]
    [SwaggerResponseExample(400, typeof(ProblemDetailsExamples))]
    public async Task<ActionResult<CalendarDto>> GetAvailableSlotsPerWeek(DateTime searchDate)
        => (await new FetchFreeSlotsQuery(searchDate)
                .To(query => _mediator.Send(query)))
                .To(ResponseByResult);

    /// <summary>
    /// Reserves a slot for a given doctor (id) and patient if it is correct and still available.
    /// </summary>
    /// <remarks>If slot reserved successfully, will return reserved slot (time range) back with Id of facility where slot was booked </remarks>
    [HttpPost("Reserve")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReservedSlotDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 422)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    [ProducesResponseType(typeof(ProblemDetails), 502)]
    [SwaggerRequestExample(typeof(ReserveSlotDto), typeof(ReserveSlotExamples))]
    [SwaggerResponseExample(200, typeof(ReservedSlotExamples))]
    [SwaggerResponseExample(400, typeof(ProblemDetailsExamples))]
    public async Task<ActionResult<ReservedSlotDto>> ReserveSlot([FromBody] ReserveSlotDto request)
        => (await new ReserveSlotCommand(request)
                .To(command => _mediator.Send(command)))
                .To(ResponseByResult);
}