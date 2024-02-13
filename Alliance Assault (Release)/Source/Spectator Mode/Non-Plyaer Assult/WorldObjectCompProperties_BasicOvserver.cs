using Non_Plyaer_Assault;
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
    public class WorldObjectCompProperties_BasicObserver:WorldObjectCompProperties
    {
        public WorldObjectCompProperties_BasicObserver()
        {
            compClass = typeof(BasicObserverComp);

        }
    }

    public class BasicObserverComp:WorldObjectComp
    {
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref IsObservation, "IsObservation", false);
        }

        public bool IsObservation=false;
    }
}
