---
uid: Mapster.Configuration.Overview
title: "Configuration - Overview"
---
<!-- TODO: Check if this is really Configuration or Settings -->

## Setting per type pair

You can easily create settings for a type mapping by using: `TypeAdapterConfig<TSource, TDestination>.NewConfig()`.
When `NewConfig` is called, any previous configuration for this particular TSource => TDestination mapping is dropped.

```csharp
TypeAdapterConfig<TSource, TDestination>
    .NewConfig()
    .Ignore(dest => dest.Age)
    .Map(dest => dest.FullName,
          src => string.Format("{0} {1}", src.FirstName, src.LastName));
```

As an alternative to `NewConfig`, you can use `ForType` in the same way:

```csharp
TypeAdapterConfig<TSource, TDestination>
    .ForType()
    .Ignore(dest => dest.Age)
    .Map(dest => dest.FullName,
         src => string.Format("{0} {1}", src.FirstName, src.LastName));
```

`ForType` differs in that it will create a new mapping if one doesn't exist, but if the specified TSource => TDestination
mapping does already exist, it will enhance the existing mapping instead of dropping and replacing it.  

## Global setting

Use global settings to apply policies to all mappings.

```csharp
TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
```

Then for individual type mappings, you can easily override the global setting(s).

```csharp
TypeAdapterConfig<SimplePoco, SimpleDto>.NewConfig().PreserveReference(false);
```

## Rule based settings

To set the setting at a more granular level. You can use the `When` method in global settings.
In the example below, when any source type and destination type are the same, we will not the copy the `Id` property.

```csharp
TypeAdapterConfig.GlobalSettings.When((srcType, destType, mapType) => srcType == destType)
                               .Ignore("Id");
```

In this example, the config would only apply to Query Expressions (projections).

```csharp
TypeAdapterConfig.GlobalSettings.When((srcType, destType, mapType) => mapType == MapType.Projection)
                                .IgnoreAttribute(typeof(NotMapAttribute));
```

## Destination type only

A setting can also be created without knowing the source type, by using `ForDestinationType`. For example, you can do `AfterMapping` setting to validate after mapping.

```csharp
TypeAdapterConfig.GlobalSettings.ForDestinationType<IValidator>()
                                .AfterMapping(dest => dest.Validate());
```

>[!NOTE] 
>`ForDestinationType` above will always apply to all types assignable to `IValidator`. If destination class implements `IValidator`, it will also apply the `AfterMapping` config.

## Open generics

If the mapping type is generic, you can create a setting by passing generic type definition to `ForType`.

```csharp
TypeAdapterConfig.GlobalSettings.ForType(typeof(GenericPoco<>), typeof(GenericDto<>))
                                .Map("value", "Value");
```
>[!Note]
> Starting with Mapster version 10.0.8, [validation](xref:Mapster.Configuration.ValidationAndCompilation#validating-mappings) for type pairs settings containing open generic type is disabled.
>If you require validation of such settings, you must register the configuration for at least one pair of closed generic types:
>```csharp
>TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true; // check all destination member mapping
>TypeAdapterConfig.GlobalSettings.ForType(typeof(GenericPoco<>), typeof(GenericDto<>))
>                                .Map("value", "Value");
>
> TypeAdapterConfig<GenericPoco<int>, GenericDto<string>>.NewConfig();
> TypeAdapterConfig.GlobalSettings.Compile(); // validate
>```