namespace FrenziedMarmot.DependencyInjection;

/// <summary>
///     Abstract class for factory objects to implement when specified by `[Injectable(Factory = ...)]` providing type
///     safety.
/// </summary>
/// <remarks>
///     Reduces the boilerplate between <see cref="IInjectableFactory" /> and
///     <see cref="IInjectableFactory{TTarget}" />.
/// </remarks>
public abstract class AbstractInjectableFactory<TTarget> : IInjectableFactory<TTarget>
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
public abstract class AbstractInjectableFactory<TTarget, TImpl> : AbstractInjectableFactory<TImpl> where TImpl : TTarget { }

/// <summary>
///     Interface for factory objects to implement when specified by `[Injectable(Factory = ...)]`.
/// </summary>
public interface IInjectableFactory<out TTarget> : IInjectableFactory
{
    /// <summary>
    ///     Creates the object this factory is intended to create.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A TTarget.</returns>
    new TTarget Create(IServiceProvider serviceProvider);
}

/// <summary>
///     Interface for factory objects to implement when specified by `[Injectable(Factory = ...)]`.
/// </summary>
public interface IInjectableFactory
{
    /// <summary>
    ///     Creates the object this factory is intended to create.
    /// </summary>
    /// <param name="serviceProvider">The IService provider.</param>
    /// <returns>The implementation to inject.</returns>
    object Create(IServiceProvider serviceProvider);
}