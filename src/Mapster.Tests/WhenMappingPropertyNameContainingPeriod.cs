using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mapster.Tests;

[TestClass]
public class WhenMappingPropertyNameContainingPeriod
{
    private const string PropertyName = "Some.Property.With.Periods";

    [TestMethod]
    public void Property_Name_Containing_Periods_Is_Supported()
    {
        Type targetType = CreateTargetType();

        MethodInfo genericMethod = GetType()
            .GetMethod(nameof(MapPropertyWithPeriod), BindingFlags.NonPublic | BindingFlags.Static)!;
        MethodInfo method = genericMethod.MakeGenericMethod(targetType);
        method.Invoke(null, null);
    }

    private static void MapPropertyWithPeriod<TTarget>()
    {
        Expression<Func<TTarget, int>> targetPropertyExpression = CreatePropertyExpression<TTarget, int>(PropertyName);

        TwoWaysTypeAdapterSetter<Source, TTarget> config = TypeAdapterConfig<Source, TTarget>
            .NewConfig()
            .TwoWays()
            .Map(targetPropertyExpression, src => src.Value);

        config.DestinationToSourceSetter.Compile();
    }
    class Source
    {
        public int Value { get; set; }
    }

    private static Type CreateTargetType()
    {
        string assemblyName = "Types";
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(assemblyName),
            AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("<Module>");

        TypeBuilder typeBuilder = moduleBuilder.DefineType(
            "Types.Target",
            TypeAttributes.Public |
            TypeAttributes.Class |
            TypeAttributes.Sealed |
            TypeAttributes.AutoClass |
            TypeAttributes.AnsiClass |
            TypeAttributes.BeforeFieldInit |
            TypeAttributes.AutoLayout,
            null);

        AddProperty(typeBuilder, PropertyName, typeof(int));

        Type type = typeBuilder.CreateType();
        return type;
    }

    private static Expression<Func<T, TProperty>> CreatePropertyExpression<T, TProperty>(string propertyName)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(parameter, propertyName);
        return Expression.Lambda<Func<T, TProperty>>(property, parameter);
    }

    private static void AddProperty(TypeBuilder typeBuilder, string name, Type type)
    {
        FieldBuilder fieldBuilder = typeBuilder.DefineField($"_{name}", type, FieldAttributes.Private);
        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);
        AddGetMethod(typeBuilder, propertyBuilder, fieldBuilder, name, type);
        AddSetMethod(typeBuilder, propertyBuilder, fieldBuilder, name, type);
    }

    private static PropertyBuilder AddGetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, FieldBuilder fieldBuilder, string name, Type type)
    {
        MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
            "get_" + name,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            type,
            Type.EmptyTypes);
        ILGenerator getMethodGenerator = getMethodBuilder.GetILGenerator();

        getMethodGenerator.Emit(OpCodes.Ldarg_0);
        getMethodGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
        getMethodGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMethodBuilder);

        return propertyBuilder;
    }

    private static PropertyBuilder AddSetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, FieldBuilder fieldBuilder, string name, Type type)
    {
        MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
            $"set_{name}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null,
            new Type[] { type });

        ILGenerator setMethodGenerator = setMethodBuilder.GetILGenerator();
        Label modifyProperty = setMethodGenerator.DefineLabel();
        Label exitSet = setMethodGenerator.DefineLabel();

        setMethodGenerator.MarkLabel(modifyProperty);
        setMethodGenerator.Emit(OpCodes.Ldarg_0);
        setMethodGenerator.Emit(OpCodes.Ldarg_1);
        setMethodGenerator.Emit(OpCodes.Stfld, fieldBuilder);

        setMethodGenerator.Emit(OpCodes.Nop);
        setMethodGenerator.MarkLabel(exitSet);
        setMethodGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetSetMethod(setMethodBuilder);

        return propertyBuilder;
    }
}
