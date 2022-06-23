namespace FrenziedMarmot.DependencyInjection.Test.AssemblyC
{
    [InjectableOptions(nameof(TestOptions3), typeof(TestOptions3))]
    [InjectableOptions]
    public class TestOptions1
    {
        public int Number { get; set; }
    }

    [InjectableOptions(nameof(TestOptions2))]
    public class TestOptions2 : TestOptions1
    {
    }

    public class TestOptions3 : TestOptions1
    {
    }
}