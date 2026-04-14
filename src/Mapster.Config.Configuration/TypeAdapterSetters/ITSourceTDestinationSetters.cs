using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster.Configuration.TypeAdapterSetters
{
    public interface ITSourceTDestinationSetters<TSource, TDestination>
    {
        TypeAdapterSetter<TSource, TDestination> AfterMapping(Action<TDestination> action);
        TypeAdapterSetter<TSource, TDestination> AfterMapping(Action<TSource, TDestination, TDestination?> action);
        TypeAdapterSetter<TSource, TDestination> AfterMapping(Action<TSource, TDestination> action);
        TypeAdapterSetter<TSource, TDestination> AfterMappingInline(Expression<Action<TDestination>> action);
        TypeAdapterSetter<TSource, TDestination> AfterMappingInline(Expression<Action<TSource, TDestination, TDestination?>> action);
        TypeAdapterSetter<TSource, TDestination> AfterMappingInline(Expression<Action<TSource, TDestination>> action);
        TypeAdapterSetter<TSource, TDestination> BeforeMapping(Action<TDestination> action);
        TypeAdapterSetter<TSource, TDestination> BeforeMapping(Action<TSource, TDestination, TDestination?> action);
        TypeAdapterSetter<TSource, TDestination> BeforeMapping(Action<TSource, TDestination> action);
        TypeAdapterSetter<TSource, TDestination> BeforeMappingInline(Expression<Action<TDestination>> action);
        TypeAdapterSetter<TSource, TDestination> BeforeMappingInline(Expression<Action<TSource, TDestination, TDestination?>> action);
        TypeAdapterSetter<TSource, TDestination> BeforeMappingInline(Expression<Action<TSource, TDestination>> action);
        void Compile();
        void CompileProjection();
        TypeAdapterSetter<TSource, TDestination> ConstructUsing(Expression<Func<TDestination>> constructUsing);
        TypeAdapterSetter<TSource, TDestination> ConstructUsing(Expression<Func<TSource, TDestination?, TDestination>> constructUsing);
        TypeAdapterSetter<TSource, TDestination> ConstructUsing(Expression<Func<TSource, TDestination>> constructUsing);
        TypeAdapterSetter<TSource, TDestination> Fork(Action<ITypeAdapterConfigBase> action);
        //TypeAdapterSetter<TSource, TDestination> GenerateMapper(MapType mapType);
        TypeAdapterSetter<TSource, TDestination> Ignore(params Expression<Func<TDestination, object>>[] members);
        TypeAdapterSetter<TSource, TDestination> IgnoreIf(Expression<Func<TSource, TDestination, bool>> condition, params Expression<Func<TDestination, object>>[] members);
        TypeAdapterSetter<TSource, TDestination> IgnoreIf(Expression<Func<TSource, TDestination, bool>> condition, params string[] members);
        TypeAdapterSetter<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>()
            where TDerivedSource : class, TSource
            where TDerivedDestination : class, TDestination;
        TypeAdapterSetter<TSource, TDestination> Inherits<TBaseSource, TBaseDestination>();
        TypeAdapterSetter<TSource, TDestination> Map<TDestinationMember, TSourceMember>(Expression<Func<TDestination, TDestinationMember>> member, Expression<Func<TSource, TSourceMember>> source, Expression<Func<TSource, bool>> shouldMap = null);
        TypeAdapterSetter<TSource, TDestination> Map<TDestinationMember, TSourceMember>(Expression<Func<TDestination, TDestinationMember>> member, Expression<Func<TSourceMember>> source);
        TypeAdapterSetter<TSource, TDestination> Map<TDestinationMember>(Expression<Func<TDestination, TDestinationMember>> destinationMember, string sourceMemberName);
        TypeAdapterSetter<TSource, TDestination> Map<TSourceMember>(string memberName, Expression<Func<TSource, TSourceMember>> source, Expression<Func<TSource, bool>> shouldMap = null);
        TypeAdapterSetter<TSource, TDestination> MapToConstructor(ConstructorInfo ctor);
        TypeAdapterSetter<TSource, TDestination> MapToTargetWith(Expression<Func<TSource, TDestination, TDestination>> converterFactory, bool applySettings = false);
        TypeAdapterSetter<TSource, TDestination> MapWith(Expression<Func<TSource, TDestination>> converterFactory, bool applySettings = false);
        TwoWaysTypeAdapterSetter<TSource, TDestination> TwoWays();
    }
}