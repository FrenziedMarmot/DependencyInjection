using System;

namespace FrenziedMarmot.DependencyInjection.Test.AssemblyB
{
    [Injectable(Factory = typeof(ClassB3Factory))]
    public class ClassB3
    {
    }

    public class ClassB3Factory : IInjectableFactory
    {
        public object Create(IServiceProvider serviceProvider)
        {
            return new ClassB3();
        }
    }
}