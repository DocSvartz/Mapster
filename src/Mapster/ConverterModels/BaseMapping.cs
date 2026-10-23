using Mapster.Models;
using System.Collections.Generic;

namespace Mapster.MappingModels
{
    internal abstract class BaseMapping
    {
        public IEnumerable<MemberMapping> Members { get; set; }
    }
}
