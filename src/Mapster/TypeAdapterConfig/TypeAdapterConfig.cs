using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mapster
{
    public class TypeAdapterConfig : ITypeAdapterConfig, IConfigConcurrency
    {
        [AdaptIgnore]
        public bool IsGlobalSettings { get; private set; }
        public bool RequireDestinationMemberSource { get; set; }
        public bool RequireExplicitMapping { get; set; }
        public bool RequireExplicitMappingPrimitive { get; set; }
        public bool AllowImplicitDestinationInheritance { get; set; }
        public bool AllowImplicitSourceInheritance { get; set; } = true;
        public bool SelfContainedCodeGeneration { get; set; }
        public Func<LambdaExpression, Delegate> Compiler { get; set; } = lambda => lambda.Compile();
        
        [AdaptMember]
        private List<TypeAdapterRule> Rules { get; set; }
        public ConcurrentDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; internal set; } = new ConcurrentDictionary<TypeTuple, TypeAdapterRule>();
        
        [AdaptIgnore]
        public ConfigCompileStorage ConfigCompile { get; private set; }

        public bool ConcurrencyEnvironment {  get;  set; }
        
        [AdaptIgnore]
        public AutoResetEvent Configure { get; private set; }
      
        internal TypeAdapterConfig(bool IsGlobal) : this()
        {
            IsGlobalSettings = IsGlobal;
        }

        public TypeAdapterConfig()
        {
            Configure = new(true);
            Rules = TypeAdapterConfigFactory.RulesTemplate.ToList();
            var settings = new TypeAdapterSettings();
            ConfigCompile = new ConfigCompileStorage(this);
            Rules.Add(new TypeAdapterRule
            {
                Priority = arg => -100,
                Settings = settings,
            });
        }


        /// <summary>
        /// Configures a mapping for a specific source and destination type pair.
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            var settings = this.GetSettings(key);
            return new TypeAdapterSetter(settings, this);
        }

        /// <summary>
        /// Configures a mapping for a specific source and destination type pair.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            var key = new TypeTuple(typeof(TSource), typeof(TDestination));
            var settings = this.GetSettings(key);
            return new TypeAdapterSetter<TSource, TDestination>(settings, this);
        }

        public LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType)
        {
            if (ConcurrencyEnvironment)
            {
                Configure.WaitOne(-1);
            }
            
            var context = new CompileContext(this);
            context.Running.Add(tuple);
            Action<ITypeAdapterConfig>? fork = null;
            try
            {
                var arg = GetCompileArgument(tuple, mapType, context);
                fork = arg.Settings.Fork;
                if (fork != null)
                {
                    var cloned = Clone();
                    fork(cloned);
                    context.Configs.Push(cloned);
                    arg.Settings = cloned.GetMergedSettings(tuple, mapType);
                }
                return arg.CreateMapExpression();
            }
            finally
            {
                if (ConcurrencyEnvironment)
                {
                    Configure.Set();
                }
                    
                if (fork != null)
                    context.Configs.Pop();
                context.Running.Remove(tuple);
            }
        }

        private CompileArgument GetCompileArgument(TypeTuple tuple, MapType mapType, CompileContext context)
        {
            var setting = this.GetMergedSettings(tuple, mapType);
            return new CompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                ExplicitMapping = RuleMap.ContainsKey(tuple),
                MapType = mapType,
                Context = context,
                Settings = setting,
            };
        }


        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="failFast">A boolean parameter that determines whether exceptions should be thrown immediately when mapping errors occur or whether to collect and aggregate them. The default value is true.</param>
        /// <exception cref="AggregateException"></exception>
        public void Compile(bool failFast = true)
        {
            ConfigCompile.Compile(failFast);
        }


        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void Compile(Type sourceType, Type destinationType)
        {
            ConfigCompile.Compile(sourceType, destinationType);
        }


        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        public void CompileProjection()
        {
            ConfigCompile.CompileProjection();
        }


        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void CompileProjection(Type sourceType, Type destinationType)
        {
            ConfigCompile.CompileProjection(sourceType, destinationType);
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
        /// Clears all type mapping rules and settings
        /// </summary>
        public void Clear()
        {
            var keys = RuleMap.Keys.ToList();
            foreach (var key in keys)
            {
                Remove(key);
            }
        }


        /// <summary>
        /// Removes a specific type mapping rule.
        /// </summary>
        /// <param name="sourceType">Source type to remove.</param>
        /// <param name="destinationType">Destination type to remove.</param>
        public void Remove(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            Remove(key);
        }

        private void Remove(TypeTuple key)
        {
            if (RuleMap.TryRemove(key, out var rule))
                Rules.LockRemove(rule);
            ConfigCompile.Remove(key);
        }

        private static readonly Lazy<ITypeAdapterConfig> _cloneConfig = new Lazy<ITypeAdapterConfig>(() =>
        {
            var config = new TypeAdapterConfig();
            config.Default().Settings.PreserveReference = true;
            config.ForType<TypeAdapterSettings, TypeAdapterSettings>()
                .MapWith(src => src.Clone(), true);
            return config;
        });


        /// <summary>
        /// Clones the current TypeAdapterConfig.
        /// </summary>
        /// <returns></returns>
        public ITypeAdapterConfig Clone()
        {
            var fn = _cloneConfig.Value.ConfigCompile.GetMapFunction<TypeAdapterConfig, TypeAdapterConfig>();
            return fn(this);
        }

        private ConcurrentDictionary<string, ITypeAdapterConfig>? _inlineConfigs;
        private ConcurrentDictionary<string, ITypeAdapterConfig> InlineConfigs =>
            _inlineConfigs ??= new ConcurrentDictionary<string, ITypeAdapterConfig>();

        bool IConfigConcurrency.IsScanConcurrency { get; set; }

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
            return InlineConfigs.GetOrAdd(key, _ =>
            {
                var cloned = Clone();
                action(cloned);
                return cloned;
            });
        }

        public TypeAdapterSettings GetMergedSettings(TypeTuple tuple, MapType mapType)
        {
            var arg = new PreCompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                MapType = mapType,
                ExplicitMapping = RuleMap.ContainsKey(tuple),
            };

            //auto add setting if there is attr setting
            var attrSettings =this.GetAttributeSettings(tuple, mapType).ToList();
            if (!arg.ExplicitMapping && attrSettings.Any(rule => rule.Priority(arg) == 100))
            {
                this.GetSettings(tuple);
                arg.ExplicitMapping = true;
            }

            var result = new TypeAdapterSettings();
            lock (Rules)
            {
                var rules = Rules.Reverse<TypeAdapterRule>().Concat(attrSettings);
                var settings = from rule in rules
                               let priority = rule.Priority(arg)
                               where priority != null
                               orderby priority.Value descending
                               select rule.Settings;
                foreach (var setting in settings)
                {
                    result.Apply(setting);
                }
            }

            //remove recursive include types
            if (mapType == MapType.MapToTarget)
                result.Includes.Remove(tuple);
            else
                result.Includes.RemoveAll(t => t.Source == tuple.Source);
            return result;
        }


        public void AddRule(TypeAdapterRule rule)
        {
            Rules.LockAdd(rule);
        }

        public IEnumerable<TypeAdapterRule> GetRules (Func<TypeAdapterRule, bool> predicate)
        {
            return Rules.Where(predicate);
        }
    }

    public static class TypeAdapterConfig<TSource, TDestination>
    {
		/// <summary>
		///  Creates a new configuration for mapping between the source and destination types.
		/// </summary>
		/// <returns></returns>
		public static TypeAdapterSetter<TSource, TDestination> NewConfig()
        {
            if (TypeAdapterConfigFactory.GlobalSettings is IConfigConcurrency config)
                config.ConcurrencyEnvironment = false;

            return TypeAdapterConfigFactory.GlobalSettings.NewConfig<TSource, TDestination>();
        }


		/// <summary>
		/// Creates a configuration for mapping between the source and destination types.
		/// </summary>
		/// <returns></returns>
		public static TypeAdapterSetter<TSource, TDestination> ForType()
        {
            if (TypeAdapterConfigFactory.GlobalSettings is IConfigConcurrency config)
                config.ConcurrencyEnvironment = false;

            return TypeAdapterConfigFactory.GlobalSettings.ForType<TSource, TDestination>();
        }


		/// <summary>
		/// Clears the type mapping configuration for the specified source and destination types.
		/// </summary>
		public static void Clear()
        {
            TypeAdapterConfigFactory.GlobalSettings.Remove(typeof(TSource), typeof(TDestination));
        }
    }

    public static class TypeAdapterConfigConcurrency<TSource, TDestination>
    {
        /// <summary>
        ///  Creates a new configuration for mapping between the source and destination types.
        /// </summary>
        /// <returns></returns>
        public static TypeAdapterSetter<TSource, TDestination> NewConfig()
        {
            if (TypeAdapterConfigFactory.GlobalSettings is IConfigConcurrency config)
                config.ConcurrencyEnvironment = true;

            return TypeAdapterConfigFactory.GlobalSettings.NewConfig<TSource, TDestination>();
        }


        /// <summary>
        /// Creates a configuration for mapping between the source and destination types.
        /// </summary>
        /// <returns></returns>
        public static TypeAdapterSetter<TSource, TDestination> ForType()
        {
            if (TypeAdapterConfigFactory.GlobalSettings is IConfigConcurrency config)
                config.ConcurrencyEnvironment = true;
            return TypeAdapterConfigFactory.GlobalSettings.ForType<TSource, TDestination>();
        }


        /// <summary>
        /// Clears the type mapping configuration for the specified source and destination types.
        /// </summary>
        public static void Clear()
        {
            TypeAdapterConfigFactory.GlobalSettings.Remove(typeof(TSource), typeof(TDestination));
        }
    }

    internal interface IConfigConcurrency
    {
        public bool ConcurrencyEnvironment { get; set; }

        public bool IsScanConcurrency { get; set; }
    }
}