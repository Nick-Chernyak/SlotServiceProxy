using DryIoc;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Domain.Slots;

namespace DraliaSlotService;

public static class DraliaCompositionRoot
{
    public static IRegistrator DraliaSource(this IRegistrator registrator)
    {
        registrator.Register<ITimetableDataSource, DraliaTimetableDataSource>();
        return registrator;
    }
}