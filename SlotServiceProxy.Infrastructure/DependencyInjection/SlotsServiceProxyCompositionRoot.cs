using DraliaSlotService;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Flurl.Http.Configuration;
using SlotServiceProxy.Shared;

namespace SlotServiceProxy.Infrastructure.DependencyInjection;

public static class SlotsServiceProxyCompositionRoot
{
    private static IContainer CreateDefaultContainer() =>
        new Container(cfg => cfg.WithDefaultReuse(Reuse.ScopedOrSingleton))
            .WithDependencyInjectionAdapter();

    public static IContainer Build()
    {
        var container = CreateDefaultContainer();
        
        container.Register<IFlurlClientFactory, DefaultFlurlClientFactory>(Reuse.Singleton);
        
        return container.DraliaSource().To<IContainer>();
    }
}