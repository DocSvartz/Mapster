using System;

namespace Mapster.Enums
{
    [Flags]
    public enum DestinationMemebers
    {
        None = 0,
        PropertyOrFields = 1,
        ConstrutorMembers = 2,
        AllMembers = PropertyOrFields|ConstrutorMembers,
    }

    public static class DestinationMemebersHelpers
    {
        public static bool HasPropertyOrFields(this DestinationMemebers input)
        {
            return (input & DestinationMemebers.PropertyOrFields) == DestinationMemebers.PropertyOrFields;
        }

        public static bool HasConstrutorMembers(this DestinationMemebers input)
        {
            return (input & DestinationMemebers.ConstrutorMembers) == DestinationMemebers.ConstrutorMembers;
        }
    }
}
