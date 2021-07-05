namespace FrenziedMarmot.DependencyInjection.Test.AssemblyA
{
    [Injectable]
    [Injectable(typeof(IInterfaceA1))]
    public class ClassA1 : IInterfaceA1
    {
    }
}