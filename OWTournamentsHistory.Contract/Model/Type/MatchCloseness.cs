using System.Runtime.Serialization;

namespace OWTournamentsHistory.Contract.Model.Type
{
    public enum MatchCloseness
    {
        [EnumMember(Value = "Very close")]
        VeryClose = 0,
        [EnumMember(Value = "Close")]
        Close = 1,
        [EnumMember(Value = "Equal")]
        Equal = 2,
        [EnumMember(Value = "One side dominated")]
        OneSideFavorite = 3,
        [EnumMember(Value = "One-sided")]
        OneSided = 4,
    }
}
