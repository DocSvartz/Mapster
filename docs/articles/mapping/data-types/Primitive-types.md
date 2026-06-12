---
uid: Mapster.Mapping.DataTypes.Primitives
title: "Mapping - Primitive Types"
---

## Primitives

Converting between primitive types (ie. int, bool, double, decimal) is supported, including when those types are nullable. For all other types, if you can cast types in c#, you can also cast in Mapster.

```csharp
decimal i = 123.Adapt<decimal>(); //equal to (decimal)123;
```

## Enums

Mapster maps enums to numerics automatically, but it also maps strings to and from enums automatically in a fast manner.  
The default Enum.ToString() in .NET is quite slow. The implementation in Mapster is double the speed. Likewise, a fast conversion from strings to enums is also included.  If the string is null or empty, the enum will initialize to the first enum value.

In Mapster, flagged enums are also supported.

```csharp
var e = "Read, Write, Delete".Adapt<FileShare>();
//FileShare.Read | FileShare.Write | FileShare.Delete
```

For enum to enum with different type, by default, Mapster will map enum by value. You can override to map enum by name by:

```csharp
TypeAdapterConfig.GlobalSettings.Default
    .EnumMappingStrategy(EnumMappingStrategy.ByName);
```

## Strings

When Mapster maps other types to string, Mapster will use `ToString` method. And whenever Mapster maps string to the other types, Mapster will use `Parse` method.

```csharp
var s = 123.Adapt<string>(); //equal to 123.ToString();
var i = "123".Adapt<int>();  //equal to int.Parse("123");
```