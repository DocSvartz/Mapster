using ExpressionDebugger;
using ExpressionDebugger.Helpers.GeneratedAttributes;
using Mapster;
using Mapster.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Security;
using System.Reflection;

namespace TemplateTest
{
    [TestClass]
    public class CreateMapExpressionTest
    {
        [TestMethod]
        public void TestCreateMapExpression()
        {
            TypeAdapterConfig.GlobalSettings.SelfContainedCodeGeneration = true;
            var foo = default(Customer);
            var def = new ExpressionDefinitions
            {
                IsStatic = true,
                MethodName = "Map",
                Namespace = "Benchmark",
                TypeName = "CustomerMapper"
            };
            var code = foo.BuildAdapter()
                .CreateMapExpression<CustomerDTO>()
                .ToScript(def);

            Assert.IsNotNull(code);
        }

        [TestMethod]
        public void TestCreateMapToTargetExpression()
        {
            TypeAdapterConfig.GlobalSettings.SelfContainedCodeGeneration = true;
            var foo = default(Customer);
            var def = new ExpressionDefinitions
            {
                IsStatic = true,
                MethodName = "Map",
                Namespace = "Benchmark",
                TypeName = "CustomerMapper"
            };
            var code = foo.BuildAdapter()
                .CreateMapToTargetExpression<CustomerDTO>()
                .ToScript(def);

            Assert.IsNotNull(code);
        }

        [TestMethod]
        public void TestCreateProjectionExpression()
        {
            TypeAdapterConfig.GlobalSettings.SelfContainedCodeGeneration = true;
            var foo = default(Customer);
            var def = new ExpressionDefinitions
            {
                IsStatic = true,
                MethodName = "Map",
                Namespace = "Benchmark",
                TypeName = "CustomerMapper"
            };
            var code = foo.BuildAdapter()
                .CreateProjectionExpression<CustomerDTO>()
                .ToScript(def);

            Assert.IsNotNull(code);
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/399
        /// </summary>
        [TestMethod]
        public void TestRegressionMapperGenerationTranslation()
        {
            var S = new MapsterToolGeneratedMapperAttribute(true);

            var config = new TypeAdapterConfig();
            config.SelfContainedCodeGeneration = true;
           
            var definitions = new TypeDefinitions
            {
                Implements = new[] { typeof(IMyTypeMapper) },
                Namespace = "Benchmark",
                TypeName = "CustomerMapper",
                IsInternal = false,
                GeneratedAttributes = new(new[] {new MapsterToolGeneratedMapperAttribute()})
            };

            var translator = new ExpressionTranslator(definitions);

            foreach (var method in typeof(IMyTypeMapper).GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)
                .Where(x=>x.IsPublicOrInternal()))
            {
                if (method.IsGenericMethod)
                    continue;
                if (method.ReturnType == typeof(void))
                    continue;
                var methodArgs = method.GetParameters();
                if (methodArgs.Length < 1 || methodArgs.Length > 2)
                    continue;
                var tuple = new TypeTuple(methodArgs[0].ParameterType, method.ReturnType);
                var expr = config.CreateMapExpression(
                    tuple,
                    methodArgs.Length == 1 ? MapType.Map : MapType.MapToTarget
                );
                translator.VisitLambdaForGenerateMappers(
                    expr,
                    ExpressionTranslator.LambdaType.PublicMethod,
                    typeof(IMyTypeMapper),
                    method.Name,
                    !method.IsPublic
                );
            }

            foreach (var prop in typeof(IMyTypeMapper).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x=>x.IsGetterPublicOrInternal())
                )
            {
                if (!prop.PropertyType.IsGenericType)
                    continue;
                if (prop.PropertyType.GetGenericTypeDefinition() != typeof(Expression<>))
                    continue;
                var propArgs = prop.PropertyType.GetGenericArguments()[0];
                if (!propArgs.IsGenericType)
                    continue;
                if (propArgs.GetGenericTypeDefinition() != typeof(Func<,>))
                    continue;
                var funcArgs = propArgs.GetGenericArguments();
                var tuple = new TypeTuple(funcArgs[0], funcArgs[1]);
                var expr = config.CreateMapExpression(tuple, MapType.Projection);
                translator.VisitLambdaForGenerateMappers(
                    expr,
                    ExpressionTranslator.LambdaType.PublicLambda,
                    typeof(IMyTypeMapper),
                    prop.Name,
                    !prop.GetMethod?.IsPublic ?? false
                );
            }
                      

            var txt = translator.ToString();

            var src = new Address() { City = "City 17"};

            IMyTypeMapper mapper = new CustomerMapper();

            var result = mapper.Map(src);


            Assert.IsTrue(txt.Contains("Expression<Func<AddressDTO, Address>> TemplateTest.IMyTypeMapper.Projection"));
            Assert.IsTrue(txt.Contains("AddressDTO TemplateTest.IMyTypeMapper.Map"));
            Assert.IsTrue(txt.Contains("[MapsterToolGeneratedMapper]"));
        }

    }

   

    public partial class CustomerMapper : IMyTypeMapper
    {
        
        AddressDTO TemplateTest.IMyTypeMapper.Map(Address p1)
        {
            return p1 == null ? null : new AddressDTO()
            {
                Id = p1.Id,
                City = p1.City,
                Country = p1.Country
            };
        }
        
        Expression<Func<AddressDTO, Address>> TemplateTest.IMyTypeMapper.Projection => p2 => new Address()
        {
            Id = p2.Id,
            City = p2.City,
            Country = p2.Country
        };
    }




    public static class MethodInfoExtensions
    {
        public static bool IsPublicOrInternal(this MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            return !method.IsPrivate
                   && !method.IsFamily
                   && !method.IsFamilyOrAssembly
                   && !method.IsFamilyAndAssembly
                   && (method.IsPublic || true);
        }

       

        public static bool IsGetterPublicOrInternal(this PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            MethodInfo? getMethod = property.GetMethod;

            if (getMethod == null) return false;

            return !getMethod.IsPrivate
                   && !getMethod.IsFamily
                   && !getMethod.IsFamilyOrAssembly
                   && !getMethod.IsFamilyAndAssembly
                   && (getMethod.IsPublic || true);
        }
    }














    public interface IMyTypeMapper
    {
        internal AddressDTO Map(Address p1);
        internal Expression<Func<AddressDTO, Address>> Projection { get; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class AddressDTO
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Credit { get; set; }
        //public Address Address { get; set; }
        //public Address HomeAddress { get; set; }
        //public Address[] Addresses { get; set; }
        //public ICollection<Address> WorkAddresses { get; set; }
    }

    public class CustomerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AddressDTO Address { get; set; }
        public AddressDTO HomeAddress { get; set; }
        public AddressDTO[] Addresses { get; set; }
        public List<AddressDTO> WorkAddresses { get; set; }
        public string AddressCity { get; set; }
    }
}