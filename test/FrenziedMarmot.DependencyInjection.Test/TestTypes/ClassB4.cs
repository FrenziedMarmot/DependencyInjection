using System;

namespace FrenziedMarmot.DependencyInjection.Test.AssemblyB
{
    [Injectable(typeof(InterfaceB4), Factory = typeof(ClassB4Factory))]
    public class ClassB4 : InterfaceB4
    {
        public int Test { get; set; }
    }

    public class ClassB4Factory : InjectableFactory<InterfaceB4, ClassB4>
    {
        public override ClassB4 Create(IServiceProvider serviceProvider)
        {
            return new ClassB4()
            {
                Test = 6,
            };
        }
    }

    public interface InterfaceB4
    {
        int Test { get; }
    }
}