using System;

namespace FrenziedMarmot.DependencyInjection
{
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
}