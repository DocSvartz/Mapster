using Mapster.Cache;
using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster
{
    public abstract class BaseMapFuncProducer 
    {
        
        internal Func<LambdaExpression, Delegate> Compiler { get; init; }
                

        #region CacheFunc 
        protected abstract Delegate AddOrGetMapFuncFromCache(TypeTuple tuple, bool forceAdd = false);
        protected abstract Delegate AddOrGetMapToTargetFuncFromCache(TypeTuple tuple, bool forceAdd = false);
        protected abstract MethodCallExpression AddOrGetProjectionFuncFromCache(TypeTuple tuple, bool forceAdd = false);
        protected abstract Func<object, TDestination> AddOrGetGetDynamicMapFunction<TDestination>(Type sourceType, bool forceAdd = false);
        protected abstract void InvalidateGenericCacheMapFunc(TypeTuple tuple);
        protected abstract bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker);
        protected T AddToHash<T>(ConcurrentDictionary<TypeTuple, T> hash, TypeTuple key, Func<TypeTuple, T> func)
        {
            return hash.GetOrAdd(key, types =>
            {
                var del = func(types);
                hash[types] = del;

               // if (RuleMap.TryGetValue(types, out var rule))
                   // rule.Settings.Compiled = true;
                return del;

            });
        }

        #endregion CacheFunc

        protected abstract Expression CreateSelfExpression();
        //{
        //    //if (this == GlobalSettings)
        //    //    return Expression.Property(null, typeof(TypeAdapterConfig).GetProperty(nameof(GlobalSettings))!);
        //    //else
        //        return Expression.Constant(this);
        //}

        internal Expression CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1)
        {
            var method = (from m in typeof(ITypeAdapterMappingFuncProducer).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          where m.Name == nameof(ITypeAdapterMappingFuncProducer.GetDynamicMapFunction)
                          select m).First().MakeGenericMethod(destinationType);
            var getType = typeof(object).GetMethod(nameof(GetType));
            var invoker = Expression.Call(CreateSelfExpression(), method, Expression.Call(p1, getType!));
            return Expression.Call(invoker, "Invoke", null, p1);
        }

        public abstract LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType);

        internal MethodCallExpression CreateProjectionCallExpression(TypeTuple tuple)
        {
            var lambda = CreateMapExpression(tuple, MapType.Projection);
            var source = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(tuple.Source));
            var methodInfo = (from method in typeof(Queryable).GetMethods()
                              where method.Name == nameof(Queryable.Select)
                              let p = method.GetParameters()[1]
                              where p.ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<,>)
                              select method).First().MakeGenericMethod(tuple.Source, tuple.Destination);
            return Expression.Call(methodInfo, source, Expression.Quote(lambda));
        }

        internal protected static LambdaExpression CreateMapExpression(CompileArgument arg)
        {
            var fn = arg.MapType == MapType.MapToTarget
                ? arg.Settings.ConverterToTargetFactory
                : arg.Settings.ConverterFactory;
            if (fn == null)
                throw new CompileException(arg, new InvalidOperationException("ConverterFactory is not found"));
            try
            {
                return AdjustInheritedConverterReturnType(fn(arg), arg);
            }
            catch (Exception ex)
            {
                throw new CompileException(arg, ex);
            }
        }

        private static LambdaExpression AdjustInheritedConverterReturnType(LambdaExpression lambda, CompileArgument arg)
        {

            if (arg.Settings.ApplyCustomConverterFactoryNullPropagation.GetValueOrDefault())
                lambda = Expression.Lambda(lambda.Parameters[0].NotNullReturn(lambda.Body, arg), lambda.Parameters);

            var destinationType = arg.DestinationType;
            var returnType = lambda.ReturnType;
            var lamdaBody = lambda.Body;

            // Support for using ValueType mapping configurations of types, for mapping cases on Nulllable ValueType values
            if (arg.DestinationType.IsNullable() && !returnType.IsNullable() && returnType.IsValueType)
            {
                lamdaBody = Expression.Convert(lambda.Body, typeof(Nullable<>).MakeGenericType(lambda.ReturnType));
                lambda = Expression.Lambda(lamdaBody, lambda.Parameters);

                returnType = lambda.ReturnType;
            }

            if (returnType == destinationType)
                return lambda;

            //Support for using ValueType mapping configurations of types, for mapping cases on Nulllable ValueType values
            if (destinationType.IsNullable() && lambda.ReturnType.IsInterface)
            {
                var realDestType = destinationType.GetGenericArguments().FirstOrDefault();

                if (realDestType is null || !returnType.IsAssignableFrom(realDestType))
                    return lambda;
            }
            // MapWith configured on a base destination type returns the base type, but implicit
            // destination inheritance can compile the converter for a derived destination.
            else if (!returnType.IsAssignableFrom(destinationType))
                return lambda;

            Expression body;

            if (destinationType.CanBeNull())
                body = Expression.TypeAs(lamdaBody, destinationType);
            else
            {
                var tempDest = Expression.Variable(returnType, "tempDest");

                var variables = new[] { tempDest };

                var blockbody = new List<Expression>() { Expression.Assign(tempDest, lamdaBody) };

                var condition = Expression.TypeIs(tempDest, destinationType);
                UnaryExpression ifTrue = Expression.Convert(tempDest, destinationType);
                Expression ifFalse = destinationType.CreateDefault(arg);

                ConditionalExpression conditionalExpr = Expression.Condition(condition, ifTrue, ifFalse);
                blockbody.Add(conditionalExpr);

                BlockExpression body2 = Expression.Block(variables, blockbody);

                return Expression.Lambda(body2, lambda.Parameters);

            }

            return Expression.Lambda(body, lambda.Parameters);
        }

        protected LambdaExpression CreateDynamicMapExpression(TypeTuple tuple)
        {
            var lambda = CreateMapExpression(tuple, MapType.Map);
            var pNew = Expression.Parameter(typeof(object));
            var pOld = lambda.Parameters[0];
            var assign = ExpressionEx.Assign(pOld, pNew);
            return Expression.Lambda(
                Expression.Block(new[] { pOld }, assign, lambda.Body),
                pNew);
        }

        internal LambdaExpression CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping = null)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            var subFunction = context.IsSubFunction();

            if (!subFunction)
            {
                if (context.Running.Contains(tuple))
                {
                    if (mapType == MapType.Projection)
                        throw new InvalidOperationException("Projection does not support circular reference, please use MaxDepth setting");
                    return CreateMapInvokeExpression(tuple, mapType);
                }
                context.Running.Add(tuple);
            }

            try
            {
                var arg = GetCompileArgument(tuple, mapType, context);
                if (mapping != null)
                {
                    arg.Settings.Resolvers.AddRange(mapping.NextResolvers);
                    arg.Settings.Ignore.Apply(mapping.NextIgnore);
                    arg.UseDestinationValue = mapping.UseDestinationValue;


                    // Need refactoring !!!
                    if (mapping.OverrideSettings != null)
                    {
                        mapping.OverrideSettings.Apply(arg.Settings);

                        mapping.OverrideSettings.Source = arg.Source;
                        mapping.OverrideSettings.Destination = arg.Source;
                        mapping.OverrideSettings.Result = arg.Result;


                        if (mapping.OverrideSettings.ConverterFactory == null || mapping.OverrideSettings.ConverterToTargetFactory == null)
                        {
                            var defaultfactory = GetOvverideDefaultSettings(tuple, mapType);

                            if (mapping.OverrideSettings.ConverterFactory == null)
                                mapping.OverrideSettings.ConverterFactory = defaultfactory.ConverterFactory;
                            if (mapping.OverrideSettings.ConverterToTargetFactory == null)
                                mapping.OverrideSettings.ConverterToTargetFactory = defaultfactory.ConverterToTargetFactory;

                        }


                        arg.Settings = mapping.OverrideSettings;
                    }

                }

                //context.MappingCache.CollectCompileRules(context.CompiledRules);

                return CreateMapExpression(arg);
            }
            finally
            {
                if (!subFunction)
                    context.Running.Remove(tuple);
            }
        }

        internal LambdaExpression CreateMapInvokeExpression(TypeTuple tuple, MapType mapType)
        {
            return mapType == MapType.MapToTarget
                ? CreateMapToTargetInvokeExpression(tuple)
                : CreateMapInvokeExpression(tuple);
        }

        private LambdaExpression CreateMapInvokeExpression(TypeTuple tuple)
        {
            var p = Expression.Parameter(tuple.Source);
            var invoke = CreateMapInvokeExpressionBody(tuple, p);
            return Expression.Lambda(invoke, p);
        }

        internal Expression CreateMapInvokeExpressionBody(TypeTuple tuple, Expression p)
        {
            //var key = new TypeTuple(sourceType, destinationType);

            //if (GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMapping)) || GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMappingPrimitive)))
            //    Compiler(CreateMapExpression(key, MapType.Map));
           
            Expression? invoker;

            //Critical performance section
            if (TryGetInvokeGenericCacheMapFunc(tuple, out invoker)) { }

            //if (this.Config == MapsterGlobalConfig.GlobalSettings)
            //{
            //    var property = typeof(TypeAdapter<,>).MakeGenericType(sourceType, destinationType).GetField("Map");
            //    invoker = Expression.Field(null, property);
            //}

            // Fork Invoke call
            else
            {
                var method = (from m in typeof(ITypeAdapterMappingFuncProducer).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                              where m.Name == nameof(ITypeAdapterMappingFuncProducer.GetMapFunction)
                              select m).First().MakeGenericMethod(tuple.Source, tuple.Destination);
                invoker = Expression.Call(CreateSelfExpression(), method);
            }

            return Expression.Call(invoker!, "Invoke", null, p);
        }

        internal Expression CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2)
        {
            //if (GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMapping)) || GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMappingPrimitive)))
            //{
            //    var key = new TypeTuple(sourceType, destinationType);

            //    AddOrGetMapToTargetFuncFromCache(key, true);

            //   // _mapToTargetDict[key] = Compiler(CreateMapExpression(key, MapType.MapToTarget));
            //}
            var method = (from m in typeof(ITypeAdapterMappingFuncProducer).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          where m.Name == nameof(ITypeAdapterMappingFuncProducer.GetMapToTargetFunction)
                          select m).First().MakeGenericMethod(sourceType, destinationType);
            var invoker = Expression.Call(CreateSelfExpression(), method);
            return Expression.Call(invoker, "Invoke", null, p1, p2);
        }

        private LambdaExpression CreateMapToTargetInvokeExpression(TypeTuple tuple)
        {
            var p1 = Expression.Parameter(tuple.Source);
            var p2 = Expression.Parameter(tuple.Destination);
            var invoke = CreateMapToTargetInvokeExpressionBody(tuple.Source, tuple.Destination, p1, p2);
            return Expression.Lambda(invoke, p1, p2);
        }
                
        internal TypeAdapterSettings GetOvverideDefaultSettings(TypeTuple tuple, MapType mapType)
        {
            var arg = new PreCompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                MapType = mapType,
                ExplicitMapping = true,
            };

            var result = new TypeAdapterSettings();

            var rules = MapsterGlobalConfig.RulesTemplate.Reverse<TypeAdapterRule>();
            var settings = from rule in rules
                           let priority = rule.Priority(arg)
                           where priority != null
                           orderby priority.Value descending
                           select rule.Settings;
            foreach (var setting in settings)
            {
                result.Apply(setting);
            }

            return result;
        }

        internal CompileArgument GetCompileArgument(Type sourcetype, Type destintaiontype, MapType mapType, CompileContext context)
        {
            return GetCompileArgument(new TypeTuple(sourcetype, destintaiontype), mapType, context);
        }
        internal protected CompileArgument GetCompileArgument(TypeTuple tuple, MapType mapType, CompileContext context)
        {
            var setting = context.GetMergedSettings(tuple, mapType);
            return new CompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                ExplicitMapping = context.IsExplicitMapping(tuple),
                MapType = mapType,
                Context = context,
                Settings = setting,
            };
        }


        


        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void Compile(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);

            AddOrGetMapFuncFromCache(tuple, true);
            AddOrGetMapToTargetFuncFromCache(tuple, true);
            //_mapDict[tuple] = Compiler(CreateMapExpression(tuple, MapType.Map));
            //_mapToTargetDict[tuple] = Compiler(CreateMapExpression(tuple, MapType.MapToTarget));

            //Critical performance section
            InvalidateGenericCacheMapFunc(tuple);

            //if (this.Config == MapsterGlobalConfig.GlobalSettings)
            //{
            //    var field = typeof(TypeAdapter<,>).MakeGenericType(sourceType, destinationType).GetField("Map");
            //    field!.SetValue(null, _mapDict[tuple]);
            //}
        }

        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void CompileProjection(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);

            AddOrGetProjectionFuncFromCache(tuple, true);
            //_projectionDict[tuple] = CreateProjectionCallExpression(tuple);
        }
    }
}