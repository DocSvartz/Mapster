using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public abstract class BaseTypeAdapterConfigDecorator : ITypeAdapterConfig
    {

        private readonly ITypeAdapterConfig _Config;

        public BaseTypeAdapterConfigDecorator(bool IsGlobal = false) : this(TypeAdapterConfigFactory.GlobalSettings.Clone(),IsGlobal)
        {
        }

        public BaseTypeAdapterConfigDecorator(ITypeAdapterConfig config, bool IsGlobal = false)
        {
            IsGlobalSettings = IsGlobal;
            _Config = config.Clone();
        }

        public bool AllowImplicitDestinationInheritance { get => _Config.AllowImplicitDestinationInheritance; set => _Config.AllowImplicitDestinationInheritance = value; }
        public bool AllowImplicitSourceInheritance { get => _Config.AllowImplicitSourceInheritance; set => _Config.AllowImplicitSourceInheritance = value; }

        public ConfigCompileStorage ConfigCompile => _Config.ConfigCompile;

        public bool IsGlobalSettings { get; protected set; }

        public bool RequireDestinationMemberSource { get => _Config.RequireDestinationMemberSource; set => _Config.RequireDestinationMemberSource = value; }
        public bool RequireExplicitMapping { get => _Config.RequireExplicitMapping; set => _Config.RequireExplicitMapping = value; }
        public bool RequireExplicitMappingPrimitive { get => _Config.RequireExplicitMappingPrimitive; set => _Config.RequireExplicitMappingPrimitive = value; }

        public ConcurrentDictionary<TypeTuple, TypeAdapterRule> RuleMap => _Config.RuleMap;


        public bool SelfContainedCodeGeneration { get => _Config.SelfContainedCodeGeneration; set => _Config.SelfContainedCodeGeneration = value; }
        public Func<LambdaExpression, Delegate> Compiler { get => _Config.Compiler; set => _Config.Compiler = value; }

        public void AddRule(TypeAdapterRule rule)
        {
            _Config.AddRule(rule);
        }

        public virtual void Apply(IEnumerable<IRegister> registers)
        {
            _Config.Apply(registers);
        }

        public virtual void Clear()
        {
            _Config.Clear();
        }

        public virtual ITypeAdapterConfig Clone()
        {
            return _Config.Clone();
        }

        public void Compile(bool failFast = true)
        {
            _Config.Compile(failFast);
        }

        public void Compile(Type sourceType, Type destinationType)
        {
            _Config.Compile(sourceType, destinationType);
        }

        public void CompileProjection()
        {
            _Config.CompileProjection();
        }

        public void CompileProjection(Type sourceType, Type destinationType)
        {
            _Config.CompileProjection(sourceType, destinationType);
        }

        public virtual LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType)
        {
            return _Config.CreateMapExpression(tuple, mapType);
        }

        public virtual ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0)
        {
            return _Config.Fork(action, key1, key2);
        }

        public virtual TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            return _Config.ForType(sourceType, destinationType);
        }

        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            return _Config.ForType<TSource, TDestination>();
        }

        public TypeAdapterSettings GetMergedSettings(TypeTuple tuple, MapType mapType)
        {
            return _Config.GetMergedSettings(tuple, mapType);
        }

        public IEnumerable<TypeAdapterRule> GetRules(Func<TypeAdapterRule, bool> predicate)
        {
            return _Config.GetRules(predicate);
        }

        public virtual void Remove(Type sourceType, Type destinationType)
        {
            _Config.Remove(sourceType, destinationType);
        }
    }
}
