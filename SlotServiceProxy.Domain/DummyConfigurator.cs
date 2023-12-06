namespace SlotServiceProxy.Domain;

/// <summary>
/// Super simple configurator for getting and storing slot server credentials.
/// Mostly cover one function - remove creds from the repo code.
/// For sure, not real app should use such a solution -> simplified to concentrate on the main topic of the project.
/// </summary>
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