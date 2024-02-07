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
    public class WorldObjectCompProperties_FactionWarObserver:WorldObjectCompProperties
    {
        public WorldObjectCompProperties_FactionWarObserver()
        {
            compClass = typeof(FactionWarObserverComp);

        }

    }

    public class FactionWarObserverComp : WorldObjectComp
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            base.GetGizmos();
            MapParent mapParent = parent as MapParent;
            if (mapParent.HasMap)
            {
                yield return AllianceAssultUtility.StopObservingFactionalWar(mapParent);
            }
            else
            {
                yield return AllianceAssultUtility.ObserveFactionalWar(mapParent);
            }

        }
    }
}
