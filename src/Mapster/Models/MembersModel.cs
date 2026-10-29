using System.Collections.Generic;
using System.Reflection;

namespace Mapster.Models
{
    internal class MembersModel
    {
        public MethodBase? ConstructorInfo { get; set; }
        public IEnumerable<IMemberModelEx> Members { get; set; } 
    }
}
