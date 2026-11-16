using Mapster.Cache;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public class FrozenTypeAdapterConfig : ITypeAdapterConfig
    {
        public bool ActivateCustomConvertersSrcNullPropagation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AllowImplicitDestinationInheritance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AllowImplicitSourceInheritance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool RequireDestinationMemberSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool RequireExplicitMapping { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool RequireExplicitMappingPrimitive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool SelfContainedCodeGeneration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Func<LambdaExpression, Delegate> Compiler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap => throw new NotImplementedException();

        public IList<TypeAdapterRule> Rules => throw new NotImplementedException();

        public SettingsCacheStorage MappingCache => throw new NotImplementedException();

        public DefaultOnlySetter Default => throw new NotImplementedException();

        public ITypeAdapterMappingFuncProducer GetSnapshot => throw new NotImplementedException();

        public void Apply(IEnumerable<IRegister> registers)
        {
            throw new NotImplementedException();
        }

        public void Apply(IEnumerable<Lazy<IRegister>> registers)
        {
            throw new NotImplementedException();
        }

        public void Apply(params IRegister[] registers)
        {
            throw new NotImplementedException();
        }

        public ITypeAdapterConfig Clone()
        {
            throw new NotImplementedException();
        }

        public void Compile(bool failFast = true)
        {
            throw new NotImplementedException();
        }

        public void Compile(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public void CompileProjection()
        {
            throw new NotImplementedException();
        }

        public void CompileProjection(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public DestinationTypeOnlySetter ForDestinationType(Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TDestination> ForDestinationType<TDestination>()
        {
            throw new NotImplementedException();
        }

        public ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter NewConfig(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>()
        {
            throw new NotImplementedException();
        }

        public IList<IRegister> Scan(params Assembly[] assemblies)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap)
        {
            throw new NotImplementedException();
        }
    }
}
