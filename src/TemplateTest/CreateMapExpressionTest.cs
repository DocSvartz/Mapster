using ExpressionDebugger;
using Mapster;
using Mapster.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Xml.Linq;

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

            var config = new TypeAdapterConfig();

            config.SelfContainedCodeGeneration = true;
            var foo = default(Customer);

            var definitions = new TypeDefinitions
            {
                Implements = new[] { typeof(IMyTypeMapper) },
                Namespace = "Benchmark",
                TypeName = "CustomerMapper",
                IsInternal = true,
            };

            var translator = new ExpressionTranslator(definitions);

            foreach (var method in typeof(IMyTypeMapper).GetMethods())
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
                translator.VisitLambdaInterface(
                    expr,
                    ExpressionTranslator.LambdaType.PublicMethod,
                    typeof(IMyTypeMapper).FullName + "." + method.Name
                );
            }

            var txt = translator.ToString();
        }
    }


    internal partial class CustomerMapper : IMyTypeMapper
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
    }




    internal interface IMyTypeMapper
    {
        AddressDTO Map(Address p1);
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    internal class AddressDTO
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
        public Address Address { get; set; }
        public Address HomeAddress { get; set; }
        public Address[] Addresses { get; set; }
        public ICollection<Address> WorkAddresses { get; set; }
    }

    internal class CustomerDTO
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