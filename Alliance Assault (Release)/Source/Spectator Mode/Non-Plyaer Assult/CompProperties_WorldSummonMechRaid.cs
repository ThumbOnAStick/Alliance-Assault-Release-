using Non_Plyaer_Assault;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Non_Plyaer_Assault
{
    public class CompProperties_WorldSummonMechRaid : CompProperties
    {
        // Token: 0x060083ED RID: 33773 RVA: 0x002D0D8F File Offset: 0x002CEF8F
        public CompProperties_WorldSummonMechRaid()
        {
            compClass = typeof(CompWorldSummonMechRaid);
        }

    }

    public class CompWorldSummonMechRaid : ThingComp
    {
        public override void Notify_AbandonedAtTile(int tile)
        {

            var settlement = Find.World.worldObjects.SettlementAt(tile);
            if (settlement == null)
            {
                return;
            }
            if (settlement.Map != null)
            {
                return;
            }
            if (!settlement.Faction.HostileTo(Faction.OfPlayer))
            {
                return;
            }
            var aa = settlement.GetComponent<AllianceAssaultComp>();
            if (aa == null || aa.isAllianceAssault)
            {
                return;
            }
            aa.isAllianceAssault = true;
            aa.ally = Faction.OfMechanoids;
            LongEventHandler.QueueLongEvent(delegate ()
            {
                AllianceAssultUtility.GenerateMapAndMechAssault(settlement);
            }, "GeneratingMapForNewEncounter", false, null, true);

        }


    }
}
