using Mapster.Models;
using System.Collections.Generic;

namespace Mapster.MappingModels
{
    internal abstract class BaseConveterModel
    {
        public IEnumerable<MemberMapping> Members { get; set; }
    }
}
