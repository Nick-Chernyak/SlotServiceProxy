using DraliaSlotService;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Infrastructure.Shared;

namespace SlotServiceProxy.Infrastructure.DependencyInjection;

public static class SlotsServiceProxyCompositionRoot
{
    public static IServiceCollection Build(IServiceCollection serviceCollection)
    {
        //https://flurl.dev/docs/client-lifetime/#using-flurl-with-an-ioc-container
        // -> singleton is preferred for the FlurlClientFactory registration.
        serviceCollection.AddSingleton<IFlurlClientFactory, DefaultFlurlClientFactory>();
        serviceCollection.AddScoped<IDateTimeService, DateTimeService>();
        return serviceCollection.DraliaSource();
    }
}