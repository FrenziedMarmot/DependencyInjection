using System;

namespace FrenziedMarmot.DependencyInjection
{
    /// <summary>
    ///     Attribute class for using in dependency injection. Attributes described by this value will be found using the
    ///     extension method `ScanForOptionAttributeInjection` supplied for `IServiceCollection` and will be injected as
    ///     `IOptions{TOptions}` by using the `Configure` method on `IServiceCollection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class InjectableOptionsAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the InjectableAttribute class.
        /// </summary>
        /// <param name="path">The path within configuration. If not supplied, the implementation name is used.</param>
        /// <param name="implementation">The implementation type of the injection. If not supplied, the decorated class is used.</param>
        public InjectableOptionsAttribute(string path = null, Type implementation = null)
        {
            Implementation = implementation;
            Path = path;
        }

        /// <summary>
        ///     Gets or sets the path to the section within IConfiguration.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets or sets the implementation type of the injection.
        /// </summary>
        public Type Implementation { get; set; }
    }
}