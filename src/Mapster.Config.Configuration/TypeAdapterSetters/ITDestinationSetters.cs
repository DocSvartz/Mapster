using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster.Config.Configuration.TypeAdapterSetters
{
    public interface ITypeAdapterSetter<TDestination>
    {
        TypeAdapterSetter<TDestination> AfterMapping(Action<TDestination> action);
        TypeAdapterSetter<TDestination> AfterMappingInline(Expression<Action<TDestination>> action);
        TypeAdapterSetter<TDestination> BeforeMapping(Action<TDestination> action);
        TypeAdapterSetter<TDestination> BeforeMappingInline(Expression<Action<TDestination>> action);
        TypeAdapterSetter<TDestination> ConstructUsing(Expression<Func<TDestination>> constructUsing);
        TypeAdapterSetter<TDestination> Ignore(params Expression<Func<TDestination, object>>[] members);
        TypeAdapterSetter<TDestination> Map<TDestinationMember, TSourceMember>(Expression<Func<TDestination, TDestinationMember>> member, Expression<Func<TSourceMember>> source);
        TypeAdapterSetter<TDestination> Map<TDestinationMember>(Expression<Func<TDestination, TDestinationMember>> destinationMember, string sourceMemberName);
        TypeAdapterSetter<TDestination> MapToConstructor(ConstructorInfo ctor);
        //TypeAdapterSetters.TypeAdapterSetter UseDestinationValue(string destinationMemberName);
        TypeAdapterSetter<TDestination> UseDestinationValue<TDestinationMember>(Expression<Func<TDestination, TDestinationMember>> destinationMember);
    }
}