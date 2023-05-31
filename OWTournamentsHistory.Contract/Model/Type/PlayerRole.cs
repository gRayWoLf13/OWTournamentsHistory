using System.Runtime.Serialization;

namespace OWTournamentsHistory.Contract.Model.Type
{
    public enum PlayerRole
    {
        [EnumMember(Value = "Flex")]
        Flex = 0,
        [EnumMember(Value = "Tank")]
        Tank = 1,
        [EnumMember(Value = "Dps")]
        Dps = 2,
        [EnumMember(Value = "Support")]
        Support = 3
    }
}
