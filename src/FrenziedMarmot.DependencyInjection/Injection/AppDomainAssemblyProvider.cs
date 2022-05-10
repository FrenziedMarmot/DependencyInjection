using System.Reflection;

namespace FrenziedMarmot.DependencyInjection;

/// <summary>
///     The app domain scanner.
/// </summary>
public class AppDomainAssemblyProvider : IAssemblyProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AppDomainAssemblyProvider" /> class.
    /// </summary>
    /// <param name="domain">The domain.</param>
    public AppDomainAssemblyProvider(AppDomain domain)
    {
        Domain = domain;
    }

    /// <summary>
    ///     Gets the domain.
    /// </summary>
    public AppDomain Domain { get; }

    /// <inheritdoc cref="IAssemblyProvider" />
    public Assembly[] GetAssemblies()
    {
        return Domain.GetAssemblies();
    }
}