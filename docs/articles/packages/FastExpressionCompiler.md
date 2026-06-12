---
uid: Mapster.Packages.FastExpressionCompiler
title: "Packages - Fast Expression Compiler Support"
---

Need more speed? Let's compile with [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler).

## Installation

Getting the package:

```nuget
     PM> Install-Package FastExpressionCompiler
```

Then add following code on start up

```csharp
TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
```

That's it. Now your code will enjoy performance boost. Here is a current benchmark snapshot:

| Method | MapOperations | Mean | StdDev | Error | Ratio | Gen0 | Gen1 | Allocated | Alloc Ratio |
| -------- | -------------- | -----: | -------: | ------: | ------: | -----: | -----: | ----------: | ----------: |
| `Mapster 10.0.7` | 1000000 | 412,534 us | 2,704 us | 4,543 us | 1.00 | 77000 | - | 1243.59 MB | 1.00 |
| `Mapster 10.0.7 (FEC)` | 1000000 | 124,374 us | 1,290 us | 2,466 us | 0.30 | 74000 | - | 1182.56 MB | 0.95 |

See the [benchmark snapshot in README](../../../README.md#performance--memory-efficient) for the full comparison.
