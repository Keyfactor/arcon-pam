using arcon_pam.Exceptions;
using arcon_pam.Models;
using arcon_pam.Providers;
using RichardSzalay.MockHttp;
using Xunit.Abstractions;

namespace arcon_pam.Tests.Providers;

public sealed class ArconPAMTests(ITestOutputHelper output) : BaseTestFixture(output)
{
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
        mockHttp.When(HttpMethod.Post, $"https://{host}{GetTokenEndpoint}")
            .WithPartialContent(apiUsername)
            .WithPartialContent(apiPassword)
            .Respond("application/json", GetTokenSuccessResponse);
        mockHttp.When(HttpMethod.Get, $"https://{host}{GetPasskeyEndpoint}")
            .WithPartialContent(assetUsername)
            .WithPartialContent(assetAlias)
            .Respond("application/json", GetDevicePasskeySuccessResponse);

        ArconPAM sut = CreateSut(mockHttp);

        string result = sut.GetPassword(instanceParameters, initializationInfo);

        Assert.Equal("mypassword", result);
    }

    [Fact]
    public void GetPassword_WhenArconAuthenticationFails_PropagatesArconExceptionToCaller()
    {
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        string host = initializationInfo[Constants.InitializationInfo.ApiHost];

        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When(HttpMethod.Post, $"https://{host}{GetTokenEndpoint}")
            .Respond("application/json", GetTokenInvalidCredentialsResponse);

        ArconPAM sut = CreateSut(mockHttp);

        ArconException exception = Assert.Throws<ArconException>(() => sut.GetPassword(instanceParameters, initializationInfo));
        Assert.Contains("Failed to retrieve access token from Arcon: Invalid Credential", exception.Message);
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
            { Constants.InitializationInfo.ApiHost, "host" },
            { Constants.InitializationInfo.ApiUsername, "user" },
            { Constants.InitializationInfo.ApiPassword, "pass" },
        };
    }
}
