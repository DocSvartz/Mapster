using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mapster.Tests;

[TestClass]
public class WhenMappingMemberNameContainingPeriod
{
    private const string MemberName = "Some.Property.With.Periods";

    [TestMethod]
    public void Property_Name_Containing_Periods_Is_Supported()
    {
        // Create a target type with a property that contains periods
        Type targetType = new TestTypeBuilder()
            .AddProperty<int>(MemberName)
            .CreateType();

        // Call the local function defined below, the actual test method
        CallStaticLocalTestMethod(
            nameof(Test),
            new Type[] { targetType });

        // The actual test method adapting Source to the target type and back to the source to verify mapping the property with periods
        static void Test<TTarget>()
        {
            // Get expression for mapping the property with periods
            Expression<Func<TTarget, int>> getPropertyExpression = BuildGetPropertyExpression<TTarget, int>(MemberName);

            // Create the config
            TypeAdapterConfig<Source, TTarget>
                .NewConfig()
                .TwoWays()
                .Map(getPropertyExpression, src => src.Value);

            // Execute the mapping both ways
            Source source = new() { Value = 551 };
            TTarget target = source.Adapt<TTarget>();
            Source adaptedSource = target.Adapt<Source>();

            Assert.AreEqual(source.Value, adaptedSource.Value);
        }
    }

    [TestMethod]
    public void Constructor_Parameter_Name_Containing_Periods_Is_Supported()
    {
        // Create a target type with a property that contains periods
        Type targetTypeWithProperty = new TestTypeBuilder()
            .AddProperty<int>(MemberName)
            .CreateType();

        // Create a target type with a constructor parameter that contains periods
        Type targetTypeWithConstructor = new TestTypeBuilder()
            .AddConstructorWithReadOnlyProperty<int>(MemberName)
            .CreateType();

        // Call the local function defined below, the actual test method
        CallStaticLocalTestMethod(
            nameof(Test),
            new Type[] { targetTypeWithProperty, targetTypeWithConstructor });

        // The actual test method
        static void Test<TWithProperty, TWithConstructor>()
            where TWithProperty : new()
        {
            // Create the config
            TypeAdapterConfig<TWithProperty, TWithConstructor>
                .NewConfig()
                .TwoWays()
                .MapToConstructor(true);

            // Create delegate for setting the property value on TWithProperty
            Expression<Action<TWithProperty, int>> setPropertyExpression = BuildSetPropertyExpression<TWithProperty, int>(MemberName);
            Action<TWithProperty, int> setProperty = setPropertyExpression.Compile();

            // Create the source object
            int value = 551;
            TWithProperty source = new();
            setProperty.Invoke(source, value);

            // Map
            TWithConstructor target = source.Adapt<TWithConstructor>();
            TWithProperty adaptedSource = target.Adapt<TWithProperty>();

            // Create delegate for getting the property from TWithProperty
            Expression<Func<TWithProperty, int>> getPropertyExpression = BuildGetPropertyExpression<TWithProperty, int>(MemberName);
            Func<TWithProperty, int> getProperty = getPropertyExpression.Compile();

            // Verify
            Assert.AreEqual(value, getProperty.Invoke(adaptedSource));
        }
    }

    [TestMethod]
    public void Using_Property_Path_String_Is_Supported()
    {
        // Create a target type with a property that contains periods
        Type targetType = new TestTypeBuilder()
            .AddProperty<int>(MemberName)
            .CreateType();

        // Create the config, both ways
        TypeAdapterConfig
            .GlobalSettings
            .NewConfig(typeof(Source), targetType)
            .Map(MemberName, nameof(Source.Value));
        TypeAdapterConfig
            .GlobalSettings
            .NewConfig(targetType, typeof(Source))
            .Map(nameof(Source.Value), MemberName);

        // Execute the mapping both ways
        Source source = new() { Value = 551 };
        object target = source.Adapt(typeof(Source), targetType);
        Source adaptedSource = target.Adapt<Source>();

        Assert.AreEqual(source.Value, adaptedSource.Value);
    }

    private static void CallStaticLocalTestMethod(string methodName, Type[] genericArguments, [CallerMemberName] string caller = "Unknown")
    {
        MethodInfo genericMethodInfo = typeof(WhenMappingMemberNameContainingPeriod)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(x => x.Name.Contains($"<{caller}>") && x.Name.Contains(methodName));

        MethodInfo method = genericMethodInfo.MakeGenericMethod(genericArguments);

        method.Invoke(null, null);
    }

    private static Expression<Func<T, TProperty>> BuildGetPropertyExpression<T, TProperty>(string propertyName)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(param, propertyName);
        return Expression.Lambda<Func<T, TProperty>>(property, param);
    }

    private static Expression<Action<T, TProperty>> BuildSetPropertyExpression<T, TProperty>(string propertyName)
    {
        ParameterExpression param = Expression.Parameter(typeof(T), "x");
        ParameterExpression value = Expression.Parameter(typeof(TProperty), "value");
        MemberExpression property = Expression.Property(param, propertyName);
        BinaryExpression assign = Expression.Assign(property, value);
        return Expression.Lambda<Action<T, TProperty>>(assign, param, value);
    }

    private class Source
    {
        public int Value { get; set; }
    }

    private class TestTypeBuilder
    {
        private readonly TypeBuilder _typeBuilder;

        public TestTypeBuilder()
        {
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Types"),
                AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("<Module>");
            _typeBuilder = moduleBuilder.DefineType(
                "Types.Target",
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.Sealed |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);
        }

        public TestTypeBuilder AddConstructorWithReadOnlyProperty<TParameter>(string parameterName)
        {
            // Add read-only property
            FieldBuilder fieldBuilder = AddProperty<TParameter>(parameterName, false);

            // Build the constructor with the parameter for the property
            ConstructorBuilder constructorBuilder = _typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { typeof(TParameter) });

            // Define the parameter name
            constructorBuilder.DefineParameter(1, ParameterAttributes.None, MemberName);

            ILGenerator constructorIL = constructorBuilder.GetILGenerator();

            // Call the base class constructor
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

            // Set the property value
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_1);
            constructorIL.Emit(OpCodes.Stfld, fieldBuilder);

            constructorIL.Emit(OpCodes.Ret);

            return this;
        }

        public TestTypeBuilder AddProperty<T>(string propertyName)
        {
            AddProperty<T>(propertyName, true);
            return this;
        }

        private FieldBuilder AddProperty<T>(string propertyName, bool addSetter)
        {
            Type propertyType = typeof(T);
            FieldBuilder fieldBuilder = _typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

            AddGetMethod(_typeBuilder, propertyBuilder, fieldBuilder, propertyName, propertyType);
            if (addSetter)
            {
                AddSetMethod(_typeBuilder, propertyBuilder, fieldBuilder, propertyName, propertyType);
            }

            return fieldBuilder;
        }

        public Type CreateType() => _typeBuilder.CreateType();

        private static PropertyBuilder AddGetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, FieldBuilder fieldBuilder, string propertyName, Type propertyType)
        {
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(
                "get_" + propertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes);
            ILGenerator getMethodGenerator = getMethodBuilder.GetILGenerator();

            getMethodGenerator.Emit(OpCodes.Ldarg_0);
            getMethodGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);

            return propertyBuilder;
        }

        private static PropertyBuilder AddSetMethod(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, FieldBuilder fieldBuilder, string propertyName, Type propertyType)
        {
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(
                $"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new Type[] { propertyType });

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
}
