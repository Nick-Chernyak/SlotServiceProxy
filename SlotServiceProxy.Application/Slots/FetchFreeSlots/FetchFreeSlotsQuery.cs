using MediatR;
using SlotServiceProxy.Application.Slots.SDK;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Application.Slots.FetchFreeSlots;

public record FetchFreeSlotsQuery(DateTime SearchDate) : IRequest<Result<CalendarDto, Problem>>;