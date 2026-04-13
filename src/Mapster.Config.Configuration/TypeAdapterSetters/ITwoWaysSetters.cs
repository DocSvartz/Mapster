using System;
using System.Linq.Expressions;

namespace Mapster.Config.Configuration.TypeAdapterSetters
{
    public interface ITwoWaysSetters<TSource, TDestination>
    {
        Configuration.TypeAdapterSetter<TDestination, TSource> DestinationToSourceSetter { get; }
        Configuration.TypeAdapterSetter<TSource, TDestination> SourceToDestinationSetter { get; }

       // TwoWaysTypeAdapterSetter<TSource, TDestination> AddDestinationTransform(DestinationTransform transform);
        TwoWaysTypeAdapterSetter<TSource, TDestination> AddDestinationTransform<TDestinationMember>(Expression<Func<TDestinationMember, TDestinationMember>> transform);
        TwoWaysTypeAdapterSetter<TSource, TDestination> AvoidInlineMapping(bool value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> EnableNonPublicMembers(bool value);
       // TwoWaysTypeAdapterSetter<TSource, TDestination> EnumMappingStrategy(EnumMappingStrategy strategy);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Fork(Action<ITypeAdapterConfig> action);
       // TwoWaysTypeAdapterSetter<TSource, TDestination> GenerateMapper(MapType mapType);
       // TwoWaysTypeAdapterSetter<TSource, TDestination> GetMemberName(Func<IMemberModel, MemberSide, string> func);
       // TwoWaysTypeAdapterSetter<TSource, TDestination> GetMemberName(Func<IMemberModel, string> func);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Ignore(params Expression<Func<TDestination, object>>[] members);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Ignore(params string[] names);
        TwoWaysTypeAdapterSetter<TSource, TDestination> IgnoreAttribute(params Type[] types);
       // TwoWaysTypeAdapterSetter<TSource, TDestination> IgnoreMember(Func<IMemberModel, MemberSide, bool> predicate);
        TwoWaysTypeAdapterSetter<TSource, TDestination> IgnoreNonMapped(bool value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> IgnoreNullValues(bool value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>()
            where TDerivedSource : class, TSource
            where TDerivedDestination : class, TDestination;
        TwoWaysTypeAdapterSetter<TSource, TDestination> IncludeAttribute(params Type[] types);
       // TwoWaysTypeAdapterSetter<TSource, TDestination> IncludeMember(Func<IMemberModel, MemberSide, bool> predicate);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Inherits<TBaseSource, TBaseDestination>();
        TwoWaysTypeAdapterSetter<TSource, TDestination> Map(string destinationMemberName, string sourceMemberName);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Map<TDestinationMember, TSourceMember>(Expression<Func<TDestination, TDestinationMember>> member, Expression<Func<TSource, TSourceMember>> source);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Map<TDestinationMember>(Expression<Func<TDestination, TDestinationMember>> destinationMember, string sourceMemberName);
        TwoWaysTypeAdapterSetter<TSource, TDestination> Map<TSourceMember>(string memberName, Expression<Func<TSource, TSourceMember>> source);
        TwoWaysTypeAdapterSetter<TSource, TDestination> MapToConstructor(bool value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> MaxDepth(int value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> NameMatchingStrategy(NameMatchingStrategy value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> PreserveReference(bool value);
        TwoWaysTypeAdapterSetter<TSource, TDestination> ShallowCopyForSameType(bool value);
    }
}