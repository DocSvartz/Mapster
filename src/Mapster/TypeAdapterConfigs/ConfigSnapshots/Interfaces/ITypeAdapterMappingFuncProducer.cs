using Mapster.Models;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    public interface ITypeAdapterMappingFuncProducer
    {
        public bool GetBoolSetting(string name);

        internal Func<LambdaExpression, Delegate> Compiler { get; init; }
        internal Delegate GetMapFunction(Type sourceType, Type destinationType);
        internal Delegate GetMapToTargetFunction(Type sourceType, Type destinationType);
        internal MethodCallExpression GetProjectionCallExpression(Type sourceType, Type destinationType);
        internal Expression CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p);
        internal Expression CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1);
        internal Expression CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2);
        internal LambdaExpression CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping = null);
        internal MethodCallExpression CreateProjectionCallExpression(TypeTuple tuple);

        LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType);

        Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType);
        Func<TSource, TDestination> GetMapFunction<TSource, TDestination>();
        Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>();
    }
}
