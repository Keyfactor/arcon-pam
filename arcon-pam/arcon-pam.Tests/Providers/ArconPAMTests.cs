using arcon_pam.Exceptions;
using arcon_pam.Models;
using arcon_pam.Providers;
using RichardSzalay.MockHttp;
using Xunit.Abstractions;

namespace arcon_pam.Tests.Providers;

public sealed class ArconPAMTests(ITestOutputHelper output) : BaseTestFixture(output)
{
    private const string GetTokenEndpoint = "/DV/api/sdk/gettoken";
    private const string GetPasskeyEndpoint = "/DV/api/sdk/gettargetdevicepasskey";

    private const string GetTokenSuccessResponse = """
        {
            "Program": "Arcon Micro Service API",
            "Version": "1.0.0.0",
            "StatusCode": 200,
            "Message": "Success",
            "Ticks": "8.911",
            "ResultCount": "7",
            "AppMessage": "",
            "AppMessageCode": null,
            "Success": true,
            "Result": {
                "accessToken": "token",
                "expiresIn": "2139-08-26T19:38:41Z",
                "tokenType": "bearer",
                "success": true,
                "message": "OK",
                "errormessage": null,
                "errorcode": null
            }
        }
        """;

    private const string GetTokenInvalidCredentialsResponse = """
        {
            "Program": "Arcon Micro Service API",
            "Version": "1.0.0.0",
            "StatusCode": 200,
            "Message": "Success",
            "Ticks": "6.7528",
            "ResultCount": "7",
            "AppMessage": "",
            "AppMessageCode": null,
            "Success": true,
            "Result": {
                "accessToken": null,
                "expiresIn": "0001-01-01T00:00:00",
                "tokenType": null,
                "success": false,
                "message": null,
                "errormessage": "Invalid Credential",
                "errorcode": "InvalidCredentials-101 -TC-GT "
            }
        }
        """;

    private const string GetDevicePasskeySuccessResponse = """
        {
            "Program": "Arcon Micro Service API",
            "Version": "1.0.0.0",
            "StatusCode": 200,
            "Message": "Success",
            "Ticks": "164.7104",
            "ResultCount": "1",
            "AppMessage": "",
            "AppMessageCode": null,
            "Success": true,
            "Result": [
                {
                    "serverIp": "10.10.2.138",
                    "userName": "arcon",
                    "password": "mypassword",
                    "passKey": "",
                    "aliasName": "10.10.2.138",
                    "status": true,
                    "message": ""
                }
            ]
        }
        """;

    private const string GetDevicePasskeyNoResultsResponse = """
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

    private ArconPAM CreateSut(HttpMessageHandler? httpMessageHandler = null) => new(LoggerFactory, httpMessageHandler);

    [Fact]
    public void GetPassword_WhenCalled_SendsCredentialsAndAssetInfoToConfiguredHostAndReturnsPassword()
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        string host = initializationInfo[Constants.InitializationInfo.ApiHost];
        string apiUsername = initializationInfo[Constants.InitializationInfo.ApiUsername];
        string apiPassword = initializationInfo[Constants.InitializationInfo.ApiPassword];
        string assetUsername = instanceParameters[Constants.InstanceParameters.AssetUsername];
        string assetAlias = instanceParameters[Constants.InstanceParameters.AssetAlias];

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, $"{host}{GetTokenEndpoint}")
            .WithPartialContent(apiUsername)
            .WithPartialContent(apiPassword)
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, $"{host}{GetPasskeyEndpoint}")
            .WithHeaders("Authorization", "bearer token")
            .WithPartialContent(assetUsername)
            .WithPartialContent(assetAlias)
            .Respond("application/json", GetDevicePasskeySuccessResponse);

        ArconPAM sut = CreateSut(mockHttp);

        string result = sut.GetPassword(instanceParameters, initializationInfo);

        Assert.Equal("mypassword", result);
    }

    [Fact]
    public void GetPassword_WhenArconAuthenticationFails_PropagatesArconExceptionToCallerWithoutRequestingPasskey()
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        string host = initializationInfo[Constants.InitializationInfo.ApiHost];

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, $"{host}{GetTokenEndpoint}")
            .Respond("application/json", GetTokenInvalidCredentialsResponse);
        MockedRequest passkeyRequest = mockHttp.When(HttpMethod.Get, $"{host}{GetPasskeyEndpoint}")
            .Respond("application/json", GetDevicePasskeySuccessResponse);

        ArconPAM sut = CreateSut(mockHttp);

        ArconException exception = Assert.Throws<ArconException>(() => sut.GetPassword(instanceParameters, initializationInfo));

        Assert.Contains("Failed to retrieve access token from Arcon: Invalid Credential", exception.Message);
        Assert.Equal(0, mockHttp.GetMatchCount(passkeyRequest));
    }

    [Fact]
    public void GetPassword_WhenArconReturnsNoResults_PropagatesArconExceptionToCaller()
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        string host = initializationInfo[Constants.InitializationInfo.ApiHost];

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, $"{host}{GetTokenEndpoint}")
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, $"{host}{GetPasskeyEndpoint}")
            .Respond("application/json", GetDevicePasskeyNoResultsResponse);

        ArconPAM sut = CreateSut(mockHttp);

        ArconException exception = Assert.Throws<ArconException>(() => sut.GetPassword(instanceParameters, initializationInfo));

        Assert.Contains("Arcon returned an empty response for the requested device passkey.", exception.Message);
    }

    [Fact]
    public void GetPassword_WhenRequestedEntryHasNoPasswordData_PropagatesArconExceptionToCaller()
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        string host = initializationInfo[Constants.InitializationInfo.ApiHost];

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, $"{host}{GetTokenEndpoint}")
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, $"{host}{GetPasskeyEndpoint}")
            .Respond("application/json", GetDevicePasskeyObjectNotFoundResponse);

        ArconPAM sut = CreateSut(mockHttp);

        ArconException exception = Assert.Throws<ArconException>(() => sut.GetPassword(instanceParameters, initializationInfo));

        Assert.Contains("No password data was found for the provided entry", exception.Message);
        Assert.Contains("Please double check that the entry exists in Arcon, and that the entry has password information. Arcon message: Requested vault details not found or disabled.", exception.Message);
    }

    [Theory]
    [InlineData(Constants.InitializationInfo.ApiHost)]
    [InlineData(Constants.InitializationInfo.ApiUsername)]
    [InlineData(Constants.InitializationInfo.ApiPassword)]
    public void GetPassword_WhenInitializationInfoIsMissingRequiredParameters_ThrowsException(string keyToRemove)
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        initializationInfo.Remove(keyToRemove);

        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => CreateSut().GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The initialization info parameter {keyToRemove} is missing a value or not provided", exception.Message);
    }

    [Theory]
    [InlineData(Constants.InitializationInfo.ApiHost)]
    [InlineData(Constants.InitializationInfo.ApiUsername)]
    [InlineData(Constants.InitializationInfo.ApiPassword)]
    public void GetPassword_WhenInitializationInfoParameterIsEmpty_ThrowsException(string keyToMakeEmpty)
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        initializationInfo[keyToMakeEmpty] = "";

        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => CreateSut().GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The initialization info parameter {keyToMakeEmpty} is missing a value or not provided", exception.Message);
    }

    [Theory]
    [InlineData(Constants.InstanceParameters.AssetUsername)]
    [InlineData(Constants.InstanceParameters.AssetAlias)]
    public void GetPassword_WhenInstanceParametersIsMissingRequiredParameters_ThrowsException(string keyToRemove)
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        instanceParameters.Remove(keyToRemove);

        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => CreateSut().GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The instance parameter parameter {keyToRemove} is missing a value or not provided", exception.Message);
    }

    [Theory]
    [InlineData(Constants.InstanceParameters.AssetUsername)]
    [InlineData(Constants.InstanceParameters.AssetAlias)]
    public void GetPassword_WhenInstanceParametersParameterIsEmpty_ThrowsException(string keyToMakeEmpty)
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        instanceParameters[keyToMakeEmpty] = "";

        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => CreateSut().GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The instance parameter parameter {keyToMakeEmpty} is missing a value or not provided", exception.Message);
    }

    private static Dictionary<string, string> GetInstanceParameters()
    {
        return new Dictionary<string, string>()
        {
            { Constants.InstanceParameters.AssetUsername, "asset" },
            { Constants.InstanceParameters.AssetAlias, "alias" },
        };
    }

    private static Dictionary<string, string> GetInitializationInfo()
    {
        return new Dictionary<string, string>()
        {
            { Constants.InitializationInfo.ApiHost, "https://host.example" },
            { Constants.InitializationInfo.ApiUsername, "user" },
            { Constants.InitializationInfo.ApiPassword, "pass" },
        };
    }
}
