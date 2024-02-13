using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Non_Plyaer_Assault
{
    public class AllianceAssaultSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref AssaultPoints, "AssaultPoints", 3000, true);
            Scribe_Values.Look<bool>(ref ControllableUnits, "ControllableUnits", false, true);

        }

        public int AssaultPoints;
        public bool ControllableUnits;
    }
}
