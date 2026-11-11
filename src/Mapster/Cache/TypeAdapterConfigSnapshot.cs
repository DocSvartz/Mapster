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
    public partial class TypeAdapterConfigSnapshot
    {
        public TypeAdapterConfig Config { get; init; }

        internal TypeAdapterRule DefaultSettingsRule;
        internal ImmutableArray<TypeAdapterRule> AdapterRules { get; init;}
        internal ImmutableDictionary<TypeTuple, TypeAdapterRule> RuleMap;
        internal ImmutableDictionary<string, bool> BoolSettings;
        internal Func<LambdaExpression, Delegate> Compiler { get; init; }

        public ulong Version { get; init; }
        public bool GetBoolSetting(string name) { BoolSettings.TryGetValue(name, out var data); return data; }
        public IReadOnlyCollection<TypeAdapterRule> Rules => this;

        private T AddToHash<T>(ConcurrentDictionary<TypeTuple, T> hash, TypeTuple key, Func<TypeTuple, T> func)
        {
            return hash.GetOrAdd(key, types =>
            {
                var del = func(types);
                hash[types] = del;

                if (RuleMap.TryGetValue(types, out var rule))
                    rule.Settings.Compiled = true;
                return del;

            });
        }

       // private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        public Func<TSource, TDestination> GetMapFunction<TSource, TDestination>()
        {
            return (Func<TSource, TDestination>)GetMapFunction(typeof(TSource), typeof(TDestination));
        }
        internal Delegate GetMapFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_mapDict.TryGetValue(key, out var del))
                del = AddToHash(_mapDict, key, tuple => Compiler(CreateMapExpression(tuple, MapType.Map)));
            return del;
        }

        //private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapToTargetDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        public Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>()
        {
            return (Func<TSource, TDestination, TDestination>)GetMapToTargetFunction(typeof(TSource), typeof(TDestination));
        }
        internal Delegate GetMapToTargetFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_mapToTargetDict.TryGetValue(key, out var del))
                del = AddToHash(_mapToTargetDict, key, tuple => Compiler(CreateMapExpression(tuple, MapType.MapToTarget)));
            return del;
        }

        //private readonly ConcurrentDictionary<TypeTuple, MethodCallExpression> _projectionDict = new ConcurrentDictionary<TypeTuple, MethodCallExpression>();
        internal Expression<Func<TSource, TDestination>> GetProjectionExpression<TSource, TDestination>()
        {
            var del = GetProjectionCallExpression(typeof(TSource), typeof(TDestination));

            return (Expression<Func<TSource, TDestination>>)((UnaryExpression)del.Arguments[1]).Operand;
        }
        internal MethodCallExpression GetProjectionCallExpression(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_projectionDict.TryGetValue(key, out var del))
                del = AddToHash(_projectionDict, key, CreateProjectionCallExpression);
            return del;
        }

        //private readonly ConcurrentDictionary<TypeTuple, Delegate> _dynamicMapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        public Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType)
        {
            var key = new TypeTuple(sourceType, typeof(TDestination));
            if (!_dynamicMapDict.TryGetValue(key, out var del))
                del = AddToHash(_dynamicMapDict, key, tuple => Compiler(CreateDynamicMapExpression(tuple)));
            return (Func<object, TDestination>)del;
        }

        private Expression CreateSelfExpression()
        {
            //if (this == GlobalSettings)
            //    return Expression.Property(null, typeof(TypeAdapterConfig).GetProperty(nameof(GlobalSettings))!);
            //else
                return Expression.Constant(this);
        }

        internal Expression CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1)
        {
            var method = (from m in typeof(TypeAdapterConfigSnapshot).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          where m.Name == nameof(GetDynamicMapFunction)
                          select m).First().MakeGenericMethod(destinationType);
            var getType = typeof(object).GetMethod(nameof(GetType));
            var invoker = Expression.Call(CreateSelfExpression(), method, Expression.Call(p1, getType!));
            return Expression.Call(invoker, "Invoke", null, p1);
        }

        public LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType)
        {
            var context = new CompileContext(this);
            context.Running.Add(tuple);
            Action<TypeAdapterConfig>? fork = null;
            try
            {
                var arg = GetCompileArgument(tuple, mapType, context);
                fork = arg.Settings.Fork;
                if (fork != null)
                {
                    var cloned = Config.Clone();
                    fork(cloned);
                    context.Configs.Push(cloned.GetSnapshot);
                    arg.Settings = context.GetMergedSettings(tuple, mapType); // rebuild
                }

               // this.MappingCache.CollectCompileRules(context.CompiledRules);

                return CreateMapExpression(arg);
            }
            finally
            {
                context.AttributeMetadata.Complete();
                if (fork != null)
                    context.Configs.Pop();
                context.Running.Remove(tuple);
            }
        }

        private MethodCallExpression CreateProjectionCallExpression(TypeTuple tuple)
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

        private static LambdaExpression CreateMapExpression(CompileArgument arg)
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

        private LambdaExpression CreateDynamicMapExpression(TypeTuple tuple)
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
                    return CreateMapInvokeExpression(sourceType, destinationType, mapType);
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

                    if (mapping.OverrideSettings != null)
                    {
                        mapping.OverrideSettings.Apply(arg.Settings);

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

        internal LambdaExpression CreateMapInvokeExpression(Type sourceType, Type destinationType, MapType mapType)
        {
            return mapType == MapType.MapToTarget
                ? CreateMapToTargetInvokeExpression(sourceType, destinationType)
                : CreateMapInvokeExpression(sourceType, destinationType);
        }

        private LambdaExpression CreateMapInvokeExpression(Type sourceType, Type destinationType)
        {
            var p = Expression.Parameter(sourceType);
            var invoke = CreateMapInvokeExpressionBody(sourceType, destinationType, p);
            return Expression.Lambda(invoke, p);
        }

        internal Expression CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p)
        {
            if (GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMapping)) || GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMappingPrimitive)))
            {
                var key = new TypeTuple(sourceType, destinationType);
                Compiler(CreateMapExpression(key, MapType.Map));
            }
            Expression invoker;
            //if (this == GlobalSettings)
            //{
            //    var field = typeof(TypeAdapter<,>).MakeGenericType(sourceType, destinationType).GetField("Map");
            //    invoker = Expression.Field(null, field);
            //}
            //else
            //{
                var method = (from m in typeof(TypeAdapterConfigSnapshot).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                              where m.Name == nameof(GetMapFunction)
                              select m).First().MakeGenericMethod(sourceType, destinationType);
                invoker = Expression.Call(CreateSelfExpression(), method);
            //}
            return Expression.Call(invoker, "Invoke", null, p);
        }

        internal Expression CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2)
        {
            if (GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMapping)) || GetBoolSetting(nameof(TypeAdapterConfig.RequireExplicitMappingPrimitive)))
            {
                var key = new TypeTuple(sourceType, destinationType);
                _mapToTargetDict[key] = Compiler(CreateMapExpression(key, MapType.MapToTarget));
            }
            var method = (from m in typeof(TypeAdapterConfigSnapshot).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          where m.Name == nameof(GetMapToTargetFunction)
                          select m).First().MakeGenericMethod(sourceType, destinationType);
            var invoker = Expression.Call(CreateSelfExpression(), method);
            return Expression.Call(invoker, "Invoke", null, p1, p2);
        }

        private LambdaExpression CreateMapToTargetInvokeExpression(Type sourceType, Type destinationType)
        {
            var p1 = Expression.Parameter(sourceType);
            var p2 = Expression.Parameter(destinationType);
            var invoke = CreateMapToTargetInvokeExpressionBody(sourceType, destinationType, p1, p2);
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
        private CompileArgument GetCompileArgument(TypeTuple tuple, MapType mapType, CompileContext context)
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

                Source = Expression.Parameter(tuple.Source, "p1"),
                Destination = Expression.Parameter(tuple.Destination, "p2"),
                Result = Expression.Parameter(tuple.Destination, "result")
            };
        }


        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="failFast">A boolean parameter that determines whether exceptions should be thrown immediately when mapping errors occur or whether to collect and aggregate them. The default value is true.</param>
        /// <exception cref="AggregateException"></exception>
        public void Compile(bool failFast = true)
        {
            var exceptions = new List<Exception>();
            var keys = RuleMap.Keys.ToList();

            foreach (var key in keys)
            {
                try
                {
                    if (key.Source == typeof(void))
                        continue;

                    if (key.Source.ContainsGenericParameters || key.Destination.ContainsGenericParameters)
                        continue;

                    _mapDict[key] = Compiler(CreateMapExpression(key, MapType.Map));
                    _mapToTargetDict[key] = Compiler(CreateMapExpression(key, MapType.MapToTarget));
                }
                catch (Exception ex)
                {
                    if (failFast)
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }


        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void Compile(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            _mapDict[tuple] = Compiler(CreateMapExpression(tuple, MapType.Map));
            _mapToTargetDict[tuple] = Compiler(CreateMapExpression(tuple, MapType.MapToTarget));
            //if (this == GlobalSettings)
            //{
            //    var field = typeof(TypeAdapter<,>).MakeGenericType(sourceType, destinationType).GetField("Map");
            //    field!.SetValue(null, _mapDict[tuple]);
            //}
        }


        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        public void CompileProjection()
        {
            var keys = RuleMap.Keys.ToList();
            foreach (var key in keys)
            {
                _projectionDict[key] = CreateProjectionCallExpression(key);
            }
        }


        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void CompileProjection(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            _projectionDict[tuple] = CreateProjectionCallExpression(tuple);
        }
    }
}