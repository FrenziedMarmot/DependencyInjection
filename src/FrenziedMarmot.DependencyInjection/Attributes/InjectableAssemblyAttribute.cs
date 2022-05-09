namespace FrenziedMarmot.DependencyInjection;

/// <summary>
///     Attribute class for using in dependency injection. This is a quality-of-life attribute that can be used for
///     manually filtering through attributes within a collection or within an AppDomain. The provided extension method for
///     injecting via an AppDomain uses this attribute to filter by default.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class InjectableAssemblyAttribute : Attribute { }