namespace FrenziedMarmot.DependencyInjection.Test.AssemblyB
{
    [Injectable]
    [Injectable(typeof(IInterfaceB1))]
    public class ClassB1 : IInterfaceB1 { }
}