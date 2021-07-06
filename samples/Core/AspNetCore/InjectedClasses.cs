using System;

namespace FrenziedMarmot.DependencyInjection.Samples.AspNetCore
{
    public interface IGreetingService
    {
        public string Greet();
    }

    [Injectable]
    public class InjectedAsSelf : IGreetingService
    {
        public string Greet()
        {
            return $"{GetType().Name} was injected!";
        }
    }

    [Injectable(typeof(IGreetingService))]
    public class InjectedAsInterface : InjectedAsSelf { }

    public class FactoryInjected : IGreetingService
    {
        public FactoryInjected(string greeting)
        {
            Greeting = greeting;
        }

        public string Greeting { get; }

        public string Greet()
        {
            return Greeting;
        }
    }

    [Injectable(Factory = typeof(SampleInjectionFactory))]
    public class SampleInjectionFactory : IInjectableFactory
    {
        public object Create(IServiceProvider serviceProvider)
        {
            return new FactoryInjected("I was injected via Factory!");
        }
    }
}