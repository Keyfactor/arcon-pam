using arcon_pam.Clients;
using arcon_pam.Exceptions;
using arcon_pam.Factories;
using arcon_pam.Models;
using arcon_pam.Models.Arcon.GetTargetDevicePasskey;
using Keyfactor.Logging;
using Keyfactor.Platform.Extensions;
using Microsoft.Extensions.Logging;

namespace arcon_pam.Providers;

public class ArconPAM : IPAMProvider
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly HttpMessageHandler? _httpMessageHandler;

    public ArconPAM()
        : this(new KeyfactorLoggerFactory())
    {
    }

    public ArconPAM(ILoggerFactory loggerFactory, HttpMessageHandler? httpMessageHandler = null)
    {
        _logger = loggerFactory.CreateLogger<ArconPAM>();
        _loggerFactory = loggerFactory;
        _httpMessageHandler = httpMessageHandler;
    }

    public string Name => "Arcon";
    
    public string GetPassword(Dictionary<string, string> instanceParameters, Dictionary<string, string> initializationInfo)
    {
        _logger.MethodEntry();

        string apiHost = GetRequiredParameter(initializationInfo, Constants.InitializationInfo.ApiHost,
            "initialization info");
        string apiUsername = GetRequiredParameter(initializationInfo, Constants.InitializationInfo.ApiUsername,
            "initialization info");
        string apiPassword = GetRequiredParameter(initializationInfo, Constants.InitializationInfo.ApiPassword,
            "initialization info");
        
        _logger.LogDebug($"Initialization info parameters: {Constants.InitializationInfo.ApiHost}: {apiHost}, " +
                         $"{Constants.InitializationInfo.ApiUsername}: {apiUsername}, {Constants.InitializationInfo.ApiPassword}: [redacted]");

        string assetUsername = GetRequiredParameter(instanceParameters, Constants.InstanceParameters.AssetUsername,
            "instance parameter");
        string assetAlias = GetRequiredParameter(instanceParameters, Constants.InstanceParameters.AssetAlias, 
            "instance parameter");
        
        _logger.LogDebug($"Instance parameters: {Constants.InstanceParameters.AssetUsername}: {assetUsername}, " +
                         $"{Constants.InstanceParameters.AssetAlias}: {assetAlias}");

        try
        {
            HttpClient httpClient = ArconClient.CreateHttpClient(apiHost, _httpMessageHandler);
            ArconClient client = new ArconClient(_loggerFactory, httpClient, apiUsername, apiPassword);

            GetTargetDevicePasskeyResponseItem item = client.GetDevicePasskey(new GetTargetDevicePasskeyRequestItem()
            {
                Username = assetUsername,
                OpenForHours = 1,
                AliasName = assetAlias,
            }).GetAwaiter().GetResult();

            string password = item.Password!;

            _logger.LogInformation($"Successfully retrieved password from Arcon PAM");
            return password;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving password from Arcon PAM: {LogHandler.FlattenException(ex)}");
            throw;
        }
    }

    private string GetRequiredParameter(Dictionary<string, string> dictionary, string key, string dictionaryDescription)
    {
        _logger.LogTrace($"Looking up {dictionaryDescription} parameter '{key}'");

        bool exists = dictionary.TryGetValue(key, out string? value);
        if (!exists || string.IsNullOrWhiteSpace(value))
        {
            string errorMessage = $"The {dictionaryDescription} parameter {key} is missing a value or not provided";
            _logger.LogError(errorMessage);
            throw new MissingParameterException(errorMessage);
        }
        
        _logger.LogTrace($"Successfully retrieved {dictionaryDescription} parameter '{key}'");

        return value;
    }
}
