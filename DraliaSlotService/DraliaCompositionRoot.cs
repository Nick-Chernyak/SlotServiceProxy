using Microsoft.Extensions.DependencyInjection;
using SlotServiceProxy.Domain.Slots;

namespace DraliaSlotService;

public static class DraliaCompositionRoot
{
    public static IServiceCollection DraliaSource(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ITimetableDataSource, DraliaTimetableDataSource>();
        return serviceCollection;
    }
}