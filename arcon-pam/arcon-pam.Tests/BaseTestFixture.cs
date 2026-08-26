using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace arcon_pam.Tests;

public abstract class BaseTestFixture
{
    protected const string GetTokenEndpoint = "/DV/api/sdk/gettoken";
    protected const string GetPasskeyEndpoint = "/DV/api/sdk/gettargetdevicepasskey";

    protected const string GetTokenSuccessResponse = """
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

    protected const string GetTokenInvalidCredentialsResponse = """
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

    protected const string GetDevicePasskeySuccessResponse = """
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

    protected BaseTestFixture(ITestOutputHelper output)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder =>
        {
            builder.AddProvider(new XUnitLoggerProvider(output, new XUnitLoggerOptions()));
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        ServiceProvider provider = serviceCollection.BuildServiceProvider();
        LoggerFactory = provider.GetRequiredService<ILoggerFactory>();
    }

    protected readonly ILoggerFactory LoggerFactory;
}
