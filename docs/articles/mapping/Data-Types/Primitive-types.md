---
uid: Mapster.Mapping.DataTypes.Primitives
title: "Mapping - Primitive Types"
---

## Primitives

Converting between primitive types (ie. int, bool, double, decimal) is supported, including when those types are nullable. For all other types, if you can cast types in c#, you can also cast in Mapster.

```csharp
decimal i = 123.Adapt<decimal>(); //equal to (decimal)123;
```
