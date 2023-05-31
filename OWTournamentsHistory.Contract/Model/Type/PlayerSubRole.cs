using System.Runtime.Serialization;

namespace OWTournamentsHistory.Contract.Model.Type
{

    public enum PlayerSubRole
    {
        [EnumMember(Value = "Main tank")]
        MainTank = 0,
        [EnumMember(Value = "Off tank")]
        OffTank = 1,
        [EnumMember(Value = "Hitscan dps")]
        HitscanDps = 2,
        [EnumMember(Value = "Projectile dps")]
        ProjectileDps = 3,
        [EnumMember(Value = "Main heal")]
        MainHeal = 4,
        [EnumMember(Value = "Light heal")]
        LightHeal = 5
    }
}
