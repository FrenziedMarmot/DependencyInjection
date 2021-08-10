using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrenziedMarmot.DependencyInjection
{
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
        public static IServiceCollection ScanForAttributeInjection(this IServiceCollection services, params Assembly[] typeAssemblies)
        {
            return InjectTypes(services, typeAssemblies.SelectMany(e => e.GetTypes()).Distinct());
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
        public static IServiceCollection ScanForOptionAttributeInjection(this IServiceCollection services, IConfiguration config,
            params Assembly[] typeAssemblies)
        {
            return InjectOptionTypes(services, config, typeAssemblies.SelectMany(e => e.GetTypes()).Distinct());
        }

        private static IServiceCollection InjectTypes(IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                foreach (InjectableAttribute attr in type.GetCustomAttributes<InjectableAttribute>())
                {
                    Type target = attr.TargetType ?? type;
                    if (attr.Factory != null)
                    {
                        if (!typeof(IInjectableFactory).IsAssignableFrom(attr.Factory))
                        {
                            throw new ArgumentException(
                                @"Injectable factory for `{target.Name}` as specified must implement IInjectableFactory");
                        }

                        IInjectableFactory factory = (IInjectableFactory) Activator.CreateInstance(attr.Factory);
                        services.Add(new ServiceDescriptor(target, factory.Create, attr.Lifetime));
                    }
                    else
                    {
                        services.Add(new ServiceDescriptor(target, attr.Implementation ?? type, attr.Lifetime));
                    }
                }
            }

            return services;
        }

        private static IServiceCollection InjectOptionTypes(IServiceCollection services, IConfiguration config, IEnumerable<Type> types)
        {
            services.AddOptions();
            foreach (Type type in types)
            {
                foreach (InjectableOptionsAttribute attr in type.GetCustomAttributes<InjectableOptionsAttribute>(false))
                {
                    Type target = attr.Implementation ?? type;
                    MethodInfo injectOptionsFn =
                        typeof(InjectionExtensions).GetMethod(nameof(InjectOptions), BindingFlags.NonPublic | BindingFlags.Static);
                    // ReSharper disable once PossibleNullReferenceException - shouldn't be possible using 'nameof' and within the same class.
                    MethodInfo genericMethod = injectOptionsFn.MakeGenericMethod(target);
                    genericMethod.Invoke(null, new object[] {services, attr.Path ?? target.Name, config});
                }
            }

            return services;
        }

        private static IServiceCollection InjectOptions<TOptions>(IServiceCollection services, string name, IConfiguration config)
            where TOptions : class
        {
            return services.Configure<TOptions>(config.GetSection(name));
        }
    }
}