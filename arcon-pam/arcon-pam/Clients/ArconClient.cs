using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using arcon_pam.Exceptions;
using arcon_pam.Models.Arcon.GetTargetDevicePasskey;
using arcon_pam.Models.Arcon.GetToken;
using Microsoft.Extensions.Logging;

namespace arcon_pam.Clients;

public class ArconClient
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string _username;
    private readonly string _password;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    
    public ArconClient(ILogger logger, string host, string username, string password)
        : this(logger, CreateHttpClient(host), username, password)
    {
    }

    public ArconClient(ILogger logger, HttpClient httpClient, string username, string password)
    {
        _logger = logger;
        _httpClient = httpClient;
        _username = username;
        _password = password;
        
        _logger.LogTrace("Successfully instantiated Arcon Client");
    }

    private static HttpClient CreateHttpClient(string host)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"https://{host}"),
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }
    
    public async Task<GetTargetDevicePasskeyResponseItem> GetDevicePasskey(GetTargetDevicePasskeyRequestItem request)
    {
        _logger.LogDebug($"Getting device passkey. Request data: {JsonSerializer.Serialize(request, Options)}");
        GetTokenResponseData token = await GetAccessToken();

        string endpoint = "/DV/api/sdk/gettargetdevicepasskey";
        GetTargetDevicePasskeyResponse response = await Send<GetTargetDevicePasskeyResponse>(HttpMethod.Get, endpoint, new [] {request}, token);

        if (!response.Result.Any())
        {
            throw new ArconException("Arcon returned an empty response for the requested device passkey.");
        }

        GetTargetDevicePasskeyResponseItem data = response.Result.First();

        if (data.Password is null)
        {
            // Arcon may return an error message in the response data if the provided information was not found.
            throw new ArconException(
                $"No password data was found for the provided entry. Please double check that the entry exists in Arcon, and that the entry has password information. Arcon message: {data.Message}");
        }
        
        _logger.LogDebug("Successfully retrieved device passkey from Arcon");

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArconException"></exception>
    private async Task<GetTokenResponseData> GetAccessToken()
    {
        _logger.LogTrace("Getting access token from Arcon");
        string endpoint = "/DV/api/sdk/gettoken";
        GetTokenRequest request = new ()
        {
            Username = _username,
            Password = _password,
        };

        GetTokenResponse response = await Send<GetTokenResponse>(HttpMethod.Post, endpoint, request);
        GetTokenResponseData token = response.Result;
        if (!token.Success || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new ArconException($"Failed to retrieve access token from Arcon: {token.ErrorMessage}");
        }
        
        _logger.LogDebug("Successfully retrieved access token from Arcon");
        
        return response.Result;
    }

    private async Task<T> Send<T>(HttpMethod method, string endpoint, object? payload = null, GetTokenResponseData? token = null)
    {
        string responseString = await SendCore(method, endpoint, payload, token);
        T? response = JsonSerializer.Deserialize<T>(responseString, Options);
        if (response is null)
        {
            throw new SerializationException("Failed to deserialize response");
        }
        return response;
    }

    private async Task<string> SendCore(HttpMethod method, string endpoint, object? payload = null, GetTokenResponseData? token = null)
    {
        _logger.LogTrace($"Creating an HTTP request for {method} {endpoint}");
        var request = new HttpRequestMessage(method, endpoint);

        if (token is not null)
        {
            _logger.LogTrace("Adding authorization header to HTTP request");
            request.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType ?? "Bearer", token.AccessToken);
        }

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }
        
        _logger.LogTrace($"Sending a {request.Method} request to Arcon to the endpoint {request.RequestUri}");

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        
        _logger.LogDebug($"Endpoint {request.Method} {request.RequestUri} returned the HTTP status code {response.StatusCode}");

        string responseString = await response.Content.ReadAsStringAsync();
        return responseString;
    }
}
