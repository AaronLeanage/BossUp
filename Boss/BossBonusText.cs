using System.Collections.Generic;
using KitchenData;
using KitchenLib.Customs;

namespace BossUp.Boss
{
    /// <summary>
    /// The "Boss Bonus" row on the end-of-day money breakdown. The popup gives any money event whose
    /// identifier is a <see cref="MoneyTrackerText"/> its own named row; identifier 0 is lumped into
    /// the generic "other" row. The ID is a hash of the mod GUID and name, so it matches on every player.
    /// </summary>
    public class BossBonusText : CustomMoneyTrackerText
    {
        public override string UniqueNameID => "BossBonus";

        public override List<(Locale, BasicInfo)> InfoList => new List<(Locale, BasicInfo)>
        {
            (Locale.English, new BasicInfo { Name = "Boss Bonus", Description = "" })
        };
    }
}
