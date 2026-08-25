using arcon_pam.Clients;
using arcon_pam.Models.Arcon.GetTargetDevicePasskey;
using Keyfactor.Logging;
using Keyfactor.Platform.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace arcon_pam.Providers;

public class ArconPAM : IPAMProvider
{
    private readonly ILogger _logger;
    private readonly IArconClientFactory _clientFactory;
    
    public ArconPAM()
    {
        _logger = LogHandler.GetClassLogger<ArconPAM>();
        _clientFactory = new ArconClientFactory(new NullLoggerFactory()); // TODO: Scaffold a LoggerFactory?
    }

    public ArconPAM(IArconClientFactory factory, ILoggerFactory logger)
    {
        _logger = logger.CreateLogger<ArconPAM>();
        _clientFactory = factory;
    }
    
    public string Name => "Arcon";
    
    public string GetPassword(Dictionary<string, string> instanceParameters, Dictionary<string, string> initializationInfo)
    {
        _logger.MethodEntry();

        IArconClient client = _clientFactory.Create("", "", ""); // TODO

        GetTargetDevicePasskeyResponseItem item = client.GetDevicePasskey(new GetTargetDevicePasskeyRequestItem()
        {
            Username = "",
            OpenForHours = 1,
            AliasName = "",
        }).GetAwaiter().GetResult(); // TODO

        string password = item.Password!;
        
        _logger.LogInformation($"Successfully retrieved password from Arcon PAM");
        return password;
    }
}
