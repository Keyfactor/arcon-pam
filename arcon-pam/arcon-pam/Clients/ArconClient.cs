using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using arcon_pam.Models.Arcon.GetTargetDevicePasskey;
using arcon_pam.Models.Arcon.GetToken;

namespace arcon_pam.Clients;

public class ArconClient
{
    private readonly HttpClient _httpClient;
    private readonly string _username;
    private readonly string _password;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    
    public ArconClient(string host, string username, string password)
    {
        _username = username;
        _password = password;

        _httpClient = new HttpClient();

        _httpClient.BaseAddress = new Uri($"https://{host}");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public async Task<GetTargetDevicePasskeyResponseItem> GetDevicePasskey(GetTargetDevicePasskeyRequestItem request)
    {
        string endpoint = "/DV/api/sdk/gettargetdevicepasskey";
        GetTargetDevicePasskeyResponse response = await Send<GetTargetDevicePasskeyResponse>(HttpMethod.Get, endpoint, new [] {request});
        return response.Result.First();
    }

    // TODO: Mark this as private
    public async Task<GetTokenResponseData> GetAccessToken()
    {
        string endpoint = "/DV/api/sdk/gettoken";
        GetTokenRequest request = new GetTokenRequest()
        {
            Username = _username,
            Password = _password,
        };
        
        GetTokenResponse response = await Send<GetTokenResponse>(HttpMethod.Post, endpoint, request);
        return response.Result;
    }

    private async Task<T> Send<T>(HttpMethod method, string endpoint, object? payload = null)
    {
        string responseString = await SendCore(method, endpoint, payload);
        T? response = JsonSerializer.Deserialize<T>(responseString, Options);
        if (response is null)
        {
            // throw exception
            throw new NullReferenceException("Response deserialized to null");
        }
        return response;
    }

    private async Task<string> SendCore(HttpMethod method, string endpoint, object? payload = null)
    {
        var request = new HttpRequestMessage(method, endpoint);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        HttpResponseMessage response = await _httpClient.SendAsync(request);

        string responseString = await response.Content.ReadAsStringAsync();
        return responseString;
    }
}
