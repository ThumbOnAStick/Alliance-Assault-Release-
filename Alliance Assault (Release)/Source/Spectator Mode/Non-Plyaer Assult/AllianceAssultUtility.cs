using Non_Plyaer_Assault;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;
using Verse;
using RimWar;
using RimWar.Planet;
using static RimWorld.Reward_Pawn;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace Non_Plyaer_Assault
{

    [StaticConstructorOnStartup]
    public static class AllianceAssultUtility
    {
        public static bool HasAnyActiveAlly(MapParent settlement)
        {
            var mapPawns = settlement.Map.mapPawns;
            bool result = mapPawns.AllPawns.Any(x => !x.Downed && x.Faction != null && !x.Faction.HostileTo(Faction.OfPlayer));
            Log.Message(result.ToString());
            //return mapPawns.AllPawns.Any(x => !x.Downed && x.Faction != null && !x.Faction.HostileTo(Faction.OfPlayer));
            return result;

        }

        public static Command AllianceAssaultCommand(MapParent mapP)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandAllianceAssult".Translate();
            command_Action.defaultDesc = "CommandAllianceAssultDesc".Translate();
            command_Action.icon = AssultCommandTex;
            command_Action.action = delegate ()
            {
                OpenRaidFloatMenu(mapP);
            };
            command_Action.Order = 3000f;
            return command_Action;
        }


        public static Command CancelAttackCommand(MapParent mapP)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandCancelAttack".Translate();
            command_Action.icon = CancelAttackCommandTex;
            command_Action.action = delegate ()
            {
                if (mapP.Map != null && !mapP.Map.mapPawns.AnyColonistSpawned)
                {
                    var aa = mapP.GetComponent<AllianceAssaultComp>();
                    if (aa != null)
                        aa.isAllianceAssault = false;
                    Current.Game.DeinitAndRemoveMap_NewTemp(mapP.Map, false);
                }
            };
            return command_Action;
        }

        public static Command StopObservingDestroyedMapP(MapParent mapP)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandStopObserving".Translate();
            command_Action.icon = CancelAttackCommandTex;
            command_Action.action = delegate ()
            {
                var observer = mapP.GetComponent<DestroyedSettlementObserverComp>();
                if (observer == null)
                {
                    return;
                }

                if (mapP.Map == null || mapP.Map.mapPawns.AnyColonistSpawned)
                {
                    return;
                }
                
                Current.Game.DeinitAndRemoveMap_NewTemp(mapP.Map, false);
                if (observer.settlementDefName != "")
                {
                    var targetDef = DefDatabase<WorldObjectDef>.GetNamed(observer.settlementDefName);
                    if (targetDef.canHaveFaction == true)
                    {
                        WorldObject obj = WorldObjectMaker.MakeWorldObject(targetDef);
                        obj.Tile = mapP.Tile;
                        obj.SetFaction(observer.victoryFaction);
                        Log.Message(obj.Faction.ToString());
                    }
                    mapP.Destroy();

                }
            };

            return command_Action;
        }
        public static Command ObserveFactionalWar(MapParent mapP)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandObserver".Translate();
            command_Action.icon = mapP.ExpandingIcon;
            command_Action.action = delegate ()
            {
                ObserveFactionalWarAction(mapP);
            };

            return command_Action;
        }

        public static Command StopObservingFactionalWar(MapParent mapP)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandStopObserving".Translate();
            command_Action.icon = CancelAttackCommandTex;
            command_Action.action = delegate ()
            {
                if (mapP.Map != null && !mapP.Map.mapPawns.AnyFreeColonistSpawned)
                {
                    Current.Game.DeinitAndRemoveMap_NewTemp(mapP.Map, false);
                    mapP.Destroy();
                }
            };

            return command_Action;
        }
        public static void OpenRaidFloatMenu(MapParent mapP)
        {            
           
            List<FloatMenuOption> list = Options(mapP);            
            FloatMenu floatMenuMap = new FloatMenu(list);
            Find.WindowStack.Add(floatMenuMap);
        }

        public static List<FloatMenuOption> Options(MapParent mapP)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            Faction player = Faction.OfPlayer;
            List<Faction> allies = Find.FactionManager.AllFactions.Where(x => x != player && x.RelationKindWith(player) == FactionRelationKind.Ally).ToList();
            if (allies.Count < 1)
            {
                return list;
            }
            bool isCity = mapP.def.defName == CityName;
            foreach (var ally in allies)
            {
                if (isCity && ally.def.techLevel < TechLevel.Industrial)
                {
                    continue;
                }
                if (!ally.def.pawnGroupMakers.Any(x => x.kindDef == PawnGroupKindDefOf.Combat))
                {
                    continue;
                }
                FloatMenuOption allyOption =
                 new FloatMenuOption("", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                allyOption.autoTakeable = true;
                allyOption.Disabled = false;
                allyOption.autoTakeablePriority = 40f;
                allyOption.action = delegate ()
                {
                    List<FloatMenuOption> list2 = Options2(ally, mapP);
                    FloatMenu floatMenuMap = new FloatMenu(list2);
                    Find.WindowStack.Add(floatMenuMap);
                };
                allyOption.Label = ally.Name;
                list.Add(allyOption);
            }

            return list;
        }
        public static List<FloatMenuOption> Options2(Faction ally, MapParent mapP)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            TechLevel level = ally.def.techLevel;
            Log.Message(ally.def.defName);
            var allStrategies = DefDatabase<RaidStrategyDef>.AllDefs;
            foreach (var def in allStrategies)
            {
                if ((!ally.def.canSiege && def.defName == "Siege") || def.arrivalTextFriendly == null || def.arrivalTextFriendly == "")
                {
                    continue;
                }
                    
                IncidentParms parms = new IncidentParms();
                parms.points = NonPlayerAssault.settings.AssaultPoints;
                parms.faction = ally;
                if (!def.Worker.CanUseWith(parms, PawnGroupKindDefOf.Combat))
                {
                    continue;
                }
                    FloatMenuOption arrivalOption =
                new FloatMenuOption("", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                arrivalOption.autoTakeable = true;
                arrivalOption.Disabled = false;
                arrivalOption.autoTakeablePriority = 40f;
                arrivalOption.action = delegate ()
                {
                    List<FloatMenuOption> list2 = Options3(ally, mapP, def);
                    FloatMenu floatMenuMap = new FloatMenu(list2);
                    Find.WindowStack.Add(floatMenuMap);
                };
                arrivalOption.Label = def.defName;
                list.Add(arrivalOption);
            }
            return list;
        }
        public static List<FloatMenuOption> Options3(Faction ally, MapParent mapP, RaidStrategyDef strategyDef)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            TechLevel level = ally.def.techLevel;
            var arrivalDefs = strategyDef.arriveModes;
            foreach (var def in arrivalDefs)
            {
                if (def.minTechLevel > level||def.textFriendly==null||def.textFriendly=="")
                {
                    continue;
                }
                FloatMenuOption arrivalOption =
                 new FloatMenuOption("", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                arrivalOption.autoTakeable = true;
                arrivalOption.Disabled = false;
                arrivalOption.autoTakeablePriority = 40f;
                arrivalOption.action = delegate ()
                {
                    AllyAssult(ally, mapP, strategyDef, def);
                };
                arrivalOption.Label = def.defName;
                list.Add(arrivalOption);
            }
            return list;

        }
        public static void AllyAssult(Faction ally, MapParent mapP, RaidStrategyDef def, PawnsArrivalModeDef def1)
        {
            //Generate Map
            var aa = mapP.GetComponent<AllianceAssaultComp>();
            if (aa != null)
            {
                aa.isAllianceAssault = true;
                aa.ally = ally;
            }
            if (mapP.Map == null)
            {
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    GenerateMapAndAssault(ally, mapP, def, def1);
                }, "GeneratingMapForNewEncounter", false, null, true);

            }
            else
            {
                GenerateMapAndAssault(ally, mapP, def, def1);
            }
        }

        public static void ObserveFactionalWarAction(MapParent mapP)
        {
            LongEventHandler.QueueLongEvent(delegate ()
            {
                Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapP.Tile, null);
                Current.Game.CurrentMap = orGenerateMap;
            }, "GeneratingMapForNewEncounter", false, null, true);
        }

        public static void GenerateMapAndAssault(Faction ally, MapParent mapP, RaidStrategyDef def, PawnsArrivalModeDef def1)
        {
            if (mapP.Map == null)
            {
                GetOrGenerateMapUtility.GetOrGenerateMap(mapP.Tile, null);
            }
            Current.Game.CurrentMap = mapP.Map;
            TaggedString label = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString text;
            if (ally.def.techLevel < TechLevel.Industrial)
            {
                text = "LetterTribleAssaultEnemyBase".Translate().CapitalizeFirst();
            }
            else
            {
                text = "LetterAllyAssaultEnemyBase".Translate(mapP.Label).CapitalizeFirst();
            }
            bool flag = !mapP.HasMap;
            if (flag)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(mapP.Map.mapPawns.AllPawns, ref label, ref text, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
            }
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, null, mapP.Faction, null, null, null);

            //Affect Relations
            FactionRelation factionRelation = Faction.OfPlayer.RelationWith(ally, false);
            factionRelation.baseGoodwill = Mathf.Clamp(factionRelation.baseGoodwill - 50, -1, 100);
            FactionRelation factionRelation2 = ally.RelationWith(Faction.OfPlayer, false);
            factionRelation2.baseGoodwill = factionRelation.baseGoodwill;
            factionRelation2.kind = factionRelation.kind;

            //Ally Raid
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = mapP.Map;
            incidentParms.faction = ally;
            incidentParms.raidArrivalModeForQuickMilitaryAid = true;
            incidentParms.points = NonPlayerAssault.settings.AssaultPoints;
            incidentParms.raidStrategy = def;
            incidentParms.forced = true;
            incidentParms.raidArrivalMode = def1;
            //Decide Raid Delay
            int duration = 100;
            if (ally.def.techLevel < TechLevel.Industrial)
            {
                duration = 1000;
            }

            bool isCity = mapP.def.defName == CityName;
            //If Rimcities is loaded, force 10000 raid points
            if (isCity)
            {
                incidentParms.points = 10000;
            }

            //IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidFriendly, Find.TickManager.TicksGame + duration, incidentParms, 0);


            //If Rimcities is loaded, spawn another two raids.
            if (isCity)
            {
                var duration2 = duration + 10000;
                var duration3 = duration2 + 10000;
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidFriendly, Find.TickManager.TicksGame + duration2, incidentParms, 0);
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidFriendly, Find.TickManager.TicksGame + duration3, incidentParms, 0);

            }
        }

        public static void GenerateMapAndMechAssault(MapParent mapP)
        {
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapP.Tile, null);
            Current.Game.CurrentMap = orGenerateMap;
            TaggedString label = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString text= "LetterMechAssaultEnemyBase".Translate();
            bool flag = !mapP.HasMap;
            if (flag)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref label, ref text, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
            }
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, null, mapP.Faction, null, null, null);


            //Mech Raid
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = orGenerateMap;
            incidentParms.faction = Faction.OfMechanoids;
            incidentParms.points = NonPlayerAssault.settings.AssaultPoints;
            incidentParms.forced = true;

            //Decide Raid Delay
            int duration = 100;

            //IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + duration, incidentParms, 0); 
        }


        private static readonly Texture2D AssultCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/AllianceAssault", true);
        private static readonly Texture2D CancelAttackCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/AbandonHome", true);

        //RimCities
        private static readonly string CityName = "City_Faction";
    }
}
