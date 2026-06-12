---
uid: Mapster.References
---

# References

## Basic

| Method       | Description           | Link  |
| -------------|-----------------------| ----- |
| `src.Adapt<Dest>()`     | Mapping to new type | [basic](xref:Mapster.Mapping.BasicUsages) |
| `src.Adapt(dest)`       | Mapping to existing object      | [basic](xref:Mapster.Mapping.BasicUsages) |
| `query.ProjectToType<Dest>()` | Mapping from queryable     | [basic](xref:Mapster.Mapping.BasicUsages) |
| | Convention & Data type support | [data types](xref:Mapster.Mapping.DataTypes.Overview) |

### Mapper instance (for dependency injection)

| Method       | Description          | Link  |
| -------------|----------------------| ----- |
| `IMapper mapper = new Mapper()`| Create mapper instance | [mappers](xref:Mapster.Mapping.Mappers) |
| `mapper.Map<Dest>(src)`        | Mapping to new type |  |
| `mapper.Map(src, dest)`       | Mapping to existing object      |  |

### Builder (for complex mapping)

| Method        | Description           | Link  |
| ------------- |-----------------------| ----- |
| `src.BuildAdapter()` <br> `mapper.From(src)`     | Create builder | [mappers](xref:Mapster.Mapping.Mappers) |
| `.ForkConfig(config => ...)`     | Inline configuration | [config location](xref:Mapster.Configuration.Location) |
| `.AddParameters(name, value)`       | Passing runtime value      | [setting values](xref:Mapster.Settings.SettingValues) |
| `.AdaptToType<Dest>()`     | Mapping to new type |  |
| `.AdaptTo(dest)`       | Mapping to existing object      |  |
| `.CreateMapExpression<Dest>()`     | Get mapping expression |  |
| `.CreateMapToTargetExpression<Dest>()`       | Get mapping to existing object expression |  |
| `.CreateProjectionExpression<Dest>()`     | Get mapping from queryable expression |  |

## Config

| Method                                       | Description                             | Link  |
|----------------------------------------------|-----------------------------------------| ----- |
| `TypeAdapterConfig.GlobalSettings`           | Global config                           | [config](xref:Mapster.Configuration.Overview) |
| `var config = new TypeAdapterConfig()`       | Create new config instance              | [config instance](xref:Mapster.Configuration.Instance) |
| `src.Adapt<Dest>(config)`                    | Passing config to mapping               |  |
| `new Mapper(config)`                         | Passing config to mapper instance       |  |
| `src.BuildAdapter(config)`                   | Passing config to builder               |  |
| `config.RequireDestinationMemberSource`      | Validate all properties are mapped      | [config validation](xref:Mapster.Configuration.ValidationAndCompilation) |
| `config.RequireExplicitMapping`              | Validate all type pairs are defined     | [config validation](xref:Mapster.Configuration.ValidationAndCompilation) |
| `config.AllowImplicitDestinationInheritance` | Use config from destination based class | [inheritance](xref:Mapster.Configuration.Inheritance) |
| `config.AllowImplicitSourceInheritance`      | Use config from source based class      | [inheritance](xref:Mapster.Configuration.Inheritance) |
| `config.SelfContainedCodeGeneration`         | Generate all nested mapping in 1 method | [TextTemplate](xref:Mapster.Tools.TextTemplate) |
| `config.Compile()`                           | Validate mapping instruction & cache    | [config validation](xref:Mapster.Configuration.ValidationAndCompilation) |
| `config.CompileProjection()`                 | Validate mapping instruction & cache for queryable |  |
| `config.Clone()`                             | Copy config | [config instance](xref:Mapster.Configuration.Instance) |
| `config.Fork(forked => ...)`                 | Inline configuration | [config location](xref:Mapster.Configuration.Location) |

### Config scanning

| Method                       | Description                   | Link  |
|------------------------------|-------------------------------| ----- |
| `IRegister`                  | Interface for config scanning | [config location](xref:Mapster.Configuration.Location) |
| `config.Scan(...assemblies)` | Scan for config in assemblies | [config location](xref:Mapster.Configuration.Location) |
| `config.Apply(...registers)` | Apply registers directly      | [config location](xref:Mapster.Configuration.Location) |

<!-- TODO: Check if this might be Settings? If so, the used variable might not be the best choice 'config' -->
### Declare settings

| Method        | Description           | Link  |
| ------------- |-----------------------| ----- |
| `config.Default` | Get setting applied to all type pairs | [config](xref:Mapster.Configuration.Overview) |
| `TypeAdapterConfig<Src, Dest>.NewConfig()` <br> `config.NewConfig<Src, Dest>()` | Create setting applied to specific type pairs | [config](xref:Mapster.Configuration.Overview) |
| `TypeAdapterConfig<Src, Dest>.ForType()` <br> `config.ForType<Src, Dest>()` | Get setting applied to specific type pairs | [config](xref:Mapster.Configuration.Overview) |
| `config.ForType(typeof(GenericPoco<>),typeof(GenericDto<>))` | Get setting applied to generic type pairs | [config](xref:Mapster.Configuration.Overview) |
| `config.When((src, dest, mapType) => ...)` | Get setting that applied conditionally | [config](xref:Mapster.Configuration.Overview) |
| `config.ForDestinationType<Dest>()` | Get setting that applied to specific destination type | [config](xref:Mapster.Configuration.Overview) |
|  | Configuration for nested mapping | [nested mapping](xref:Mapster.Configuration.NestedMapping) |

## Settings

| Method        | Description           | Apply to queryable | Link  |
| ------------- |-----------------------| ------------ | ----- |
| `AddDestinationTransform` | Clean up data for a specific type | x | [setting values](xref:Mapster.Settings.SettingValues) |
| `AfterMapping` | Add steps after mapping done |  | [before-after](xref:Mapster.Settings.BeforeAfterMapping) |
| `AvoidInlineMapping` | Skip inline process for large type mapping |  | [object reference](xref:Mapster.Settings.ObjectReferences) |
| `BeforeMapping` | Add steps before mapping start |  | [before-after](xref:Mapster.Settings.BeforeAfterMapping) |
| `ConstructUsing` | Define how to create object | x | [constructor](xref:Mapster.Settings.ConstructorMapping) |
| `EnableNonPublicMembers` | Mapping non-public properties |  | [non-public](xref:Mapster.Settings.Custom.NonPublicMembers) |
| `EnumMappingStrategy` | Choose whether mapping enum by value or by name |  | [data types](xref:Mapster.Mapping.DataTypes.Primitives) |
| `Fork` | Add new settings without side effect on main config | x | [nested mapping](xref:Mapster.Configuration.NestedMapping) |
| `GetMemberName` | Define how to resolve property name | x | [custom naming](xref:Mapster.Settings.Custom.NamingConvention) |
| `Ignore` | Ignore specific properties | x | [ignore](xref:Mapster.Settings.Custom.IgnoringMembers) |
| `IgnoreAttribute` | Ignore specific attributes annotated on properties | x | [attribute](xref:Mapster.Settings.Custom.Attributes) |
| `IgnoreIf` | Ignore conditionally | x | [ignore](xref:Mapster.Settings.Custom.IgnoringMembers) |
| `IgnoreMember` | Setup rules to ignore | x | [rule based](xref:Mapster.Settings.Custom.RuleBasedMapping) |
| `IgnoreNonMapped` | Ignore all properties not defined in `Map` | x | [ignore](xref:Mapster.Settings.Custom.IgnoringMembers) |
| `IgnoreNullValues` | Not map if src property is null |  | [shallow & merge](xref:Mapster.Settings.ShallowMerge) |
| `Include` | Include derived types on mapping |  | [inheritance](xref:Mapster.Configuration.Inheritance) |
| `IncludeAttribute` | Include specific attributes annotated on properties | x | [attribute](xref:Mapster.Settings.Custom.Attributes) |
| `IncludeMember` | Setup rules to include | x | [rule based](xref:Mapster.Settings.Custom.RuleBasedMapping) |
| `Inherits` | Copy setting from based type | x | [inheritance](xref:Mapster.Configuration.Inheritance) |
| `Map` | Define property pairs | x | [custom mapping](xref:Mapster.Settings.Custom.Mapping) |
| `MapToConstructor` | Mapping to constructor | x | [constructor](xref:Mapster.Settings.ConstructorMapping) |
| `MapToTargetWith` | Define how to map to existing object between type pair |  | [custom conversion](xref:Mapster.Settings.CustomConversionLogic) |
| `MapWith` | Define how to map between type pair | x | [custom conversion](xref:Mapster.Settings.CustomConversionLogic) |
| `MaxDepth` | Limit depth of nested mapping | x | [object reference](xref:Mapster.Settings.ObjectReferences) |
| `NameMatchingStrategy` | Define how to resolve property's name | x | [custom naming](xref:Mapster.Settings.Custom.NamingConvention) |
| `PreserveReference` | Tracking reference when mapping |  | [object reference](xref:Mapster.Settings.ObjectReferences) |
| `ShallowCopyForSameType` | Direct assign rather than deep clone if type pairs are the same |  | [shallow & merge](xref:Mapster.Settings.ShallowMerge) |
| `TwoWays` | Define type mapping are 2 ways | x | [2-ways & unflattening](xref:Mapster.Settings.Custom.TwoWaysMapping) |
| `Unflattening` | Allow unflatten mapping | x |[2-ways & unflattening](xref:Mapster.Settings.Custom.TwoWaysMapping) |
| `UseDestinationValue` | Use existing property object to map data |  |[readonly-prop](xref:Mapster.Settings.Custom.ReadonlyProperty) |

## Attributes

| Annotation        | Description           | Link  |
| ------------- |-----------------------| ----- |
| `[AdaptMember(name)]` | Mapping property to different name | [attribute](xref:Mapster.Settings.Custom.Attributes) |
| `[AdaptIgnore(side)]` | Ignore property from mapping | [attribute](xref:Mapster.Settings.Custom.Attributes) |
| `[UseDestinationValue]` | Use existing property object to map data | [attribute](xref:Mapster.Settings.Custom.Attributes) |
| `[AdaptTo]` `[AdaptFrom]` `[AdaptTwoWays]` | Add setting on POCO class | [location](xref:Mapster.Configuration.Location#attributes) |
| `[Mapper]` `[GenerateMapper]` `[PropertyType]` | Define setting for code generation | [Mapster.Tool](xref:Mapster.Tools.MapsterTool.Overview) |

## Packages

| Packages | Method        | Description           |
| ------ | ------------- |-----------------------|
| [Async](xref:Mapster.Packages.Async) | `setting.AfterMappingAsync` <br> `builder.AdaptToTypeAsync` | perform async operation on mapping |
| [Debugging](xref:Mapster.Packages.ExpressionDebugging) | `config.Compiler = exp => exp.CompileWithDebugInfo()` | compile to allow step into debugging |
| [Dependency Injection](xref:Mapster.Packages.DependencyInjection) | `MapContext.Current.GetService<IService>()` | Inject service into mapping logic |
| [EF 6 & EF Core](xref:Mapster.Packages.EntityFramework) | `builder.EntityFromContext` | Copy data to tracked EF entity |
| [FEC](xref:Mapster.Packages.FastExpressionCompiler) | `config.Compiler = exp => exp.CompileFast()` | compile using FastExpressionCompiler |
| [Immutable](xref:Mapster.Packages.Immutable) | `config.EnableImmutableMapping()` | mapping to immutable collection |
| [Json.net](xref:Mapster.Packages.JsonNet) | `config.EnableJsonMapping()` | map json from/to poco and string |

## Code Generation Tools

| Plugin | Tool          | Description           |
| ------ | ------------- |-----------------------|
| [Mapster.Tool](xref:Mapster.Tools.MapsterTool.Overview) | `dotnet mapster` | generate DTOs and mapping codes on build |
| [TextTemplate](xref:Mapster.Tools.TextTemplate) | `t4` | generate mapping codes using t4 |
