using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingNamedValueTuple
    {
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
            groupDM.Adapt(tuple);


        }

        

    }

    #region TestClasses
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

    public class GroupDetails604
    {
        public int GroupId { get; set; }
    }

    #endregion TestClasses
}
