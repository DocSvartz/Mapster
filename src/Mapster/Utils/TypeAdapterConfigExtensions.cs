using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        List<(int Index, List<int> RefOthetConstr, List<Type> ExtConstrains)> ConstraintsWithConstrain = new();

        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            var constraints = args.Where(x => x.IsGenericParameter)
                .Select(x=>x.GetGenericParameterConstraints()).ToList();

            if (!constraints.Any())
                return (true,type.GetGenericArguments());


            for (var i = 0; i < constraints.Count; i++)
            {
                if (constraints[i].Length == 0)
                {
                    genericParams.Add(typeof(object));
                    continue;
                }

                if (constraints[i].Any(x => x.IsGenericParameter))
                {
                    List<int> RefOthetConstr = new List<int>();
                    List<Type> ExtConstrains = new List<Type>();

                    foreach (var item in constraints[i])
                    {
                        if (item.IsGenericParameter)
                            RefOthetConstr.Add(Array.IndexOf(args, item));
                        else
                            ExtConstrains.Add(item);
                    }

                    ConstraintsWithConstrain.Add((i, RefOthetConstr, ExtConstrains));

                    genericParams.Add(typeof(Never));
                    continue;
                }


                if (constraints[i].Length == 1 && constraints[i][0] == typeof(ValueType))
                {
                    genericParams.Add(typeof(int));
                    continue;
                }

                if (constraints[i].Any(x => x == typeof(ValueType)))
                {
                    var cInterface = constraints[i].FirstOrDefault(x => x.IsInterface);

                    genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(ValueType), cInterface));
                    continue;
                }
                else
                {
                    var cClass = constraints[i].FirstOrDefault(x => x.IsClass);
                    var cInterface = constraints[i].FirstOrDefault(x => x.IsInterface);


                    if ((cInterface?.IsAssignableFrom(cClass)).GetValueOrDefault())
                    {
                        if (cClass.IsAbstract)
                        {
                            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(cClass, cInterface));
                            continue;
                        }

                        genericParams.Add(cClass);
                        continue;
                    }
                    else if (cClass != null && cInterface == null && cClass.IsVisible)
                    {
                        if (cClass.IsAbstract)
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
                    else if (cInterface != null && cClass == null)
                    {
                        if (!cInterface.IsVisible)
                            return (false, null);

                        genericParams.Add(cInterface);
                        continue;
                    }
                    
                    else
                        return (false, null);
                }
            }

        }


        if(ConstraintsWithConstrain.Count != 0)
        {
            foreach (var item in ConstraintsWithConstrain)
            {
                var result = item.RefOthetConstr
                    .Where(i =>  i >= 0 && i < genericParams.Count)
                    .Select(i => genericParams[i])
                    .Concat(item.ExtConstrains).ToList();

                if (result.Count == 1)
                    genericParams[item.Index] = result[0];
                else
                {

                    if(result.Any(x=>x.IsInterface) || result.Any(x => x.IsClass))
                    {
                        var onlyInterface = result.Where(x => x.IsInterface);
                        var onlyClass = result.Where(x => x.IsClass);


                        if(result.Any(x => x.IsInterface) && result.Any(x => x.IsClass))
                        {
                            foreach (var Interface in onlyInterface)
                            {
                                foreach (var res in result)
                                {
                                    if (Interface.IsAssignableFrom(res) && Interface != res)
                                    {
                                        genericParams[item.Index] = res;
                                        continue;
                                    }

                                }

                            }

                            if (genericParams[item.Index] == typeof(Never))
                                genericParams[item.Index] = DynamicTypeGenerator.GetTypeWitInterface(result.First(x => x.IsClass), result.FirstOrDefault(x => x.IsInterface));

                        }
                        else if(onlyClass.Any())
                        {
                            foreach (var classItem in onlyClass)
                            {
                                foreach (var res in result)
                                {
                                    if (classItem.IsAssignableFrom(res) && classItem != res)
                                    {
                                        genericParams[item.Index] = res;
                                        continue;
                                    }

                                }
                            }

                            if (genericParams[item.Index] == typeof(Never))
                                genericParams[item.Index] = DynamicTypeGenerator.GetTypeWitInterface(result.First(x => x.IsClass), result.FirstOrDefault(x => x.IsInterface));
                        }
                        else
                            return (false, null);

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