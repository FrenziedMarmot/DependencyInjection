namespace FrenziedMarmot.DependencyInjection;

/// <summary>
///     Attribute class for using in dependency injection. Attributes described by this value will be found using the
///     extension method `ScanForAttributeInjection` supplied for `IServiceCollection`.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class InjectableAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the InjectableAttribute class.
    /// </summary>
    /// <param name="targetType">The target type of the injection.</param>
    /// <param name="implementation">The implementation type of the injection.</param>
    /// <param name="lifetime">The service lifetime of the injection.</param>
    public InjectableAttribute(Type targetType = null, Type implementation = null, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        TargetType = targetType;
        Implementation = implementation;
        Lifetime = lifetime;
    }

    /// <summary>
    ///     Gets or sets the target type of the injection.
    /// </summary>
    public Type TargetType { get; set; }

    /// <summary>
    ///     Gets or sets the implementation type of the injection.
    /// </summary>
    public Type Implementation { get; set; }

    /// <summary>
    ///     Gets or sets the service lifetime of the injection.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; }

    /// <summary>
    ///     Gets or sets the sort order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    ///     Injection factory.
    /// </summary>
    /// <remarks>
    ///     The injection factory provided must implement <see cref="IInjectableFactory" />.
    ///     Optionally, <see cref="IInjectableFactory{TTarget}" /> can also be implemented to provide type-safety.
    ///     <see cref="AbstractInjectableFactory{TTarget}" /> and <see cref="AbstractInjectableFactory{TTarget, TImpl}" /> are
    ///     provided for convenience.
    /// </remarks>
    public Type Factory { get; set; }
}