using Mapster.Config.Configuration.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster.Config.Configuration.TypeAdapterSetters
{
    public static class TypeAdapterSetterExtensions
    {
        internal static void CheckCompiled<TSetter>(this TSetter setter) where TSetter : TypeAdapterSetter
        {
            if (setter.Settings.Compiled)
                throw new InvalidOperationException("TypeAdapter.Adapt was already called, please clone or create new TypeAdapterConfig.");
        }

        //public static TSetter AddDestinationTransform<TSetter, TDestinationMember>(this TSetter setter, Expression<Func<TDestinationMember, TDestinationMember>> transform) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.DestinationTransforms.Add(new DestinationTransform
        //    {
        //        Condition = t => t == typeof(TDestinationMember),
        //        TransformFunc = _ => transform,
        //    });
        //    return setter;
        //}

        //public static TSetter AddDestinationTransform<TSetter>(this TSetter setter, DestinationTransform transform) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.DestinationTransforms.Add(transform);
        //    return setter;
        //}

        //public static TSetter Ignore<TSetter>(this TSetter setter, params string[] names) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    foreach (var name in names)
        //    {
        //        setter.Settings.Ignore[name] = new IgnoreDictionary.IgnoreItem();
        //    }
        //    return setter;
        //}

        //public static TSetter IgnoreAttribute<TSetter>(this TSetter setter, params Type[] types) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    foreach (var type in types)
        //    {
        //        setter.Settings.ShouldMapMember.Add((member, _) => member.HasCustomAttribute(type) ? (bool?)false : null);
        //    }
        //    return setter;
        //}

        //public static TSetter IncludeAttribute<TSetter>(this TSetter setter, params Type[] types) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    foreach (var type in types)
        //    {
        //        setter.Settings.ShouldMapMember.Add((member, _) => member.HasCustomAttribute(type) ? (bool?)true : null);
        //    }
        //    return setter;
        //}

        //public static TSetter IgnoreMember<TSetter>(this TSetter setter, Func<IMemberModel, MemberSide, bool> predicate) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.ShouldMapMember.Add((member, side) => predicate(member, side) ? (bool?)false : null);
        //    return setter;
        //}

        //public static TSetter IncludeMember<TSetter>(this TSetter setter, Func<IMemberModel, MemberSide, bool> predicate) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.ShouldMapMember.Add((member, side) => predicate(member, side) ? (bool?)true : null);
        //    return setter;
        //}

        public static TSetter ShallowCopyForSameType<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.ShallowCopyForSameType = value;
            return setter;
        }

        //public static TSetter EnumMappingStrategy<TSetter>(this TSetter setter, EnumMappingStrategy strategy) where TSetter : TypeAdapterSetter
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.MapEnumByName = strategy == Mapster.EnumMappingStrategy.ByName;
        //    return setter;
        //}

        public static TSetter IgnoreNullValues<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.IgnoreNullValues = value;
            return setter;
        }

        public static TSetter PreserveReference<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.PreserveReference = value;
            return setter;
        }

        public static TSetter NameMatchingStrategy<TSetter>(this TSetter setter, NameMatchingStrategy value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.NameMatchingStrategy = value;
            return setter;
        }

        //public static TSetter Map<TSetter, TSourceMember>(
        //    this TSetter setter, string memberName,
        //    Expression<Func<TSourceMember>> source) where TSetter : TypeAdapterSetter
        //{
        //    setter.CheckCompiled();

        //    var invoker = Expression.Lambda(source.Body, Expression.Parameter(typeof(object)));
        //    setter.Settings.Resolvers.Add(new InvokerModel
        //    {
        //        DestinationMemberName = memberName,
        //        Invoker = invoker,
        //        Condition = null
        //    });

        //    return setter;
        //}

        //public static TSetter Map<TSetter, TSource, TSourceMember>(
        //    this TSetter setter, string memberName,
        //    Expression<Func<TSource, TSourceMember>> source) where TSetter : TypeAdapterSetter
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.Resolvers.Add(new InvokerModel
        //    {
        //        DestinationMemberName = memberName,
        //        SourceMemberName = source.GetMemberPath(noError: true),
        //        Invoker = source,
        //        Condition = null
        //    });

        //    return setter;
        //}

        //public static TSetter Map<TSetter>(
        //    this TSetter setter, string destinationMemberName, string sourceMemberName) where TSetter : TypeAdapterSetter
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.Resolvers.Add(new InvokerModel
        //    {
        //        DestinationMemberName = destinationMemberName,
        //        SourceMemberName = sourceMemberName,
        //        Condition = null
        //    });

        //    return setter;
        //}

        public static TSetter EnableNonPublicMembers<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.EnableNonPublicMembers = value;
            return setter;
        }

        public static TSetter IgnoreNonMapped<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.IgnoreNonMapped = value;
            return setter;
        }

        public static TSetter AvoidInlineMapping<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.AvoidInlineMapping = value;
            return setter;
        }

        public static TSetter RequireDestinationMemberSource<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.RequireDestinationMemberSource = value;
            return setter;
        }

        //public static TSetter GetMemberName<TSetter>(this TSetter setter, Func<IMemberModel, string?> func) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.GetMemberNames.Add((member, _) => func(member));
        //    return setter;
        //}

        //public static TSetter GetMemberName<TSetter>(this TSetter setter, Func<IMemberModel, MemberSide, string?> func) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.GetMemberNames.Add(func);
        //    return setter;
        //}

        public static TSetter MapToConstructor<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.MapToConstructor = value ? "*" : null;
            return setter;
        }

        public static TSetter MaxDepth<TSetter>(this TSetter setter, int? value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.MaxDepth = value;
            return setter;
        }

        public static TSetter Unflattening<TSetter>(this TSetter setter, bool value) where TSetter : TypeAdapterSetter
        {
            setter.CheckCompiled();

            setter.Settings.Unflattening = value;
            return setter;
        }

        //public static TSetter UseDestinationValue<TSetter>(this TSetter setter, Func<IMemberModel, bool> func) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    setter.Settings.UseDestinationValues.Add(func);
        //    return setter;
        //}

        //public static TSetter Include<TSetter>(this TSetter setter, Type sourceType, Type destType) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();

        //    Type baseSourceType = setter.Settings.SourceType ?? typeof(void);
        //    Type baseDestinationType = setter.Settings.DestinationType ?? typeof(void);

        //    if (baseSourceType.IsOpenGenericType() && baseDestinationType.IsOpenGenericType())
        //    {
        //        if (!sourceType.IsAssignableToGenericType(baseSourceType))
        //            throw new InvalidCastException("In order to use inherits, TSource must be inherited from TBaseSource.");
        //        if (!destType.IsAssignableToGenericType(baseDestinationType))
        //            throw new InvalidCastException("In order to use inherits, TDestination must be inherited from TBaseDestination.");
        //    }
        //    else
        //    {
        //        if (!baseSourceType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo()))
        //            throw new InvalidCastException("In order to use inherits, TSource must be inherited from TBaseSource.");

        //        if (!baseDestinationType.GetTypeInfo().IsAssignableFrom(destType.GetTypeInfo()))
        //            throw new InvalidCastException("In order to use inherits, TDestination must be inherited from TBaseDestination.");
        //    }
        
        //    setter.Config.Rules.LockAdd(new TypeAdapterRule
        //    {
        //        Priority = arg =>
        //            arg.SourceType == sourceType &&
        //            arg.DestinationType == destType ? (int?)100 : null,
        //        Settings = setter.Settings
        //    });

        //    setter.Settings.Includes.Add(new TypeTuple(sourceType, destType));

        //    return setter;
        //}

        //public static TSetter Inherits<TSetter>(this TSetter setter, Type baseSourceType, Type baseDestinationType) where TSetter : TypeAdapterSetterBase
        //{
        //    setter.CheckCompiled();
                      
        //    Type derivedSourceType = setter.Settings.SourceType ?? typeof(void);
        //    Type derivedDestinationType = setter.Settings.DestinationType ?? typeof(void);

        //    if(baseSourceType.IsOpenGenericType() && baseDestinationType.IsOpenGenericType())
        //    {
        //        if (!derivedSourceType.IsAssignableToGenericType(baseSourceType))
        //            throw new InvalidCastException("In order to use inherits, TSource must be inherited from TBaseSource.");
        //        if (!derivedDestinationType.IsAssignableToGenericType(baseDestinationType))
        //            throw new InvalidCastException("In order to use inherits, TDestination must be inherited from TBaseDestination.");
        //    }
        //    else
        //    {
        //        if (!baseSourceType.GetTypeInfo().IsAssignableFrom(derivedSourceType.GetTypeInfo()))
        //            throw new InvalidCastException("In order to use inherits, TSource must be inherited from TBaseSource.");

        //        if (!baseDestinationType.GetTypeInfo().IsAssignableFrom(derivedDestinationType.GetTypeInfo()))
        //            throw new InvalidCastException("In order to use inherits, TDestination must be inherited from TBaseDestination.");
        //    }

        //    if (setter.Config.RuleMap.TryGetValue(new TypeTuple(baseSourceType, baseDestinationType), out var rule))
        //    {
        //        setter.Settings.Apply(rule.Settings);
        //    }
        //    return setter;
        //}

        //public static TSetter ApplyAdaptAttribute<TSetter>(this TSetter setter, BaseAdaptAttribute attr) where TSetter : TypeAdapterSetterBase
        //{
        //    if (attr.IgnoreAttributes != null)
        //        setter.IgnoreAttribute(attr.IgnoreAttributes);
        //    if (attr.IgnoreNoAttributes != null)
        //    {
        //        setter.IgnoreMember((member, _) => !member.GetCustomAttributesData()
        //            .Select(it => it.GetAttributeType())
        //            .Intersect(attr.IgnoreNoAttributes)
        //            .Any());
        //    }
        //    if (attr.IgnoreNamespaces != null)
        //    {
        //        foreach (var ns in attr.IgnoreNamespaces)
        //        {
        //            setter.IgnoreMember((member, _) => member.Type.Namespace?.StartsWith(ns) == true);
        //        }
        //    }
        //    if (attr.MaxDepth > 0)
        //        setter.MaxDepth(attr.MaxDepth);
        //    if (attr.GetBooleanSettingValues(nameof(attr.IgnoreNullValues)) != null)
        //        setter.IgnoreNullValues(attr.IgnoreNullValues);
        //    if (attr.GetBooleanSettingValues(nameof(attr.MapToConstructor)) != null)
        //        setter.MapToConstructor(attr.MapToConstructor);
        //    if (attr.GetBooleanSettingValues(nameof(attr.PreserveReference)) != null)
        //        setter.PreserveReference(attr.PreserveReference);
        //    if (attr.GetBooleanSettingValues(nameof(attr.ShallowCopyForSameType)) != null)
        //        setter.ShallowCopyForSameType(attr.ShallowCopyForSameType);
        //    if (attr.GetBooleanSettingValues(nameof(attr.RequireDestinationMemberSource)) != null)
        //        setter.RequireDestinationMemberSource(attr.RequireDestinationMemberSource);
        //    return setter;
        //}
    } 
}
