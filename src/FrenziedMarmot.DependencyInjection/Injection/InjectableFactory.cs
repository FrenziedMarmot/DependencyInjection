namespace FrenziedMarmot.DependencyInjection;

/// <summary>
///     Abstract class for factory objects to implement when specified by `[Injectable(Factory = ...)]` providing type
///     safety.
/// </summary>
/// <remarks>
///     Reduces the boilerplate between <see cref="IInjectableFactory" /> and
///     <see cref="IInjectableFactory{TTarget}" />.
/// </remarks>
public abstract class InjectableFactory<TTarget> : IInjectableFactory<TTarget>
{
    /// <inheritdoc />
    public abstract TTarget Create(IServiceProvider serviceProvider);

    /// <inheritdoc />
    object IInjectableFactory.Create(IServiceProvider serviceProvider)
    {
        return Create(serviceProvider);
    }
}

/// <summary>
///     Abstract class for factory objects to implement when specified by `[Injectable(Factory = ...)]` providing type
///     safety.
/// </summary>
/// <remarks>
///     Reduces the boilerplate between <see cref="IInjectableFactory" /> and
///     <see cref="IInjectableFactory{TTarget}" />.
/// </remarks>
public abstract class InjectableFactory<TTarget, TImpl> : InjectableFactory<TImpl> where TImpl : TTarget { }