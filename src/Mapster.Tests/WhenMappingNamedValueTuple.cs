using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Drawing;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingNamedValueTuple
    {
        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/604
        /// </summary>
        [TestMethod]
        public void MappedNamedTuple()
        {
            TypeAdapterConfig<GroupDM604, (AuthorizationGroup604 authorizationGroup, GroupDetails604 groupDetails)>
            .NewConfig()
            .Map(dest => dest.authorizationGroup, src => src)
            .Map(dest => dest.groupDetails, src => src);

            GroupDM604 groupDM = new() { Name = "Hello world", Description = "Jon Bon Jovi" };

            // Create a new instance of (AuthorizationGroup, GroupDetails): This works
            var tuple = groupDM.Adapt<(AuthorizationGroup604 authorizationGroup, GroupDetails604 groupDetails)>();

            // Now lets update poco
            groupDM.GroupId = 99;
            groupDM.Description = "Led Zep";

            //Now lets update tuple: Doesn't work
            var c =   groupDM.Adapt(tuple);


            c.groupDetails.GroupId.ShouldBe(99);
            c.authorizationGroup.Description.ShouldBe("Led Zep");
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/568
        /// </summary>
        [TestMethod]
        public void TupleAndPoco()
        {
            TypeAdapterConfig<A568, (int Id, string Name)>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

            TypeAdapterConfig<(int Id, string Name),A568>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

            var pocoTo = new A568() { Name = "Me", Id = 2 };
            var pocoDest = new A568() { Name = "You", Id = 3 };

            (int Id, string Name) tupleTO = (4, "John");
            (int Id, string Name) tupleDest = (5, "Name");


            var _tupleResult = pocoTo.Adapt(tupleDest); // _tupleResult  = {2, Me}
            var _pocoResult = tupleTO.Adapt(pocoDest); // _pocoResult = {4, John}
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/501
        /// </summary>
        [TestMethod]
        public void MultiplyTuple()
        {

            TypeAdapterConfig<(Source501, Dest501), DestRecord501>.NewConfig()
                    .Map(dest => dest, src => src.Item1)
                    .Map(dest => dest, src => src.Item2);

            var src1 = new Source501 { MyInt = 1 };
            var src2 = new Dest501 { MyString = "Hello" };

            //work
            var destTuple = ValueTuple.Create(src1, src2).Adapt<DestRecord501>();

           //Works
            var destSrc1 = src1.Adapt<DestRecord501>();
            var destSrc2 = src2.Adapt<DestRecord501>();
        }

    }




    #region TestClasses

    internal sealed class Source501
    {
        public int MyInt { get; init; }
    }

    public class Dest501
    {
        public string MyString { get; init; }
    }

    public record DestRecord501
    {
        public DestRecord501(int myInt)
        {
            MyInt = myInt;
        }

        public DestRecord501(int myInt, string myString) : this(myInt)
        {
            MyString = myString;
        }

        public int MyInt { get; init; }
        public string MyString { get; init; }
    }


    class A568
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GroupDM604
    {
        public int GroupId;
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class AuthorizationGroup604
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public record GroupDetails604
    {
        public int GroupId { get; set; }
    }

    #endregion TestClasses
}
