using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SlotServiceProxy.Application.Slots.FetchFreeSlots;
using SlotServiceProxy.Application.Slots.ReserveSlot;

namespace SlotServiceProxy.Infrastructure.DependencyInjection;

public static class MediatrConfiguration
{
    public static IServiceCollection RegisterMediatR(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(typeof(FetchFreeSlotsQuery));
        serviceCollection.AddMediatR(typeof(ReserveSlotCommand));

        return serviceCollection;
    }
}
