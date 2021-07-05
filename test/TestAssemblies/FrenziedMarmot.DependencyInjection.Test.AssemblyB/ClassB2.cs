using Microsoft.Extensions.DependencyInjection;

namespace FrenziedMarmot.DependencyInjection.Test.AssemblyB
{
    [Injectable(typeof(ClassB2), typeof(ClassB2), ServiceLifetime.Singleton)]
    [Injectable(typeof(IInterfaceB2), typeof(ClassB2), ServiceLifetime.Transient)]
    public class ClassB2
    {
    }
}