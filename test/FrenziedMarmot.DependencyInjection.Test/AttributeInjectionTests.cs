using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FrenziedMarmot.DependencyInjection.Test.AssemblyA;
using FrenziedMarmot.DependencyInjection.Test.AssemblyB;
using FrenziedMarmot.DependencyInjection.Test.AssemblyC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FrenziedMarmot.DependencyInjection.Test
{
    [TestClass]
    public class AttributeInjectionTests
    {
        private Mock<IServiceCollection> Services { get; set; }

        private Dictionary<ServiceLifetime, List<InjectionRecord>> Injections { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Injections = new Dictionary<ServiceLifetime, List<InjectionRecord>>
            {
                {ServiceLifetime.Singleton, new List<InjectionRecord>()},
                {ServiceLifetime.Scoped, new List<InjectionRecord>()},
                {ServiceLifetime.Transient, new List<InjectionRecord>()}
            };
            Services = new Mock<IServiceCollection>();
            Services.Setup(e => e.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor descriptor) =>
                {
                    InjectionRecord record = new InjectionRecord(descriptor.ServiceType, descriptor.ImplementationType,
                        descriptor.ImplementationFactory);
                    Injections[descriptor.Lifetime].Add(record);
                });
        }

        [TestMethod]
        public void AssemblyA_TestRepresentativeType_Single()
        {
            Services.Object.ScanForAttributeInjection(typeof(ClassA1));
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AssemblyA_TestRepresentativeType_Multiple()
        {
            Services.Object.ScanForAttributeInjection(typeof(ClassA1), typeof(ClassA2));
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AssemblyA_TestAssemblies_Single()
        {
            Services.Object.ScanForAttributeInjection(Assembly.LoadFrom("FrenziedMarmot.DependencyInjection.Test.AssemblyA.dll"));
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AssemblyA_TestAssemblies_Multiple()
        {
            Services.Object.ScanForAttributeInjection(Assembly.LoadFrom("FrenziedMarmot.DependencyInjection.Test.AssemblyA.dll"),
                Assembly.GetAssembly(typeof(ClassA1)));
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void Multiple_TestRepresentativeType_Distinct()
        {
            Services.Object.ScanForAttributeInjection(typeof(ClassA1), typeof(ClassB2));
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void MultipleAssemblies_TestRepresentativeType_Dupes()
        {
            Services.Object.ScanForAttributeInjection(typeof(ClassA1), typeof(ClassA2), typeof(ClassB1), typeof(ClassB2));
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void MultipleAssemblies_TestAssemblies_Distinct()
        {
            Services.Object.ScanForAttributeInjection(Assembly.LoadFrom("FrenziedMarmot.DependencyInjection.Test.AssemblyA.dll"),
                Assembly.GetAssembly(typeof(ClassB1)));
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void MultipleAssemblies_TestAssemblies_Dupes()
        {
            Services.Object.ScanForAttributeInjection(Assembly.LoadFrom("FrenziedMarmot.DependencyInjection.Test.AssemblyB.dll"),
                Assembly.GetAssembly(typeof(ClassA1)), Assembly.GetAssembly(typeof(ClassB1)), Assembly.GetAssembly(typeof(ClassB2)));
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void InvalidFactory()
        {
            Assert.ThrowsException<ArgumentException>(
                () => Services.Object.ScanForAttributeInjection(Assembly.GetAssembly(typeof(ClassC1))));
        }

        protected int CountInjections(ServiceLifetime lifetime, Type target, Type impl)
        {
            return Injections[lifetime].Count(e => e.Target == target && e.Implementation == impl);
        }

        protected void AssertAssemblyAInjections()
        {
            Assert.AreEqual(1, Injections[ServiceLifetime.Singleton].Count);
            Assert.AreEqual(1, Injections[ServiceLifetime.Transient].Count);
            Assert.AreEqual(2, Injections[ServiceLifetime.Scoped].Count);

            Assert.AreEqual(typeof(ClassA2), Injections[ServiceLifetime.Singleton][0].Target);
            Assert.AreEqual(typeof(ClassA2), Injections[ServiceLifetime.Singleton][0].Implementation);
            Assert.AreEqual(typeof(IInterfaceA2), Injections[ServiceLifetime.Transient][0].Target);
            Assert.AreEqual(typeof(ClassA2), Injections[ServiceLifetime.Transient][0].Implementation);

            Assert.IsTrue(Injections[ServiceLifetime.Scoped].All(e => e.Implementation == typeof(ClassA1)));
            Assert.IsTrue(Injections[ServiceLifetime.Scoped].Any(e => e.Target == typeof(ClassA1)));
            Assert.IsTrue(Injections[ServiceLifetime.Scoped].Any(e => e.Target == typeof(IInterfaceA1)));
        }

        protected void AssertMultiAssemblyInjections()
        {
            Assert.AreEqual(2, Injections[ServiceLifetime.Singleton].Count);
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Singleton, typeof(ClassA2), typeof(ClassA2)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Singleton, typeof(ClassB2), typeof(ClassB2)));

            Assert.AreEqual(2, Injections[ServiceLifetime.Transient].Count);
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Transient, typeof(IInterfaceA2), typeof(ClassA2)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Transient, typeof(IInterfaceB2), typeof(ClassB2)));

            Assert.AreEqual(5, Injections[ServiceLifetime.Scoped].Count);
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(IInterfaceA1), typeof(ClassA1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(ClassA1), typeof(ClassA1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(IInterfaceB1), typeof(ClassB1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(ClassB1), typeof(ClassB1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(ClassB3), null));

            var factoryCreated = Injections[ServiceLifetime.Scoped].Where(e => e.Factory != null).ToList();
            Assert.AreEqual(1, factoryCreated.Count);
            InjectionRecord expectedFactory = factoryCreated.Single();
            Assert.AreEqual(typeof(ClassB3), expectedFactory.Target);
            var factoryResult = expectedFactory.Factory(null);
            Assert.IsNotNull(factoryResult);
            Assert.IsInstanceOfType(factoryResult, typeof(ClassB3));
        }
    }
}