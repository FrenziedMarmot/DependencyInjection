using Microsoft.Extensions.DependencyInjection;

namespace FrenziedMarmot.DependencyInjection.Test.AssemblyA
{
    [Injectable(typeof(ClassA2), typeof(ClassA2), ServiceLifetime.Singleton)]
    [Injectable(typeof(IInterfaceA2), typeof(ClassA2), ServiceLifetime.Transient)]
    public class ClassA2 : IInterfaceA2 { }
}