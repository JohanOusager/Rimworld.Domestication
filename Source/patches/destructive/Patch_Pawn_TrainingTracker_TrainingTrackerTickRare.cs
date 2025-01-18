using Verse;
using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

// Pawn_TrainingTracker public void TrainingTrackerTickRare()
[HarmonyPatch(typeof(Pawn_TrainingTracker), "TrainingTrackerTickRare")]
public static class Patch_Pawn_TrainingTracker_TrainingTrackerTickRare
{
    // Cache the field info for performance
    private static readonly FieldInfo stepsField = typeof(Pawn_TrainingTracker).GetField("steps", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo countDecayFromField = typeof(Pawn_TrainingTracker).GetField("countDecayFrom", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo learnedField = typeof(Pawn_TrainingTracker).GetField("learned", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPrefix]
    public static bool ReplaceTrainingTrackerTickRare(Pawn_TrainingTracker __instance)
    {
        if (!WildnessUtility.CanDomesticate(__instance.pawn))
        {
            return true; // run the original function
        }
        else
        {
            // Access private fields using reflection
            var steps = (DefMap<TrainableDef, int>)stepsField.GetValue(__instance);
            var countDecayFrom = (int)countDecayFromField.GetValue(__instance);
            var learned = (DefMap<TrainableDef, bool>)learnedField.GetValue(__instance);

            if (__instance.pawn.Suspended)
            {
                countDecayFrom += 250;
            }
            else if (!__instance.pawn.Spawned)
            {
                countDecayFrom += 250;
            }
            else if (steps[TrainableDefOf.Tameness] == 0)
            {
                countDecayFrom = Find.TickManager.TicksGame;
            }
            else
            {
                // replaced TrainableUtility.DegradationPeriodTicks(pawn.def)
                if (Find.TickManager.TicksGame < countDecayFrom + TrainableUtilityPawn.TrainingDegradationPeriodTicks(__instance.pawn) ||
                    __instance.pawn.RaceProps.animalType == AnimalType.Dryad)
                {
                    return false; // Don't execute the original method
                }

                TrainableDef trainableDef = (from kvp in steps
                                             where kvp.Value > 0
                                             select kvp.Key)
                                             .Except(steps
                                             .Where((KeyValuePair<TrainableDef, int> kvp) => kvp.Value > 0 && kvp.Key.prerequisites != null)
                                             .SelectMany((KeyValuePair<TrainableDef, int> kvp) => kvp.Key.prerequisites))
                                             .RandomElement();

                if (trainableDef == TrainableDefOf.Tameness && !TrainableUtility.TamenessCanDecay(__instance.pawn.def))
                {
                    countDecayFrom = Find.TickManager.TicksGame;
                    return false; // Don't execute the original method
                }

                countDecayFrom = Find.TickManager.TicksGame;
                steps[trainableDef] -= 1;

                if (steps[trainableDef] > 0 || !learned[trainableDef])
                {
                    return false; // Don't execute the original method
                }

                learned[trainableDef] = false;

                if (__instance.pawn.Faction == Faction.OfPlayer)
                {
                    if (trainableDef == TrainableDefOf.Tameness)
                    {
                        __instance.pawn.SetFaction(null);
                        Messages.Message("MessageAnimalReturnedWild".Translate(__instance.pawn.LabelShort, __instance.pawn), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                    }
                    else
                    {
                        Messages.Message("MessageAnimalLostSkill".Translate(__instance.pawn.LabelShort, trainableDef.LabelCap, __instance.pawn.Named("ANIMAL")), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }

            // Update the field value using reflection
            countDecayFromField.SetValue(__instance, countDecayFrom);

            return false; // Don't execute the original method
        }
    }
}
