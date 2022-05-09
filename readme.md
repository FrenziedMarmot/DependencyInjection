# FrenziedMarmot.DependencyInjection

The library, `FrenziedMarmot.DependencyInjection` provides the ability to map dependency injection via attributes.

Github: 
[![GitHub license](https://img.shields.io/github/license/FrenziedMarmot/DependencyInjection)](https://github.com/FrenziedMarmot/DependencyInjection/blob/main/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/FrenziedMarmot/DependencyInjection)](https://github.com/FrenziedMarmot/DependencyInjection/issues)
[![GitHub stars](https://img.shields.io/github/stars/FrenziedMarmot/DependencyInjection)](https://github.com/FrenziedMarmot/DependencyInjection/stargazers)
[![.NET](https://github.com/FrenziedMarmot/DependencyInjection/actions/workflows/dotnet.yml/badge.svg)](https://github.com/FrenziedMarmot/DependencyInjection/actions/workflows/dotnet.yml)

Sonar: 
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=FrenziedMarmot_DependencyInjection&metric=coverage)](https://sonarcloud.io/dashboard?id=FrenziedMarmot_DependencyInjection)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=FrenziedMarmot_DependencyInjection&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=FrenziedMarmot_DependencyInjection)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=FrenziedMarmot_DependencyInjection&metric=alert_status)](https://sonarcloud.io/dashboard?id=FrenziedMarmot_DependencyInjection)

Nuget: 
[![NuGet Status](https://img.shields.io/nuget/v/frenziedmarmot.dependencyinjection.svg?style=flat)](https://www.nuget.org/packages/FrenziedMarmot.DependencyInjection/)

## Purpose

By default, using dependency injection in C# requires that every time you have a new class to inject, you end up having to create it AND register it in the dependency injection system - usually in `Startup.cs`. Using this library, you modify your `Startup` once and then each time you create a class you decorate it with the `[Injectable]` attribute. This way, you define how something is injected with the class you're injecting. As long as the assembly that class is contained in is scanned, then it gets picked up and adding a class is one less step.

## Installation

Install via nuget package manager or use dotnet.

`dotnet add package FrenzedMarmot.DependencyInjection`

## Description

The library works by specifying injection via attributes AND finding those mappings by scanning for them.

### Step 1: Specifying Injection

Using the constructor on the `InjectableAttribute` you will be able to specify:

- What class/interface is being injected
- The service lifetime the injection lives for

and either:

- The implementation being injected
- The type acting as a factory for the injection

:warning: If you provide both the implementation and a factory, the **Factory** will take precedence.

In addition, you can also enable injection from your `appsettings.json` by specifying the class to hold the settings and instead using the `InjectableOptionsAttribute` and specifying:

 - The json path, using `:` to indicate nesting. If not specified, will use the class name.
 - The type to deserialize to.  If not specified, uses the decorated class.

### Step 2: Scanning for Injection Specified by Attributes  

There are 2 extension methods which provide the functionality for this library:

```cs
    services.ScanForAttributeInjection(GetType().Assembly)
            .ScanForOptionAttributeInjection(Configuration, GetType().Assembly);
```

In Startup.cs when you're configuring dependency injection, utilize the extension method `ScanForAttributeInjection` and supply a list of assemblies or types for it to scan. The method utilizes the `params` keyword so multiple types/assemblies can be provided.

```cs
    services.ScanForAttributeInjection(GetType().Assembly);
```

Optionally, there's a method that will take an entire appdomain. However, note that it will require marking up the assemblies that you want it to actually scan with the `InjectableAssembly` attribute. A good location to put this is usually `AssemblyInfo.cs` which is common, however, anywhere in the assembly code will work.

```cs
    services.ScanForAttributeInjection(AppDomain.CurrentDomain);
```

For scanning for `IOptions<T>` to provide a type from appsettings, you need to additionally call `ScanForOptionAttributeInjection` and provide an `IConfiguration` object.

```cs
    services.ScanForOptionAttributeInjection(Configuration, GetType().Assembly);
```

:warning: These injection scanning options are independent. They do not rely on each other. You can use either or both. The important thing is to ensure that if you're using either of the attributes that you also scan for them or the attribute will be useless.

---
## Example Attribute Usage

The `[Injectable]` attribute is for injecting services and allows for a fairly flexible method of specifying your injections declaratively.

Specifying a concrete implementation as an interface, the following is the equivalent of coding `services.AddScoped<IGreetingService, GreetingService>()`

```cs
    public interface IGreetingService
    {
        public string Greet();
    }

    [Injectable(typeof(IGreetingService), typeof(GreetingService), ServiceLifetime.Scoped)]
    public class GreetingService : IGreetingService
    {
        public string Greet()
        {
            return $"{GetType().Name} was injected!";
        }
    }
```

If any parameter is left off, it uses the class it's attached to as an argument and `ServiceLifetime.Scoped` is the default service lifetime.

For example, the attribute usage above for `GreetingService` could be simplified to:

```cs
    [Injectable(typeof(IGreetingService))]
    public class GreetingService : IGreetingService
```

For example, the following is the same as `services.AddScoped<MyClass>()`:

```cs
    [Injectable]
    public class MyClass
```

Property initializers can be used to set only a specific property. For example, the following is the same as `services.AddSingleton<MyClass>()`:

```cs
    [Injectable(Lifetime = ServiceLifetime.Singleton)]
    public class MyClass
```

The `Factory` property can be used to specify a factory class that will be called for the implementation. The factory class provided must implement `IInjectableFactory`. For example, a factory attached to an injectable would look like:


```cs
    [Injectable(Factory = typeof(MyClassFactory))]
    public class MyClass
    {
        public MyClass(string someString)
        {
            //...
        }
    }

    public class MyClassFactory : IInjectableFactory
    {
        public object Create(IServiceProvider serviceProvider)
        {
            return new MyClass("I was injected via Factory!");
        }
    }
```

Type-safety through the factory is also provided in `1.0.2` via `IInjectableFactory<T>`. This also implements `IInjectableFactory` so to reduce boilerplace `AbstractInjectableFactor<TTarget>` and `AbstractInjectableFactory<TTarget, TImpl>` are also provided and encouraged.


```cs
    public class MyClassFactory : AbstractInjectableFactory<MyClass>
    {
        public MyClass Create(IServiceProvider serviceProvider)
        {
            return new MyClass("I was injected via Factory!");
        }
    }

    public class AnotherFactory : AbstractInjectableFactory<IAnother, AnotherClass>
    {
        public AnotherClass Create(IServiceProvider serviceProvider)
        {
            return new AnotherClass("I was injected via Factory!");
        }
    }
```

Note: If you specify the same factory type twice, it will NOT inject the same instance to both. It will create 2 instances.

---
## Example Option Attribute Usage

The `[InjectableOptions]` attribute is for `IOption<T>` objects useful for injecting from your `appsettings.json` file.

For example, the class you would provide for 
```cs
    [InjectableOptions("My:Injected:Options")]
    public class InjectedOptions
    {
        public string SomeValue { get; set; }
        //...
    }
```

Will deserialize:

```json
  "My": {
    "Injected": {
      "Options": {
          "SomeValue": "The value"
          //...
      }
    }
  }
```
And you can inject it into your class:
```cs
        public IndexModel(IOptions<InjectedOptions> opts)
        {
            DoSomething(opts.Value.SomeValue);
        }
```

You can optionally pass a second argument if the decorated type isn't the one you intend to inject. This is useful if, for example, the type you're intending to inject is in a different assembly:

```cs

    [InjectableOptions(Implementation = typeof(SomeApiOptions))]
    [InjectableOptions("OtherApi", typeof(SomeOtherApiOptions))]
    public class Startup
```

---
If you want to fuel our continued work:

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/frenziedmarmot)
