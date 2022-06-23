using System;
using System.Reflection;
using Moq;

namespace FrenziedMarmot.DependencyInjection.Test
{
    public class InjectionTestAssembly : Assembly
    {
        public InjectionTestAssembly(bool isInjectable, params Type[] testTypes)
        {
            IsInjectable = isInjectable;
            TestTestTypes = testTypes;
        }

        private bool IsInjectable { get; }
        private Type[] TestTestTypes { get; }

        public override Type[] GetTypes()
        {
            return TestTestTypes;
        }

        public Type GetRepresentativeType()
        {
            Mock<Type> mockType = new Mock<Type>();
            mockType.SetupGet(e => e.Assembly).Returns(this);
            return mockType.Object;
        }

        public override object[] GetCustomAttributes(Type type, bool inherit)
        {
            return IsInjectable ? new[] { new InjectableAssemblyAttribute() } : null;
        }
    }
}