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

### Step 2: Scanning for Injection Specified by Attributes

In Startup.cs when you're configuring dependency injection, utilize the extension method `ScanForAttributeInjection` supplying a list of assemblies for it to scan. The method utilizes the `params` keyword so multiple assemblies can be provided.

```cs
    services.ScanForAttributeInjection(GetType().Assembly);
```

## Example Attribute Usage

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

Note: If you specify the same factory type twice, it will NOT inject the same instance to both. It will create 2 instances.

---
If you want to fuel our continued work:

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/frenziedmarmot)
