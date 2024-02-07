using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Non_Plyaer_Assault
{
    public class WorldObjectCompProperties_DestroyedSettlementObserver:WorldObjectCompProperties
    {
        public WorldObjectCompProperties_DestroyedSettlementObserver()
        {
            compClass = typeof(DestroyedSettlementObserverComp);

        }

    }

    public class DestroyedSettlementObserverComp : WorldObjectComp
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            base.GetGizmos();
            MapParent mapParent = parent as MapParent;
            if (mapParent.HasMap)
            {
                yield return AllianceAssultUtility.StopObservingDestroyedMapP(mapParent);
            }

        }

        public string settlementDefName="";
        public Faction victoryFaction;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<string>(ref settlementDefName, "settlementDef","");
            Scribe_References.Look(ref victoryFaction, "victoryFaction");
        }
    }
}
