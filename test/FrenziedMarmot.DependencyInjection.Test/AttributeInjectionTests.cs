using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using FrenziedMarmot.DependencyInjection.Test.AssemblyA;
using FrenziedMarmot.DependencyInjection.Test.AssemblyB;
using FrenziedMarmot.DependencyInjection.Test.AssemblyC;
using FrenziedMarmot.DependencyInjection.Test.TestTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AppDomain = System.AppDomain;

namespace FrenziedMarmot.DependencyInjection.Test
{
    [TestClass]
    public class AttributeInjectionTests
    {
        private Mock<IServiceCollection> Services { get; set; }

        private Dictionary<ServiceLifetime, List<InjectionRecord>> Injections { get; set; }
        private InjectionTestAssembly AssemblyA { get; set; }
        private InjectionTestAssembly AssemblyB { get; set; }
        private InjectionTestAssembly AssemblyC { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Injections = new Dictionary<ServiceLifetime, List<InjectionRecord>>
            {
                { ServiceLifetime.Singleton, new List<InjectionRecord>() },
                { ServiceLifetime.Scoped, new List<InjectionRecord>() },
                { ServiceLifetime.Transient, new List<InjectionRecord>() },
            };
            Services = new Mock<IServiceCollection>();
            Services.Setup(e => e.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor descriptor) =>
                {
                    InjectionRecord record = new InjectionRecord(descriptor.ServiceType, descriptor.ImplementationType,
                        descriptor.ImplementationFactory);
                    Injections[descriptor.Lifetime].Add(record);
                });

            AssemblyA = new InjectionTestAssembly(true, typeof(ClassA1), typeof(ClassA2), typeof(IInterfaceA1), typeof(IInterfaceA2));
            AssemblyB = new InjectionTestAssembly(false, typeof(ClassB1), typeof(ClassB2), typeof(ClassB3), typeof(ClassB4), typeof(IInterfaceB1), typeof(IInterfaceB2));
            AssemblyC = new InjectionTestAssembly(false, typeof(ClassC1));
        }

        [TestMethod]
        public void AssemblyA_TestRepresentativeType_Single()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA.GetRepresentativeType());
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AssemblyA_TestRepresentativeType_Multiple()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA.GetRepresentativeType(), AssemblyA.GetRepresentativeType());
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AppScanner_NoFilter()
        {
            Mock<IInjectableAssemblyProvider> scanner = new Mock<IInjectableAssemblyProvider>();
            scanner.Setup(e => e.GetAssemblies()).Returns(new[]
            {
                AssemblyA,
                AssemblyB,
            });

            Services.Object.ScanForAttributeInjection(scanner.Object, false);
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void AppScanner_Filter()
        {
            Mock<IInjectableAssemblyProvider> scanner = new Mock<IInjectableAssemblyProvider>();
            scanner.Setup(e => e.GetAssemblies()).Returns(new[]
            {
                AssemblyA,
                AssemblyB,
            });

            Services.Object.ScanForAttributeInjection(scanner.Object, true);
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AssemblyA_TestAssemblies_Single()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA);
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void AssemblyA_TestAssemblies_Multiple()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA, AssemblyA);
            AssertAssemblyAInjections();
        }

        [TestMethod]
        public void Multiple_TestRepresentativeType_Distinct()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA.GetRepresentativeType(), AssemblyB.GetRepresentativeType());
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void MultipleAssemblies_TestRepresentativeType_Dupes()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA.GetRepresentativeType(), AssemblyB.GetRepresentativeType(), AssemblyA.GetRepresentativeType(), AssemblyB.GetRepresentativeType());
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void MultipleAssemblies_TestAssemblies_Distinct()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA, AssemblyB);
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void MultipleAssemblies_TestAssemblies_Dupes()
        {
            Services.Object.ScanForAttributeInjection(AssemblyA, AssemblyA, AssemblyB, AssemblyB);
            AssertMultiAssemblyInjections();
        }

        [TestMethod]
        public void Factory_Invalid()
        {
            Assert.ThrowsException<ArgumentException>(
                () => Services.Object.ScanForAttributeInjection(AssemblyC));
        }

        [TestMethod]
        public void Factory_Typed()
        {
            InjectionTestAssembly testAssembly = new InjectionTestAssembly(true, new[] { typeof(ITypedFactoryTarget) });
            Services.Object.ScanForAttributeInjection(testAssembly);
            Assert.AreEqual(1, Injections[ServiceLifetime.Scoped].Count);

            InjectionRecord record = Injections[ServiceLifetime.Scoped].First();
            Assert.IsNotNull(record);
            Assert.AreEqual(typeof(ITypedFactoryTarget), record.Target);
            var factory = record.Factory(null);
            Assert.IsNotNull(factory);
            Assert.IsInstanceOfType(factory, typeof(ITypedFactoryTarget));
            Assert.IsInstanceOfType(factory, typeof(TypedImplClass));
        }

        [TestMethod]
        public void Factory_Typed_Invalid()
        {
            InjectionTestAssembly testAssembly = new InjectionTestAssembly(true, new[] { typeof(IInvalidTypedFactoryTarget) });
            Assert.ThrowsException<ArgumentException>(
                () => Services.Object.ScanForAttributeInjection(testAssembly));
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

            Assert.AreEqual(6, Injections[ServiceLifetime.Scoped].Count);
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(IInterfaceA1), typeof(ClassA1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(ClassA1), typeof(ClassA1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(IInterfaceB1), typeof(ClassB1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(ClassB1), typeof(ClassB1)));
            Assert.AreEqual(1, CountInjections(ServiceLifetime.Scoped, typeof(ClassB3), null));

            var factoryCreated = Injections[ServiceLifetime.Scoped].Where(e => e.Factory != null).ToList();
            Assert.AreEqual(2, factoryCreated.Count);

            InjectionRecord b3Factory = factoryCreated.FirstOrDefault(e => e.Target.IsAssignableFrom(typeof(ClassB3)));
            Assert.IsNotNull(b3Factory);
            Assert.AreEqual(typeof(ClassB3), b3Factory.Target);
            var b3FactoryResult = b3Factory.Factory(null);
            Assert.IsNotNull(b3FactoryResult);
            Assert.IsInstanceOfType(b3FactoryResult, typeof(ClassB3));


            InjectionRecord b4Factory = factoryCreated.FirstOrDefault(e => e.Target.IsAssignableFrom(typeof(InterfaceB4)));
            Assert.IsNotNull(b4Factory);
            Assert.AreEqual(typeof(InterfaceB4), b4Factory.Target);
            var b4FactoryFactory = b4Factory.Factory(null);
            Assert.IsNotNull(b4FactoryFactory);
            Assert.IsInstanceOfType(b4FactoryFactory, typeof(ClassB4));
        }
    }
}