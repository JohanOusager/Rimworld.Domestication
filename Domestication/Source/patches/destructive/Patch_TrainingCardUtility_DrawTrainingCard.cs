using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse.AI;

[HarmonyPatch(typeof(TrainingCardUtility), "DrawTrainingCard")]
public static class Patch_TrainingCardUtility_DrawTrainingCard
{
    // Cache FieldInfo for private static fields TrainabilityLeft and TrainabilityTop
    private static readonly FieldInfo trainabilityLeftField = typeof(TrainingCardUtility).GetField("TrainabilityLeft", BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly FieldInfo trainabilityTopField = typeof(TrainingCardUtility).GetField("TrainabilityTop", BindingFlags.NonPublic | BindingFlags.Static);

    // Cache MethodInfo for the private static TryDrawTrainableRow method
    private static readonly MethodInfo tryDrawTrainableRowMethod = typeof(TrainingCardUtility).GetMethod("TryDrawTrainableRow", BindingFlags.NonPublic | BindingFlags.Static);

    [HarmonyPrefix]
    public static bool ReplaceDrawTrainingCard(Rect rect, Pawn pawn)
    {
        if (!WildnessUtility.CanDomesticate(pawn))
        {
            return true; // run the original func instead
        }
        else
        {
            // Access TrainabilityLeft and TrainabilityTop via reflection
            float trainabilityLeft = (float)trainabilityLeftField.GetValue(null);
            float trainabilityTop = (float)trainabilityTopField.GetValue(null);

            Text.Font = GameFont.Small;
            RenameUIUtility.DrawRenameButton(new Rect(trainabilityLeft, trainabilityTop, 30f, 30f), pawn);

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.Label("CreatureTrainability".Translate(pawn.def.label).CapitalizeFirst() + ": " + pawn.RaceProps.trainability.LabelCap, 22f);
            // replaced pawn.RaceProps.wildness & TrainableUtility.GetWildnessExplanation(pawn.def)
            listing_Standard.Label("CreatureWildness".Translate(pawn.def.label).CapitalizeFirst() + ": " + WildnessUtility.GetWildness(pawn).ToStringPercent(), 22f, TrainableUtilityPawn.GetWildnessExplanation(pawn));

            if (pawn.training.HasLearned(TrainableDefOf.Obedience))
            {
                Rect rect2 = listing_Standard.GetRect(25f);
                Widgets.Label(rect2, "Master".Translate() + ": ");
                rect2.xMin = rect2.center.x;
                if (pawn.RaceProps.playerCanChangeMaster || !ModsConfig.IdeologyActive)
                {
                    TrainableUtility.MasterSelectButton(rect2, pawn, paintable: false);
                }
                else if (pawn.playerSettings?.Master != null)
                {
                    Widgets.Label(rect2, TrainableUtility.MasterString(pawn).Truncate(rect2.width));
                    TooltipHandler.TipRegion(rect2, "DryadCannotChangeMaster".Translate(pawn.Named("ANIMAL"), pawn.playerSettings.Master.Named("MASTER")).CapitalizeFirst());
                }
                listing_Standard.Gap();

                Rect rect3 = listing_Standard.GetRect(25f);
                bool checkOn = pawn.playerSettings.followDrafted;
                Widgets.CheckboxLabeled(rect3, "CreatureFollowDrafted".Translate(), ref checkOn);
                if (checkOn != pawn.playerSettings.followDrafted)
                {
                    pawn.playerSettings.followDrafted = checkOn;
                }

                Rect rect4 = listing_Standard.GetRect(25f);
                bool checkOn2 = pawn.playerSettings.followFieldwork;
                Widgets.CheckboxLabeled(rect4, "CreatureFollowFieldwork".Translate(), ref checkOn2);
                if (checkOn2 != pawn.playerSettings.followFieldwork)
                {
                    pawn.playerSettings.followFieldwork = checkOn2;
                }
            }

            if (pawn.RaceProps.showTrainables)
            {
                listing_Standard.Gap();
                float num = 50f;
                List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
                for (int i = 0; i < trainableDefsInListOrder.Count; i++)
                {
                    // Call TryDrawTrainableRow using reflection
                    bool rowDrawn = (bool)tryDrawTrainableRowMethod.Invoke(null, new object[] { listing_Standard.GetRect(28f), pawn, trainableDefsInListOrder[i] });
                    if (rowDrawn)
                    {
                        num += 28f;
                    }
                }
            }

            listing_Standard.End();
            return false; // Prevent the original method from running
        }
    }
}
