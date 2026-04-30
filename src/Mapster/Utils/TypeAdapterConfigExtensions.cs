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

                if(item.Length == 1 && item[0] == typeof(ValueType))
                {
                    genericParams.Add(typeof(int));
                    continue;
                }

                if (!item.Contains(typeof(ValueType)) && !item.Any(x=>x.IsGenericParameter))
                {
                    var cClass = item.FirstOrDefault(x => x.IsClass);
                    var cInterface = item.FirstOrDefault(x => x.IsInterface);


                    if ((cInterface?.IsAssignableFrom(cClass)).GetValueOrDefault())
                    {
                        if(cClass.IsAbstract)
                            return (false, null);

                        genericParams.Add(cClass);
                        continue;
                    }
                    else if(cInterface != null && cInterface.IsVisible)
                    {
                        genericParams.Add(cInterface);
                        continue;
                    }
                    else
                        return (false, null);

                   

                }
                else
                    return(false, null);

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