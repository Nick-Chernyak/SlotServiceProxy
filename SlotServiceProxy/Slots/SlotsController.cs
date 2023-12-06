using MediatR;
using Microsoft.AspNetCore.Mvc;
using SlotServiceProxy.Application.Slots.FetchFreeSlots;
using SlotServiceProxy.Application.Slots.ReserveSlot;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Shared;

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
    /// If given date is Wednesday, then calendar will contain slots Wednesday-Sunday.
    /// </summary>
    /// <param name="searchDate">Base date for search in scope of current week.</param>
    [HttpGet("Availability/Week/{searchDate:datetime}")]
    [ProducesResponseType(typeof(CalendarDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 422)]
    [ProducesResponseType(typeof(ProblemDetails), 500)]
    [ProducesResponseType(typeof(ProblemDetails), 502)]
    public async Task<ActionResult<CalendarDto>> GetAvailableSlotsPerWeek(DateTime searchDate)
        => (await new FetchFreeSlotsQuery(searchDate)
                .To(query => _mediator.Send(query)))
                .To(ResponseByResult);

    /// <summary>
    /// Reserves a slot for a given doctor (id) and patient if it is correct and still available.
    /// </summary>
    [HttpPost("Reserve")]
    public async Task<ActionResult<ReservedSlotDto>> ReserveSlot([FromBody] ReserveSlotDto request)
        => (await new ReserveSlotCommand(request)
                .To(command => _mediator.Send(command)))
                .To(ResponseByResult);
    
}