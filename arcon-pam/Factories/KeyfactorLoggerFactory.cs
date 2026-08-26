using Keyfactor.Logging;
using Microsoft.Extensions.Logging;

namespace arcon_pam.Factories;

/// <summary>
/// This lightweight fixture creates an ILoggerFactory that uses the Keyfactor logging system. It is used in unit tests to provide logging capabilities without relying on external logging frameworks.
/// The LogHandler.Factory property has a private getter, so we cannot retrieve the Factory from outside the LogHandler context.
/// </summary>
internal sealed class KeyfactorLoggerFactory : ILoggerFactory
{
    public ILogger CreateLogger(string categoryName) => LogHandler.GetLogger(categoryName);
    public void AddProvider(ILoggerProvider provider) { }
    public void Dispose() { }
}
