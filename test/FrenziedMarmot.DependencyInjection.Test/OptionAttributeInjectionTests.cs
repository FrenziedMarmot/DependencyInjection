using System;
using System.IO;
using System.Reflection;
using System.Text;
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

        [TestInitialize]
        public void Setup()
        {
            var appSettings = JsonConvert.SerializeObject(new
            {
                TestOptions = new TestOptions {Number = 1},
                TestOptions2 = new TestOptions2 {Number = 2},
                TestOptions3 = new TestOptions3 {Number = 3},
            });

            ConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));

            Config = builder.Build();
            Services = new ServiceCollection();
        }

        [TestMethod]
        public void TestRepresentativeType()
        {
            Services.ScanForOptionAttributeInjection(Config, typeof(ClassC1));
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertOptions<TestOptions>(provider, 1);
            AssertOptions<TestOptions2>(provider, 2);
            AssertOptions<TestOptions3>(provider, 3);
        }

        [TestMethod]
        public void TestAssembly()
        {
            Services.ScanForOptionAttributeInjection(Config, typeof(ClassC1).Assembly);
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertOptions<TestOptions>(provider, 1);
            AssertOptions<TestOptions2>(provider, 2);
            AssertOptions<TestOptions3>(provider, 3);
        }

        [TestMethod]
        public void AppScanner_NoFilter()
        {
            Mock<IAssemblyProvider> scanner = new Mock<IAssemblyProvider>();
            scanner.Setup(e => e.GetAssemblies()).Returns(new[]
            {
                Assembly.Load("FrenziedMarmot.DependencyInjection.Test.AssemblyA"),
                Assembly.Load("FrenziedMarmot.DependencyInjection.Test.AssemblyB"),
                Assembly.Load("FrenziedMarmot.DependencyInjection.Test.AssemblyC"),
            });

            Services.ScanForOptionAttributeInjection(Config, scanner.Object, false);
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertOptions<TestOptions>(provider, 1);
            AssertOptions<TestOptions2>(provider, 2);
            AssertOptions<TestOptions3>(provider, 3);
        }

        [TestMethod]
        public void AppScanner_Filter()
        {
            Mock<IAssemblyProvider> scanner = new Mock<IAssemblyProvider>();
            scanner.Setup(e => e.GetAssemblies()).Returns(new[]
            {
                Assembly.Load("FrenziedMarmot.DependencyInjection.Test.AssemblyA"),
                Assembly.Load("FrenziedMarmot.DependencyInjection.Test.AssemblyB"),
                Assembly.Load("FrenziedMarmot.DependencyInjection.Test.AssemblyC"),
            });

            Services.ScanForOptionAttributeInjection(Config, scanner.Object);
            ServiceProvider provider = Services.BuildServiceProvider();
            AssertNoOptions<TestOptions>(provider);
            AssertNoOptions<TestOptions2>(provider);
            AssertNoOptions<TestOptions3>(provider);
        }

        private static void AssertOptions<T>(IServiceProvider provider, int expectedNumber) where T : TestOptions
        {
            T opts = provider.GetService<IOptions<T>>()?.Value;
            Assert.IsNotNull(opts);
            Assert.AreEqual(expectedNumber, opts.Number);
        }

        private static void AssertNoOptions<T>(IServiceProvider provider) where T : TestOptions
        {
            IOptions<T> opts = provider.GetService<IOptions<T>>();
            Assert.IsNotNull(opts);
            Assert.IsNotNull(opts.Value);
            Assert.AreEqual(0, opts.Value.Number);
        }
    }
}