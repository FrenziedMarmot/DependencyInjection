namespace FrenziedMarmot.DependencyInjection.Samples.AspNetCore
{
    [InjectableOptions("My:Test:Options")]
    public class InjectedOptions
    {
        public string Value1 { get; set; }
        public int Value3 { get; set; }
        public string Value5 { get; set; }
    }
}
