using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace arcon_pam.Tests;

public abstract class BaseTestFixture
{
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
