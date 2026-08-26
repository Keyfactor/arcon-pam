using arcon_pam.Clients;
using arcon_pam.Exceptions;
using arcon_pam.Models.Arcon.GetTargetDevicePasskey;
using arcon_pam.Providers;
using Moq;
using Xunit.Abstractions;

namespace arcon_pam.Tests.Providers;

public sealed class ArconPAMTests : BaseTestFixture
{
    private readonly Mock<IArconClientFactory> _arconClientFactory = new (); 
    private readonly Mock<IArconClient> _arconClientMock = new ();
    private readonly ArconPAM _sut;
    
    public ArconPAMTests(ITestOutputHelper output) : base(output)
    {
        _arconClientFactory.Setup(p => 
                p.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_arconClientMock.Object);
        
        _sut = new ArconPAM(_arconClientFactory.Object, LoggerFactory);
    }

    [Fact]
    public void GetPassword_WhenClientIsInitialized_IsProvidedExpectedHostInformation()
    {
        SetupHappyPath();
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        string host = initializationInfo["ApiHost"];
        string username = initializationInfo["ApiUsername"];
        string password = initializationInfo["ApiPassword"];
        
        _sut.GetPassword(instanceParameters, initializationInfo);
        
        _arconClientFactory.Verify(p => p.Create(host, username, password), Times.Once);
    }
    
    [Fact]
    public void GetPassword_WhenDevicePasskeyMethodIsCalled_CallsMethodWithExpectedInformation()
    {
        SetupHappyPath();
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();

        string assetUsername = instanceParameters["AssetUsername"];
        string assetAlias = instanceParameters["AssetAlias"];
        
        _sut.GetPassword(instanceParameters, initializationInfo);
        
        _arconClientMock.Verify(p => p.GetDevicePasskey(It.Is<GetTargetDevicePasskeyRequestItem>(item =>
            item.Username == assetUsername && item.AliasName == assetAlias)), Times.Once);
    }
    
    [Fact]
    public void GetPassword_WhenClientIsCalled_ReturnsExpectedPassword()
    {
        string passwordToReturn = "arcontest";
        SetupHappyPath(passwordToReturn);
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        
        string result = _sut.GetPassword(instanceParameters, initializationInfo);
        Assert.Equal(passwordToReturn, result);
    }
    
    [Fact]
    public void GetPassword_WhenClientThrowsException_ReturnsExceptionToCaller()
    {
        SetupHappyPath();
        _arconClientMock.Setup(p => p.GetDevicePasskey(It.IsAny<GetTargetDevicePasskeyRequestItem>()))
            .ThrowsAsync(new ArconException("That did not work!"));
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        
        ArconException exception = Assert.Throws<ArconException>(() => _sut.GetPassword(instanceParameters, initializationInfo));
        Assert.Contains("That did not work!", exception.Message);
    }
    
    [Theory]
    [InlineData("ApiHost")]
    [InlineData("ApiUsername")]
    [InlineData("ApiPassword")]
    public void GetPassword_WhenInitializationInfoIsMissingRequiredParameters_ThrowsException(string keyToRemove)
    {
        SetupHappyPath();
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        
        initializationInfo.Remove(keyToRemove);
        
        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => _sut.GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The initialization info parameter {keyToRemove} is missing a value or not provided", exception.Message);
    }
    
    [Theory]
    [InlineData("ApiHost")]
    [InlineData("ApiUsername")]
    [InlineData("ApiPassword")]
    public void GetPassword_WhenInitializationInfoParameterIsEmpty_ThrowsException(string keyToMakeEmpty)
    {
        SetupHappyPath();
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        
        initializationInfo[keyToMakeEmpty] = "";
        
        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => _sut.GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The initialization info parameter {keyToMakeEmpty} is missing a value or not provided", exception.Message);
    }
    
    [Theory]
    [InlineData("AssetUsername")]
    [InlineData("AssetAlias")]
    public void GetPassword_WhenInstanceParametersIsMissingRequiredParameters_ThrowsException(string keyToRemove)
    {
        SetupHappyPath();
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        
        instanceParameters.Remove(keyToRemove);
        
        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => _sut.GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The instance parameter parameter {keyToRemove} is missing a value or not provided", exception.Message);
    }
    
    [Theory]
    [InlineData("AssetUsername")]
    [InlineData("AssetAlias")]
    public void GetPassword_WhenInstanceParametersParameterIsEmpty_ThrowsException(string keyToMakeEmpty)
    {
        SetupHappyPath();
        
        Dictionary<string, string> initializationInfo = GetInitializationInfo();
        Dictionary<string, string> instanceParameters = GetInstanceParameters();
        
        instanceParameters[keyToMakeEmpty] = "";
        
        MissingParameterException exception = Assert.Throws<MissingParameterException>(() => _sut.GetPassword(instanceParameters, initializationInfo));
        Assert.Contains($"The instance parameter parameter {keyToMakeEmpty} is missing a value or not provided", exception.Message);
    }

    private Dictionary<string, string> GetInstanceParameters()
    {
        return new Dictionary<string, string>()
        {
            { "AssetUsername", "asset" },
            { "AssetAlias", "alias" },
        };
    }

    private Dictionary<string, string> GetInitializationInfo()
    {
        return new Dictionary<string, string>()
        {
            { "ApiHost", "host" },
            { "ApiUsername", "user" },
            { "ApiPassword", "pass" },
        };
    }

    private void SetupHappyPath(string password = "foobar")
    {
        _arconClientMock.Setup(p => p.GetDevicePasskey(It.IsAny<GetTargetDevicePasskeyRequestItem>()))
            .ReturnsAsync(new GetTargetDevicePasskeyResponseItem
        {
            ServerIp = "ip",
            UserName = "asset",
            Password = password,
            AliasName = "alias",
            Status = true,
            Message = "",
        });
    }
}
