using RimWorld;
using Verse;
using HarmonyLib;
using System.Runtime;
using UnityEngine;


namespace Non_Plyaer_Assault
{
    public class NonPlayerAssault : Mod
    {
        public NonPlayerAssault(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("NonPlayerAssaultRelease");
            harmonyInstance.PatchAll();
            settings = base.GetSettings<AllianceAssaultSettings>();
        }

        public override string SettingsCategory() 
        {
            return "AllianceAssaultRelease".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Label("AssaultPoints".Translate() + " :" + settings.AssaultPoints, -1f, "AssaultPoints.Explained".Translate());
            settings.AssaultPoints
                = (int)listing_Standard.Slider(settings.AssaultPoints, 1000, 5000);
            //listing_Standard.CheckboxLabeled("ControllableUnits".Translate(),ref settings.ControllableUnits);
            listing_Standard.End();
            settings.Write();
            base.DoSettingsWindowContents(inRect);
        }
        public static Harmony harmonyInstance;
        public static AllianceAssaultSettings settings;

    }

}
