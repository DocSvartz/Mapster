---
uid: Mapster.Benchmarks
title: "Benchmark results"
---

Mapster includes a benchmark project in [`src/Benchmark`](https://github.com/MapsterMapper/Mapster/tree/master/src/Benchmark) that compares the local Mapster build with AutoMapper, Facet, and Mapperly across several object shapes.

This page is a May 2026 snapshot of those benchmarks. Treat the numbers as a comparison point for this environment and benchmark configuration rather than an absolute guarantee for every machine or application.

## How to read the tables

- `Mean` is the total time for one BenchmarkDotNet benchmark invocation. Each invocation runs a manual hot loop with `MapOperations = 1,000,000`.
- `Ns/Map` normalizes `Mean` to the approximate cost of one logical mapping call.
- `Allocated` is the memory allocated by one benchmark invocation.
- `Bytes/Map` normalizes `Allocated` to one logical mapping call.
- `Ratio` and `Alloc Ratio` are relative to the default Mapster benchmark in the same scenario.
- `TotalAllTypes` runs three mapping scenarios in one benchmark invocation, so its `Ns/Map` and `Bytes/Map` values are divided by `MapOperations * 3`.

## Benchmark scenarios

### FlatTypes

`FlatTypes` maps `Person -> PersonDTO`. It is a flat DTO shape: simple property-to-property copy, no nested objects, and no collections. This scenario mostly highlights mapper call overhead, generated IL quality, and allocation rate for a best-case DTO.

### ComplexTypes

`ComplexTypes` maps `Customer -> CustomerDTO`. It includes nested address mapping, array/list shape changes, and a flattening rule (`AddressCity <- Address.City`). This scenario is useful for typical DTOs that combine nested objects, collections, and a small amount of custom member mapping.

### RecursiveTypes

`RecursiveTypes` maps `Foo -> FooDTO`. The type shape is self-recursive: a `Foo` can contain another `Foo`, an enumerable of `Foo`, and an array of `Foo`. The sample data does not intentionally create a back-reference cycle, but the mapping graph is deeper and allocates more nested DTOs than the flat or customer scenarios.

### TotalAllTypes

`TotalAllTypes` runs `FlatTypes`, `RecursiveTypes`, and `ComplexTypes` sequentially in one benchmark method. `Mean` is therefore the total batch time for all three scenarios; use `Ns/Map` and `Bytes/Map` for normalized per-map interpretation.

## Compared methods

- `Mapster` uses the default Mapster expression compiler.
- `Mapster (Roslyn)` uses the Roslyn/debug-info compiler path.
- `Mapster (FEC)` uses FastExpressionCompiler.
- `Mapster (Codegen)` uses generated mapping code.
- `AutoMapper`, `Facet`, and `Mapperly` are included as external comparison points.
- `Facet (Compiled Projection)` uses Facet's compiled projection path, which can behave very differently from the constructor path depending on the object shape.

## Results

### FlatTypes

| Scenario  | Method                              | MapOperations | Mean      | StdDev    | Error     | Ns/Map | Ratio | Gen0 | Allocated | Alloc Ratio | Bytes/Map |
|---------- |------------------------------------ |-------------- |----------:|----------:|----------:|-------:|------:|-----:|----------:|------------:|----------:|
| FlatTypes | `Mapster 10.0.8`                    | 1000000       |  6.849 ms | 0.6851 ms | 1.0358 ms |  6.849 |  1.01 | 4781 |  76.29 MB |        1.00 |        80 |
| FlatTypes | `Mapster 10.0.8 (Roslyn)`           | 1000000       |  6.579 ms | 0.2782 ms | 0.4206 ms |  6.579 |  0.97 | 4781 |  76.29 MB |        1.00 |        80 |
| FlatTypes | `Mapster 10.0.8 (FEC)`              | 1000000       |  6.549 ms | 0.9130 ms | 1.3803 ms |  6.549 |  0.97 | 4781 |  76.29 MB |        1.00 |        80 |
| FlatTypes | `Mapster 10.0.8 (Codegen)`          | 1000000       |  5.868 ms | 0.3266 ms | 0.5488 ms |  5.868 |  0.86 | 4781 |  76.29 MB |        1.00 |        80 |
| FlatTypes | `AutoMapper 14.0.0`                 | 1000000       | 29.645 ms | 0.8963 ms | 1.5062 ms | 29.645 |  4.37 | 4750 |  76.29 MB |        1.00 |        80 |
| FlatTypes | `Facet 6.5.5`                       | 1000000       |  7.801 ms | 1.0231 ms | 1.5467 ms |  7.801 |  1.15 | 8601 | 137.33 MB |        1.80 |       144 |
| FlatTypes | `Facet 6.5.5 (Compiled Projection)` | 1000000       |  5.508 ms | 0.7064 ms | 1.0679 ms |  5.508 |  0.81 | 4781 |  76.29 MB |        1.00 |        80 |
| FlatTypes | `Mapperly 4.3.1`                    | 1000000       |  6.521 ms | 0.8369 ms | 1.2652 ms |  6.521 |  0.96 | 4781 |  76.29 MB |        1.00 |        80 |

### ComplexTypes

| Scenario     | Method                              | MapOperations | Mean      | StdDev    | Error     | Ns/Map  | Ratio | Gen0   | Gen1 | Allocated  | Alloc Ratio | Bytes/Map |
|------------- |------------------------------------ |-------------- |----------:|----------:|----------:|--------:|------:|-------:|-----:|-----------:|------------:|----------:|
| ComplexTypes | `Mapster 10.0.8`                    | 1000000       |  78.59 ms |  1.519 ms |  2.553 ms |  78.586 |  1.00 |  28111 |    - |  450.13 MB |        1.00 |       472 |
| ComplexTypes | `Mapster 10.0.8 (Roslyn)`           | 1000000       |  53.48 ms |  1.760 ms |  2.958 ms |  53.475 |  0.68 |  25818 |    - |  411.99 MB |        0.92 |       432 |
| ComplexTypes | `Mapster 10.0.8 (FEC)`              | 1000000       |  69.29 ms |  1.539 ms |  2.326 ms |  69.288 |  0.88 |  28200 |    - |  450.13 MB |        1.00 |       472 |
| ComplexTypes | `Mapster 10.0.8 (Codegen)`          | 1000000       |  51.86 ms |  2.514 ms |  3.801 ms |  51.863 |  0.66 |  25750 |    - |  411.99 MB |        0.92 |       432 |
| ComplexTypes | `AutoMapper 14.0.0`                 | 1000000       | 112.27 ms |  4.516 ms |  7.590 ms | 112.270 |  1.43 |  29142 |    - |  465.39 MB |        1.03 |       488 |
| ComplexTypes | `Facet 6.5.5`                       | 1000000       | 446.72 ms | 51.706 ms | 78.172 ms | 446.725 |  5.69 | 175000 |  500 | 2792.36 MB |        6.20 |      2928 |
| ComplexTypes | `Facet 6.5.5 (Compiled Projection)` | 1000000       | 678.87 ms | 42.646 ms | 64.474 ms | 678.868 |  8.64 |  44000 |    - |  709.53 MB |        1.58 |       744 |
| ComplexTypes | `Mapperly 4.3.1`                    | 1000000       |  52.81 ms |  1.134 ms |  1.714 ms |  52.812 |  0.67 |  25769 |    - |  411.99 MB |        0.92 |       432 |

### RecursiveTypes

| Scenario       | Method                              | MapOperations | Mean      | StdDev    | Error     | Ns/Map  | Ratio | Gen0   | Gen1 | Allocated  | Alloc Ratio | Bytes/Map |
|--------------- |------------------------------------ |-------------- |----------:|----------:|----------:|--------:|------:|-------:|-----:|-----------:|------------:|----------:|
| RecursiveTypes | `Mapster 10.0.8`                    | 1000000       | 404.49 ms | 63.593 ms |  96.14 ms | 404.486 |  1.02 |  49000 |    - |  793.46 MB |        1.00 |       832 |
| RecursiveTypes | `Mapster 10.0.8 (Roslyn)`           | 1000000       | 383.00 ms | 34.563 ms |  52.25 ms | 382.999 |  0.97 |  49000 |    - |  793.46 MB |        1.00 |       832 |
| RecursiveTypes | `Mapster 10.0.8 (FEC)`              | 1000000       |  84.82 ms | 10.777 ms |  16.29 ms |  84.816 |  0.21 |  45875 |  125 |  732.42 MB |        0.92 |       768 |
| RecursiveTypes | `Mapster 10.0.8 (Codegen)`          | 1000000       |  82.81 ms | 11.104 ms |  16.79 ms |  82.806 |  0.21 |  49625 |  125 |  793.46 MB |        1.00 |       832 |
| RecursiveTypes | `AutoMapper 14.0.0`                 | 1000000       | 585.42 ms | 88.660 ms | 134.04 ms | 585.425 |  1.48 | 168000 | 1000 | 2693.18 MB |        3.39 |      2824 |
| RecursiveTypes | `Facet 6.5.5`                       | 1000000       | 302.24 ms |  9.444 ms |  18.06 ms | 302.241 |  0.76 | 150000 |  666 | 2395.63 MB |        3.02 |      2512 |
| RecursiveTypes | `Facet 6.5.5 (Compiled Projection)` | 1000000       | 708.77 ms | 58.207 ms |  88.00 ms | 708.770 |  1.79 |  73000 |    - | 1174.93 MB |        1.48 |      1232 |
| RecursiveTypes | `Mapperly 4.3.1`                    | 1000000       | 116.05 ms | 16.639 ms |  25.16 ms | 116.055 |  0.29 |  68833 |  166 | 1098.63 MB |        1.38 |      1152 |

### TotalAllTypes

| Scenario      | Method                              | MapOperations | Mean       | StdDev   | Error     | Ns/Map | Ratio | Gen0   | Gen1 | Allocated | Alloc Ratio | Bytes/Map |
|-------------- |------------------------------------ |-------------- |-----------:|---------:|----------:|-------:|------:|-------:|-----:|----------:|------------:|----------:|
| TotalAllTypes | `Mapster 10.0.8`                    | 1000000       |   413.9 ms | 16.88 ms |  25.52 ms | 137.97 |  1.00 |  82000 |    - |   1.29 GB |        1.00 |       461 |
| TotalAllTypes | `Mapster 10.0.8 (Roslyn)`           | 1000000       |   546.4 ms | 29.01 ms |  43.86 ms | 182.12 |  1.32 |  80000 |    - |   1.25 GB |        0.97 |       448 |
| TotalAllTypes | `Mapster 10.0.8 (FEC)`              | 1000000       |   155.7 ms | 24.48 ms |  37.01 ms |  51.91 |  0.38 |  78800 |    - |   1.23 GB |        0.95 |       440 |
| TotalAllTypes | `Mapster 10.0.8 (Codegen)`          | 1000000       |   117.4 ms |  8.56 ms |  12.94 ms |  39.14 |  0.28 |  80250 |    - |   1.25 GB |        0.97 |       448 |
| TotalAllTypes | `AutoMapper 14.0.0`                 | 1000000       |   681.4 ms | 76.25 ms | 115.27 ms | 227.13 |  1.65 | 202000 | 1000 |   3.16 GB |        2.45 |      1130 |
| TotalAllTypes | `Facet 6.5.5`                       | 1000000       |   677.4 ms | 33.30 ms |  55.96 ms | 225.80 |  1.64 | 329000 | 1000 |   5.14 GB |        3.99 |      1840 |
| TotalAllTypes | `Facet 6.5.5 (Compiled Projection)` | 1000000       | 1,337.4 ms | 42.95 ms |  64.94 ms | 445.81 |  3.24 | 122000 |    - |   1.91 GB |        1.49 |       685 |
| TotalAllTypes | `Mapperly 4.3.1`                    | 1000000       |   141.5 ms |  3.04 ms |   5.81 ms |  47.10 |  0.34 |  99250 |  250 |   1.55 GB |        1.20 |       554 |

## Interpretation notes

- The fastest method depends on the object shape. Flat DTOs mostly measure low-level call overhead; recursive graphs and collection-heavy DTOs shift the bottleneck toward nested object creation and collection mapping.
- `Facet` constructor and compiled-projection paths are shown separately because they exercise different APIs and can trade CPU time for allocation behavior differently across scenarios.
