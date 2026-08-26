using arcon_pam.Clients;
using arcon_pam.Exceptions;
using arcon_pam.Models.Arcon.GetTargetDevicePasskey;
using RichardSzalay.MockHttp;
using Xunit.Abstractions;

namespace arcon_pam.Tests.Clients;

public class ArconClientTests(ITestOutputHelper output) : BaseTestFixture(output)
{
    private const string Host = "arcon.example.com";

    private const string GetDevicePasskeyObjectNoResultData = """
        {
            "Program": "Arcon Micro Service API",
            "Version": "1.0.0.0",
            "StatusCode": 200,
            "Message": "Success",
            "Ticks": "13.7802",
            "ResultCount": "1",
            "AppMessage": "",
            "AppMessageCode": null,
            "Success": true,
            "Result": []
        }
        """;

    private const string GetDevicePasskeyObjectNotFoundResponse = """
        {
            "Program": "Arcon Micro Service API",
            "Version": "1.0.0.0",
            "StatusCode": 200,
            "Message": "Success",
            "Ticks": "13.7802",
            "ResultCount": "1",
            "AppMessage": "",
            "AppMessageCode": null,
            "Success": true,
            "Result": [
                {
                    "serverIp": "",
                    "userName": "arcon",
                    "password": null,
                    "passKey": null,
                    "aliasName": null,
                    "status": false,
                    "message": "Requested vault details not found or disabled."
                }
            ]
        }
        """;

    private ArconClient CreateClient(MockHttpMessageHandler mockHttp)
    {
        var httpClient = new HttpClient(mockHttp)
        {
            BaseAddress = new Uri($"https://{Host}"),
        };
        return new ArconClient(LoggerFactory, httpClient, "username", "password");
    }

    private static GetTargetDevicePasskeyRequestItem CreateRequest() => new()
    {
        Username = "arcon",
        OpenForHours = 1,
        AliasName = "10.10.2.138",
    };

    [Fact]
    public async Task GetDevicePasskey_Success_ReturnsPasskeyAndSendsIssuedTokenAsBearerAuth()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, GetTokenEndpoint)
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, GetPasskeyEndpoint)
            .WithHeaders("Authorization", "bearer token")
            .Respond("application/json", GetDevicePasskeySuccessResponse);

        ArconClient client = CreateClient(mockHttp);

        GetTargetDevicePasskeyResponseItem result = await client.GetDevicePasskey(CreateRequest());

        Assert.Equal("10.10.2.138", result.ServerIp);
        Assert.Equal("mypassword", result.Password);
        Assert.True(result.Status);
    }

    [Fact]
    public async Task GetDevicePasskey_InvalidCredentials_ThrowsArconExceptionWithoutCallingPasskeyEndpoint()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, GetTokenEndpoint)
            .Respond("application/json", GetTokenInvalidCredentialsResponse);
        MockedRequest passkeyRequest = mockHttp.When(HttpMethod.Get, GetPasskeyEndpoint)
            .Respond("application/json", GetDevicePasskeySuccessResponse);

        ArconClient client = CreateClient(mockHttp);

        ArconException exception = await Assert.ThrowsAsync<ArconException>(() => client.GetDevicePasskey(CreateRequest()));

        Assert.Contains("Failed to retrieve access token from Arcon: Invalid Credential", exception.Message);
        Assert.Equal(0, mockHttp.GetMatchCount(passkeyRequest));
    }
    
    [Fact]
    public async Task GetDevicePasskey_NoResultFound_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, GetTokenEndpoint)
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, GetPasskeyEndpoint)
            .Respond("application/json", GetDevicePasskeyObjectNoResultData);

        ArconClient client = CreateClient(mockHttp);

        ArconException exception = await Assert.ThrowsAsync<ArconException>(() => client.GetDevicePasskey(CreateRequest()));

        Assert.Contains("Arcon returned an empty response for the requested device passkey.", exception.Message);
    }

    [Fact]
    public async Task GetDevicePasskey_ObjectNotFound_ThrowsException()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, GetTokenEndpoint)
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, GetPasskeyEndpoint)
            .Respond("application/json", GetDevicePasskeyObjectNotFoundResponse);

        ArconClient client = CreateClient(mockHttp);

        ArconException exception = await Assert.ThrowsAsync<ArconException>(() => client.GetDevicePasskey(CreateRequest()));

        Assert.Contains("No password data was found for the provided entry. Please double check that the entry exists in Arcon, and that the entry has password information. Arcon message: Requested vault details not found or disabled.", exception.Message);
    }
}
