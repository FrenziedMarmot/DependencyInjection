using System;
using System.IO;
using System.Reflection;
using System.Text;
using FrenziedMarmot.DependencyInjection.Test.AssemblyA;
using FrenziedMarmot.DependencyInjection.Test.AssemblyB;
using FrenziedMarmot.DependencyInjection.Test.AssemblyC;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace FrenziedMarmot.DependencyInjection.Test
{
    [TestClass]
    public class OptionAttributeInjectionTests
    {
        private IServiceCollection Services { get; set; }

        private IConfiguration Config { get; set; }
        private InjectionTestAssembly AssemblyA { get; set; }
        private InjectionTestAssembly AssemblyB { get; set; }
        private InjectionTestAssembly AssemblyC { get; set; }

        [TestInitialize]
        public void Setup()
        {
            var appSettings = JsonConvert.SerializeObject(new
            {
                TestOptions1 = new TestOptions1 {Number = 1},
                TestOptions2 = new TestOptions2 {Number = 2},
                TestOptions3 = new TestOptions3 {Number = 3},
            });

            ConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));

            Config = builder.Build();
            Services = new ServiceCollection();

            AssemblyA = new InjectionTestAssembly(true, typeof(ClassA1), typeof(ClassA2), typeof(IInterfaceA1), typeof(IInterfaceA2));
            AssemblyB = new InjectionTestAssembly(false, typeof(ClassB1), typeof(ClassB2), typeof(ClassB3), typeof(ClassB4), typeof(IInterfaceB1), typeof(IInterfaceB2));
            AssemblyC = new InjectionTestAssembly(false, typeof(ClassC1), typeof(TestOptions1), typeof(TestOptions2), typeof(TestOptions3));
        }

        [TestMethod]
        public void TestRepresentativeType()
        {
            Services.ScanForOptionAttributeInjection(Config, AssemblyC.GetRepresentativeType());
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertOptions<TestOptions1>(provider, 1);
            AssertOptions<TestOptions2>(provider, 2);
            AssertOptions<TestOptions3>(provider, 3);
        }

        [TestMethod]
        public void TestAssembly()
        {
            Services.ScanForOptionAttributeInjection(Config, AssemblyC);
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertOptions<TestOptions1>(provider, 1);
            AssertOptions<TestOptions2>(provider, 2);
            AssertOptions<TestOptions3>(provider, 3);
        }

        [TestMethod]
        public void AppScanner_NoFilter()
        {
            Mock<IInjectableAssemblyProvider> scanner = new Mock<IInjectableAssemblyProvider>();
            scanner.Setup(e => e.GetAssemblies()).Returns(new[]
            {
                AssemblyA,
                AssemblyB,
                AssemblyC,
            });

            Services.ScanForOptionAttributeInjection(Config, scanner.Object, false);
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertOptions<TestOptions1>(provider, 1);
            AssertOptions<TestOptions2>(provider, 2);
            AssertOptions<TestOptions3>(provider, 3);
        }

        [TestMethod]
        public void AppScanner_Filter()
        {
            Mock<IInjectableAssemblyProvider> scanner = new Mock<IInjectableAssemblyProvider>();
            scanner.Setup(e => e.GetAssemblies()).Returns(new[]
            {
                AssemblyA,
                AssemblyB,
                AssemblyC,
            });

            Services.ScanForOptionAttributeInjection(Config, scanner.Object);
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertNoOptions<TestOptions1>(provider);
            AssertNoOptions<TestOptions2>(provider);
            AssertNoOptions<TestOptions3>(provider);
        }

        private static void AssertOptions<T>(IServiceProvider provider, int expectedNumber) where T : TestOptions1
        {
            T opts = provider.GetService<IOptions<T>>()?.Value;
            Assert.IsNotNull(opts);
            Assert.AreEqual(expectedNumber, opts.Number);
        }

        private static void AssertNoOptions<T>(IServiceProvider provider) where T : TestOptions1
        {
            IOptions<T> opts = provider.GetService<IOptions<T>>();
            Assert.IsNotNull(opts);
            Assert.IsNotNull(opts.Value);
            Assert.AreEqual(0, opts.Value.Number);
        }
    }
}