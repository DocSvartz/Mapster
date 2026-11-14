using Mapster.Cache;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using Mapster.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public class TypeAdapterConfigConcurrency : ITypeAdapterConfig
    {
        [Obsolete($"This will be removed in version 11. Use {nameof(MapsterGlobalConfig)}.RulesTemplate")]
        public static IReadOnlyCollection<TypeAdapterRule> RulesTemplate => MapsterGlobalConfig.RulesTemplate;

        [Obsolete($"This will be removed in version 11. Use {nameof(MapsterGlobalConfig)}.GlobalSettings")]
        public static TypeAdapterConfig GlobalSettings => MapsterGlobalConfig.GlobalSettings;
        
        private readonly TypeTuple _defaultSetterTuple = TypeTuple.Create<MapsterCacheMarker, DefaultOnlySetter>();

        public SettingsCacheStorage MappingCache { get; }

        public bool RequireDestinationMemberSource { get => MappingCache.GetBoolSetting(nameof(RequireDestinationMemberSource)); set => MappingCache.SetBoolSetting(nameof(RequireDestinationMemberSource),value); }
        public bool RequireExplicitMapping { get => MappingCache.GetBoolSetting(nameof(RequireExplicitMapping)); set => MappingCache.SetBoolSetting(nameof(RequireExplicitMapping), value); }
        public bool RequireExplicitMappingPrimitive { get => MappingCache.GetBoolSetting(nameof(RequireExplicitMappingPrimitive)); set => MappingCache.SetBoolSetting(nameof(RequireExplicitMappingPrimitive), value); }
        public bool AllowImplicitDestinationInheritance { get => MappingCache.GetBoolSetting(nameof(AllowImplicitDestinationInheritance)); set => MappingCache.SetBoolSetting(nameof(AllowImplicitDestinationInheritance), value); }
        public bool AllowImplicitSourceInheritance { get => MappingCache.GetBoolSetting(nameof(AllowImplicitSourceInheritance)); set => MappingCache.SetBoolSetting(nameof(AllowImplicitSourceInheritance), value); }
        public bool SelfContainedCodeGeneration { get => MappingCache.GetBoolSetting(nameof(SelfContainedCodeGeneration)); set => MappingCache.SetBoolSetting(nameof(SelfContainedCodeGeneration), value); }
        public bool ActivateCustomConvertersSrcNullPropagation { get => MappingCache.GetBoolSetting(nameof(ActivateCustomConvertersSrcNullPropagation)); set => MappingCache.SetBoolSetting(nameof(ActivateCustomConvertersSrcNullPropagation), value); }




        public Func<LambdaExpression, Delegate> Compiler { get; set; } = lambda => lambda.Compile();

        [Obsolete($"This will be removed in version 11. Use {nameof(MappingCache)}.Rules")]
        public IList<TypeAdapterRule> Rules => MappingCache.Rules.ToList();
        public DefaultOnlySetter Default => MappingCache.GetForTypeSetter(_defaultSetterTuple,this);
        public ConcurrentDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; internal set; } = new ConcurrentDictionary<TypeTuple, TypeAdapterRule>();

        public TypeAdapterConfigConcurrency()
        {
            MappingCache = new(MapsterGlobalConfig.RulesTemplate, this);
            MappingCache.GetNewConfigSetter(_defaultSetterTuple, this);
            MappingCache.SetBoolSetting(nameof(AllowImplicitSourceInheritance), true);
        }

        internal TypeAdapterConfigConcurrency(TypeAdapterConfigSnapshot snapshot)
        {
            MappingCache = new(snapshot, this);
        }

        /// <summary>
        /// allows you to specify conditions for when a mapping should occur based on source and destination types and the mapping type.
        /// </summary>
        /// <param name="canMap"></param>
        /// <returns></returns>
        public TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap)
        {
            var rule = new TypeAdapterRule
            {
                Priority = arg => canMap(arg.SourceType, arg.DestinationType, arg.MapType) ? (int?)25 : null
            };
            //Rules.LockAdd(rule);
            MappingCache.AddRule(rule);
            return new TypeAdapterSetter((TypeAdapterSettings)rule.Settings, this);
        }


		/// <summary>
		/// allows you to specify conditions for when a mapping should occur based on PreCompileArgument delegate
		/// </summary>
		/// <param name="canMap"></param>
		/// <returns></returns>
		public TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap)
        {
            var rule = new TypeAdapterRule
            {
                Priority = arg => canMap(arg) ? (int?)25 : null
            };
            //Rules.LockAdd(rule);
            MappingCache.AddRule(rule);
            return new TypeAdapterSetter((TypeAdapterSettings)rule.Settings, this);
        }


		/// <summary>
		/// Creates a new configuration for mapping between source and destination types.
		/// </summary>
		/// <typeparam name="TSource">Source type.</typeparam>
		/// <typeparam name="TDestination">Destination type.</typeparam>
		/// <returns></returns>
		public TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>()
        {
           // Remove(typeof(TSource), typeof(TDestination));
            return MappingCache.GetNewConfigSetter<TSource, TDestination>(TypeTuple.Create<TSource,TDestination>(),this);
        }


		/// <summary>
		/// Creates a new configuration for mapping between source and destination types.
		/// </summary>
		/// <param name="sourceType">Source type to create new configuration.</param>
		/// <param name="destinationType">Destination type to create new configuration.</param>
		/// <returns></returns>
		public TypeAdapterSetter NewConfig(Type sourceType, Type destinationType)
        {
            //Remove(sourceType, destinationType);
            return MappingCache.GetNewConfigSetter(new TypeTuple(sourceType, destinationType), this);
        }


		/// <summary>
		/// Configures a mapping for a specific source and destination type pair.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TDestination"></typeparam>
		/// <returns></returns>
		public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
          //  var key = new TypeTuple(typeof(TSource), typeof(TDestination));
           // var settings = GetSettings(key);
            return MappingCache.GetForTypeSetter<TSource, TDestination>(new TypeTuple(typeof(TSource), typeof(TDestination)), this);
        }


		/// <summary>
		/// Configures a mapping for a specific source and destination type pair.
		/// </summary>
		/// <param name="sourceType"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
           // var key = new TypeTuple(sourceType, destinationType);
           // var settings = GetSettings(key);
            return MappingCache.GetForTypeSetter(new TypeTuple(sourceType, destinationType), this);
        }


		/// <summary>
		/// Configures a mapping for a specific destination type.
		/// </summary>
		/// <typeparam name="TDestination">Destination type.</typeparam>
		/// <returns></returns>
		public TypeAdapterSetter<TDestination> ForDestinationType<TDestination>()
        {
            //var key = new TypeTuple(typeof(void), typeof(TDestination));
            //var settings = GetSettings(key);
            return MappingCache.GetForDestinationTypeSetter<TDestination>(new TypeTuple(typeof(void), typeof(TDestination)), this);
        }


		/// <summary>
		/// Configures a mapping for a specific destination type.
		/// </summary>
		/// <param name="destinationType">Destination type.</param>
		/// <returns></returns>
		public DestinationTypeOnlySetter ForDestinationType(Type destinationType)
        {
            //var key = new TypeTuple(typeof(void), destinationType);
            //var settings = GetSettings(key);
            return MappingCache.GetForDestinationTypeSetter(new TypeTuple(typeof(void), destinationType), this);
        }
        
        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="failFast">A boolean parameter that determines whether exceptions should be thrown immediately when mapping errors occur or whether to collect and aggregate them. The default value is true.</param>
        /// <exception cref="AggregateException"></exception>
        public void Compile(bool failFast = true)
        {
            var exceptions = new List<Exception>();
            var conf = GetSnapshot;

            var keys = conf.RuleMap.Keys.ToList();

            foreach (var key in keys)
            {
                try
                {
                    if (key.Source == typeof(void))
                        continue;

                    if (key.Source.ContainsGenericParameters || key.Destination.ContainsGenericParameters)
                        continue;
                    Compiler(conf.CreateMapExpression(key, MapType.Map));
                    Compiler(conf.CreateMapExpression(key, MapType.MapToTarget));
                    //_mapDict[key] = Compiler(CreateMapExpression(key, MapType.Map));
                    //_mapToTargetDict[key] = Compiler(CreateMapExpression(key, MapType.MapToTarget));
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
            

            Compiler(GetSnapshot.CreateMapExpression(tuple, MapType.Map));
            Compiler(GetSnapshot.CreateMapExpression(tuple, MapType.MapToTarget));
        }


		/// <summary>
		/// Validates and cache mapping instructions for queryable.
		/// </summary>
		public void CompileProjection()
        {
            var keys = MappingCache.GetRuleMap.Keys.ToList(); ;
            foreach (var key in keys)
            {
               // _projectionDict[key] = CreateProjectionCallExpression(key);
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
          //  _projectionDict[tuple] = CreateProjectionCallExpression(tuple);
        }


		/// <summary>
		/// Scans and registers mappings from specified assemblies.
		/// </summary>
		/// <param name="assemblies">assemblies to scan.</param>
		/// <returns>A list of registered mappings</returns>
		public IList<IRegister> Scan(params Assembly[] assemblies)
        {
            List<IRegister> registers = assemblies.Select(assembly => assembly.GetLoadableTypes()
                .Where(x => typeof(IRegister).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo()) && x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract))
                .SelectMany(registerTypes =>
                    registerTypes.Select(registerType => (IRegister)Activator.CreateInstance(registerType))).ToList();

            Apply(registers);
            return registers;
        }


		/// <summary>
		/// Applies type mappings.
		/// </summary>
		/// <param name="registers">collection of IRegister interface to apply mapping.</param>
		public void Apply(IEnumerable<Lazy<IRegister>> registers)
        {
            Apply(registers.Select(register => register.Value));
        }


		/// <summary>
		/// Applies type mappings.
		/// </summary>
		/// <param name="registers">collection of IRegister interface to apply mapping.</param>
		public void Apply(IEnumerable<IRegister> registers)
        {
            foreach (IRegister register in registers)
            {
                register.Register(this);
            }
        }


		/// <summary>
		/// Applies type mappings.
		/// </summary>
		/// <param name="registers">IRegister interface params to apply mapping.</param>
		public void Apply(params IRegister[] registers)
        {
            foreach (IRegister register in registers)
            {
                register.Register(this);
            }
        }


		/// <summary>
		/// Clears all type mapping rules and settings
		/// </summary>
		internal void Clear()
        {
            MappingCache.Clear();
        }


		/// <summary>
		/// Removes a specific type mapping rule.
		/// </summary>
		/// <param name="sourceType">Source type to remove.</param>
		/// <param name="destinationType">Destination type to remove.</param>
		internal void Remove(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            MappingCache.Remove(key);
        }

        private void Remove(TypeTuple key)
        {
            ////if (RuleMap.TryRemove(key, out var rule))
            ////    Rules.LockRemove(rule);
            //_mapDict.TryRemove(key, out _);
            //_mapToTargetDict.TryRemove(key, out _);
            //_projectionDict.TryRemove(key, out _);
            //_dynamicMapDict.TryRemove(key, out _);
        }

        //private s

        //private static readonly Lazy<TypeAdapterConfig> _cloneConfig = new Lazy<TypeAdapterConfig>(() =>
        //{
        //    return new TypeAdapterConfig(this.GetSnapshot);
        //});


		/// <summary>
		/// Clones the current TypeAdapterConfig.
		/// </summary>
		/// <returns></returns>
		public ITypeAdapterConfig Clone()
        {
            //var fn = _cloneConfig;
            //return fn(this);

            return new TypeAdapterConfigConcurrency(this.GetSnapshot);

           // throw new NotImplementedException();
        }

     

        public ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action,
#if !NET40
            [CallerFilePath]
#endif
            string key1 = "",
#if !NET40
            [CallerLineNumber]
#endif
            int key2 = 0)
        {
            var key = $"{key1}|{key2}";
           

            var cloned = Clone();
                action(cloned);
                return cloned;

            // throw new NotImplementedException();
        }

        public TypeAdapterConfigSnapshot GetSnapshot => MappingCache.CurrentSnapshot;
    }
}