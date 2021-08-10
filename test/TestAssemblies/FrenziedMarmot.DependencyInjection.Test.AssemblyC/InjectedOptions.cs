namespace FrenziedMarmot.DependencyInjection.Test.AssemblyC
{
    [InjectableOptions("TestOptions3", typeof(TestOptions3))]
    [InjectableOptions]
    public class TestOptions
    {
        public int Number { get; set; }
    }

    [InjectableOptions("TestOptions2")]
    public class TestOptions2 : TestOptions
    {
    }

    public class TestOptions3 : TestOptions
    {
    }
}