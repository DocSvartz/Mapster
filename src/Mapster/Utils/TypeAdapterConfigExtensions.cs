using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Mapster.Models;

namespace Mapster.Utils;

public static class TypeAdapterConfigExtensions
{
    public static void ScanInheritedTypes(this TypeAdapterConfig config, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t =>
                t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
            .ToList();
        InterfaceDynamicMapper dynamicMapper = new(config, types);
        dynamicMapper.ApplyMappingFromAssembly();
    }

    internal static void ScanInheritedTypes(this TypeAdapterConfig config, List<Type> types)
    {
        types = types.Where(t =>
                t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
            .ToList();
        InterfaceDynamicMapper dynamicMapper = new(config, types);
        dynamicMapper.ApplyMappingFromAssembly();
    }

    public static bool HasRuleFor(this TypeAdapterConfig config, Type srcType, Type dstType) =>
        config.RuleMap.ContainsKey(new TypeTuple(srcType, dstType));

    public static (bool isSuccess,Type[]? Result) GetOpenGenericTypeParamsStubs(this Type type)
    {
        var genericParams = new List<Type>();

        if (type.IsGenericType)
        {
            var constraints = type.GetGenericArguments().Where(x => x.IsGenericParameter)
                .Select(x=>x.GetGenericParameterConstraints());

            if (!constraints.Any())
                return (true,type.GetGenericArguments());

            foreach (var item in constraints)
            {
                if (item.Length == 0)
                {
                    genericParams.Add(typeof(object));
                    continue;
                }

                if (item.Any(x => x.IsGenericParameter))
                    return (false, null);

                if (item.Length == 1 && item[0] == typeof(ValueType))
                {
                    genericParams.Add(typeof(int));
                    continue;
                }

                if(item.Any(x => x == typeof(ValueType) || x.IsValueType))
                {
                    var cStruct = item.FirstOrDefault(x => x == typeof(ValueType) || x.IsValueType);
                    var cInterface = item.FirstOrDefault(x => x.IsInterface);

                    if(cStruct == typeof(ValueType))
                    {
                        genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(cStruct, cInterface));
                        continue;
                    }
                    genericParams.Add(cStruct);
                    continue;

                }
                else
                {
                    var cClass = item.FirstOrDefault(x => x.IsClass);
                    var cInterface = item.FirstOrDefault(x => x.IsInterface);


                    if ((cInterface?.IsAssignableFrom(cClass)).GetValueOrDefault())
                    {
                        if(cClass.IsAbstract)
                        {
                            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(cClass, cInterface));
                            continue;
                        }
                    
                        genericParams.Add(cClass);
                        continue;
                    }
                    else if (cInterface != null && cClass != null && cInterface.IsVisible)
                    {
                        genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(cClass, cInterface));
                        continue;
                    }
                    else if(cInterface != null && cClass == null)
                    {
                        if(!cInterface.IsVisible)
                            return (false, null);

                        genericParams.Add(cInterface);
                        continue;
                    }
                    else
                        return (false, null);
                }
            }
        }


        return (true,genericParams.ToArray());
    }

    internal static void ForDestinationTypeRegsiter(this TypeAdapterConfig cfg, Type[] destTypes)
    {
        foreach (var item in destTypes)
        {
            cfg.ForDestinationType(item).DirectAssignmentForSameType(true);
        }
    }
}