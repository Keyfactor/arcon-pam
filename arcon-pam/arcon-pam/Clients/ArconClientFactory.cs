using Microsoft.Extensions.Logging;

namespace arcon_pam.Clients;

public interface IArconClientFactory
{
    IArconClient Create(string host, string username, string password);
}

public class ArconClientFactory : IArconClientFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    
    public ArconClientFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ArconClientFactory>();
    }
    
    public IArconClient Create(string host, string username, string password)
    {
        _logger.LogTrace($"Creating ArconClient");
        
        IArconClient client = new ArconClientBase(_loggerFactory, host, username, password);
        
        _logger.LogTrace("Successfully created ArconClient");
        
        return client;
    }
}
