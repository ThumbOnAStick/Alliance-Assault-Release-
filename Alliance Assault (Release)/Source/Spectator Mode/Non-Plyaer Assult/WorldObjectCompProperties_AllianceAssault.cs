using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using RimWar;

namespace Non_Plyaer_Assault
{
    public class WorldObjectCompProperties_AllianceAssult:WorldObjectCompProperties
    {
        public WorldObjectCompProperties_AllianceAssult()
        {
            compClass = typeof(AllianceAssaultComp);
        }
    }

    public class AllianceAssaultComp : WorldObjectComp
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            base.GetGizmos();
            MapParent mapParent = parent as MapParent;            
            if (!mapParent.Faction.HostileTo(Faction.OfPlayer))
            {
                yield break;
            }            
            if (mapParent.HasMap)
            {
                yield return AllianceAssultUtility.CancelAttackCommand(mapParent);
            }
            List<Faction> factions = Find.FactionManager.AllFactions
                .Where(x => x != Faction.OfPlayer && x.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally&&
                x.RelationKindWith(mapParent.Faction)==FactionRelationKind.Hostile).ToList();
            if (factions.Count < 1)
            {
                yield break;
            }
            yield return AllianceAssultUtility.AllianceAssaultCommand(mapParent);
        }

        public bool isAllianceAssault;
        public Faction ally;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref isAllianceAssault, "isAllianceAssault", false, false);
            Scribe_References.Look<Faction>(ref ally, "ally", false);
        }

    }


}
