namespace SlotServiceProxy.Infrastructure;

public static class DummyConfigurator
{
    private static IDictionary<string, string> _draliaCredentials = new Dictionary<string, string>();
    
    public static IDictionary<string, string> DraliaCredentials {
        get => _draliaCredentials;
        set
        {
            if (_draliaCredentials.Count == 0)
            {
               _draliaCredentials = value;   
            }
            else
            {
                 throw new InvalidOperationException("Dralia credentials already set!");
            }
        }
    }
}