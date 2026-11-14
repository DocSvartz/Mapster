using Mapster.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster
{
    public static class Extensions
    {
        public static IQueryable<TDestination> ProjectToType<TDestination>(this IQueryable source, TypeAdapterConfig? config = null)
        {
            config ??= TypeAdapterConfig.GlobalSettings;
            var mockCall = config.GetSnapshot.GetProjectionCallExpression(source.ElementType, typeof(TDestination));
            var sourceCall = Expression.Call(mockCall.Method, source.Expression, mockCall.Arguments[1]);
            return source.Provider.CreateQuery<TDestination>(sourceCall);
        }

        public static IQueryable ProjectToType(this IQueryable source, Type destinationType, TypeAdapterConfig? config = null)
        {
            config ??= TypeAdapterConfig.GlobalSettings;
            var mockCall = config.GetSnapshot.GetProjectionCallExpression(source.ElementType, destinationType);
            var sourceCall = Expression.Call(mockCall.Method, source.Expression, mockCall.Arguments[1]);
            return source.Provider.CreateQuery(sourceCall);
        }

        public static bool HasCustomAttribute(this IMemberModel member, Type type)
        {
            return member.GetCustomAttributesData().Any(attr => attr.GetAttributeType() == type);
        }

        public static bool HasCustomAttribute<T>(this IMemberModel member) where T : Attribute
        {
            return member.GetCustomAttributesData().Any(attr => attr.GetAttributeType() == typeof(T));
        }

        public static T? GetCustomAttribute<T>(this IMemberModel member) where T : Attribute
        {
            return (T?) member.GetCustomAttributes(true).FirstOrDefault(attr => attr is T);
            // I don’t quite understand why this is necessary.
            // It seems to me that for an "Ignore" operation, the mere presence of the attribute should be sufficient. 
            // Why, then, is it necessary to create all the attributes while taking the hierarchy into account?

            // If we only need to check whether these attributes exist or not, then this is sufficient.
            //memberInfo.IsDefined(typeof(T), inherit: true);
        }

        internal static T? GetCustomAttributeFromData<T>(this IMemberModel member) where T : Attribute
        {
            var attr = member.GetCustomAttributesData().FirstOrDefault(it => it.GetAttributeType() == typeof(T));
            return attr?.CreateCustomAttribute<T>();
        }
    }
}
