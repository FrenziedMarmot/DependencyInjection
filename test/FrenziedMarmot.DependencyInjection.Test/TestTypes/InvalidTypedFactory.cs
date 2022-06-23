using System;

namespace FrenziedMarmot.DependencyInjection.Test.TestTypes
{
    [Injectable(Factory = typeof(TypedImplFactory))]
    internal interface ITypedFactoryTarget { }

    [Injectable(Factory = typeof(TypedImplFactory))]
    internal interface IInvalidTypedFactoryTarget { }

    internal class TypedImplClass : ITypedFactoryTarget { }

    internal class TypedImplFactory : InjectableFactory<ITypedFactoryTarget, TypedImplClass>
    {
        public override TypedImplClass Create(IServiceProvider serviceProvider)
        {
            return new TypedImplClass();
        }
    }
}