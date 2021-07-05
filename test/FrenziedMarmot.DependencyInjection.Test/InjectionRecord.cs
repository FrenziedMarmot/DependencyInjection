using System;

namespace FrenziedMarmot.DependencyInjection.Test
{
    public class InjectionRecord
    {
        public InjectionRecord(Type target, Type implementation, Func<IServiceProvider, object> factory)
        {
            Target = target;
            Implementation = implementation;
            Factory = factory;
        }

        public Type Target { get; }
        public Type Implementation { get; }
        public Func<IServiceProvider, object> Factory { get; }
    }
}