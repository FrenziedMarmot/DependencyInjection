using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace FrenziedMarmot.DependencyInjection;

/// <summary>
///     Extension class for mapping dependency injection via attributes.
/// </summary>
public static class InjectionExtensions
{
    /// <summary>
    ///     Scans for dependency injection via attributes from assemblies which contain the specified "Representative types".
    /// </summary>
    /// <remarks>
    ///     This is supplied because it is often simpler to know a type contained in an assembly you care about as opposed
    ///     to the assembly's full name.
    /// </remarks>
    /// <param name="services">The service collection to specify injections within.</param>
    /// <param name="representativeTypes">The types representing assemblies to scan.</param>
    /// <returns></returns>
    [UsedImplicitly]
    public static IServiceCollection ScanForAttributeInjection(this IServiceCollection services, params Type[] representativeTypes)
    {
        return services.ScanForAttributeInjection(representativeTypes.Select(e => e.Assembly).Distinct().ToArray());
    }

    /// <summary>
    ///     Scans for dependency injection via attributes from the supplied assemblies.
    /// </summary>
    /// <param name="services">The service collection to specify injections within.</param>
    /// <param name="typeAssemblies">The assemblies to scan.</param>
    /// <returns></returns>
    [UsedImplicitly]
    public static IServiceCollection ScanForAttributeInjection(this IServiceCollection services, params Assembly[] typeAssemblies)
    {
        return InjectTypes(services, typeAssemblies.SelectMany(e => e.GetTypes()).Distinct());
    }

    /// <summary>
    ///     Scans for dependency injection via attributes from the supplied scanner's assemblies.
    /// </summary>
    /// <param name="services">The service collection to specify injections within.</param>
    /// <param name="provider">The selected assembly scanner</param>
    /// <param name="filterToInjectable">
    ///     Filters the assemblies in the app domain to only those marked with
    ///     <see cref="InjectableAssemblyAttribute" />
    /// </param>
    /// <returns></returns>
    [UsedImplicitly]
    public static IServiceCollection ScanForAttributeInjection(this IServiceCollection services, IAssemblyProvider provider,
        bool filterToInjectable = true)
    {
        return services.ScanForAttributeInjection(provider.GetAssemblies()
            .Where(e => !filterToInjectable || e.GetCustomAttribute<InjectableAssemblyAttribute>() != null).ToArray());
    }

    /// <summary>
    ///     Scans for dependency injection via attributes from assemblies which contain the specified "Representative types".
    /// </summary>
    /// <remarks>
    ///     This is supplied because it is often simpler to know a type contained in an assembly you care about as opposed
    ///     to the assembly's full name.
    /// </remarks>
    /// <param name="services">The service collection to specify injections within.</param>
    /// <param name="config">The IConfiguration instance to pull settings from.</param>
    /// <param name="representativeTypes">The types representing assemblies to scan.</param>
    /// <returns></returns>
    [UsedImplicitly]
    public static IServiceCollection ScanForOptionAttributeInjection(this IServiceCollection services, IConfiguration config,
        params Type[] representativeTypes)
    {
        return services.ScanForOptionAttributeInjection(config, representativeTypes.Select(e => e.Assembly).Distinct().ToArray());
    }

    /// <summary>
    ///     Scans for dependency injection via attributes from the supplied assemblies.
    /// </summary>
    /// <param name="services">The service collection to specify injections within.</param>
    /// <param name="config">The IConfiguration instance to pull settings from.</param>
    /// <param name="typeAssemblies">The assemblies to scan.</param>
    /// <returns></returns>
    [UsedImplicitly]
    public static IServiceCollection ScanForOptionAttributeInjection(this IServiceCollection services, IConfiguration config,
        params Assembly[] typeAssemblies)
    {
        return InjectOptionTypes(services, config, typeAssemblies.SelectMany(e => e.GetTypes()).Distinct());
    }

    /// <summary>
    ///     Scans for dependency injection via attributes from the supplied assemblies.
    /// </summary>
    /// <param name="services">The service collection to specify injections within.</param>
    /// <param name="config">The IConfiguration instance to pull settings from.</param>
    /// <param name="provider">The assembly scanner for providing the assemblies.</param>
    /// <param name="filterToInjectable">
    ///     Filters the assemblies in the app domain to only those marked with
    ///     <see cref="InjectableAssemblyAttribute" />
    /// </param>
    /// <returns></returns>
    [UsedImplicitly]
    public static IServiceCollection ScanForOptionAttributeInjection(this IServiceCollection services, IConfiguration config,
        IAssemblyProvider provider, bool filterToInjectable = true)
    {
        return services.ScanForOptionAttributeInjection(config,
            provider.GetAssemblies().Where(e => !filterToInjectable || e.GetCustomAttribute<InjectableAssemblyAttribute>() != null)
                .ToArray());
    }

    /// <summary>
    ///     Injects the types into the service collection.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="types">The types.</param>
    /// <returns>An IServiceCollection.</returns>
    private static IServiceCollection InjectTypes(IServiceCollection services, IEnumerable<Type> types)
    {
        var injections = types
            .SelectMany(decorated => decorated.GetCustomAttributes<InjectableAttribute>().Select(attr => (decorated, attr)))
            .Where(e => e.attr != null)
            .OrderBy(e => e.attr.SortOrder);

        foreach ((Type decoratedType, InjectableAttribute attr) in injections)
        {
            Type target = attr.TargetType ?? decoratedType;
            Type impl = attr.Implementation ?? decoratedType;

            if (attr.Factory is not { } factoryType)
            {
                services.Add(new ServiceDescriptor(target, impl, attr.Lifetime));
            }
            else
            {
                ValidateInjectionFactory(target, factoryType);
                IInjectableFactory factory = (IInjectableFactory)Activator.CreateInstance(factoryType);
                services.Add(new ServiceDescriptor(target, factory.Create, attr.Lifetime));
            }
        }

        return services;
    }

    /// <summary>
    ///     Validates the injection factory.
    /// </summary>
    /// <param name="target">The target type that the factory should be generating.</param>
    /// <param name="factoryType">The factory type.</param>
    private static void ValidateInjectionFactory(Type target, Type factoryType)
    {
        if (!typeof(IInjectableFactory).IsAssignableFrom(factoryType))
        {
            throw new ArgumentException(
                @$"Injectable factory for `{target.Name}` as specified must implement IInjectableFactory");
        }

        var isGenericFactory = factoryType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IInjectableFactory<>));
        Type expectedGenericConstraint = typeof(IInjectableFactory<>).MakeGenericType(target);
        if (isGenericFactory && !expectedGenericConstraint.IsAssignableFrom(factoryType))
        {
            throw new ArgumentException(
                @$"Injectable factory for `{target.Name}` provides incompatible IInjectableFactory");
        }
    }

    /// <summary>
    ///     Injects the options for the provided types.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="config">The config.</param>
    /// <param name="types">The types.</param>
    /// <returns>An IServiceCollection.</returns>
    private static IServiceCollection InjectOptionTypes(IServiceCollection services, IConfiguration config, IEnumerable<Type> types)
    {
        services.AddOptions();
        foreach (Type type in types)
        {
            foreach (InjectableOptionsAttribute attr in type.GetCustomAttributes<InjectableOptionsAttribute>(false))
            {
                Type target = attr.Implementation ?? type;
                // Note: We call this via a regular static function on this class because the `.Configure<T>` method on the `IServiceCollection` is an extension method and the reflection in this manner wouldn't work on an extension method.
                MethodInfo injectOptionsFn =
                    typeof(InjectionExtensions).GetMethod(nameof(InjectOptions), BindingFlags.NonPublic | BindingFlags.Static);
                // ReSharper disable once PossibleNullReferenceException - won't be a null reference using 'nameof' within the same class type on a static function.
                MethodInfo genericMethod = injectOptionsFn.MakeGenericMethod(target);
                genericMethod.Invoke(null, new object[] { services, attr.Path ?? target.Name, config });
            }
        }

        return services;
    }

    /// <summary>
    ///     Generic method for calling the generic Configure on services.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="name">The name of the configuration section.</param>
    /// <param name="config">The configuration object.</param>
    /// <returns>An IServiceCollection.</returns>
    private static IServiceCollection InjectOptions<TOptions>(IServiceCollection services, string name, IConfiguration config)
        where TOptions : class
    {
        return services.Configure<TOptions>(config.GetSection(name));
    }
}