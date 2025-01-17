using Verse;
using RimWorld;
using UnityEngine;
using Verse.AI;
using HarmonyLib;
using System.Reflection;

[HarmonyPatch(typeof(Toils_Interpersonal), "TryTrain")]
public static class Patch_Toils_Interpersonal_TryTrain
{
    // Cache the reflection MethodInfo for the internal GetSteps method
    private static readonly MethodInfo getStepsMethod = typeof(Pawn_TrainingTracker).GetMethod("GetSteps", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

    [HarmonyPrefix]
    public static bool ReplaceTryTrain(ref Toil __result, TargetIndex traineeInd)
    {
        Toil toil = ToilMaker.MakeToil("TryTrain");
        toil.initAction = delegate
        {
            Pawn actor = toil.actor;
            Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(traineeInd).Thing;

            if (pawn.Spawned && pawn.Awake() && actor.interactions.TryInteractWith(pawn, InteractionDefOf.TrainAttempt))
            {
                float statValue = actor.GetStatValue(StatDefOf.TrainAnimalChance);
                statValue *= GenMath.LerpDouble(0f, 1f, 1.5f, 0.5f, WildnessUtility.GetWildness(pawn)); // replaced pawn.RaceProps.wildness
                if (actor.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn))
                {
                    statValue *= 5f;
                }
                statValue = Mathf.Clamp01(statValue);

                TrainableDef trainableDef = pawn.training.NextTrainableToTrain();
                if (trainableDef == null)
                {
                    Log.ErrorOnce("Attempted to train untrainable animal", 7842936);
                }
                else
                {
                    string text;
                    if (Rand.Value < statValue)
                    {
                        pawn.training.Train(trainableDef, actor);
                        if (pawn.caller != null)
                        {
                            pawn.caller.DoCall();
                        }
                        text = "TextMote_TrainSuccess".Translate(trainableDef.LabelCap, statValue.ToStringPercent());
                        RelationsUtility.TryDevelopBondRelation(actor, pawn, 0.007f);
                        TaleRecorder.RecordTale(TaleDefOf.TrainedAnimal, actor, pawn, trainableDef);
                    }
                    else
                    {
                        text = "TextMote_TrainFail".Translate(trainableDef.LabelCap, statValue.ToStringPercent());
                    }

                    // Use the GetSteps method to retrieve the steps
                    int currentSteps = (int)getStepsMethod.Invoke(pawn.training, new object[] { trainableDef });

                    text = text + "\n" + currentSteps + " / " + trainableDef.steps;
                    MoteMaker.ThrowText((actor.DrawPos + pawn.DrawPos) / 2f, actor.Map, text, 5f);
                }
            }
        };

        toil.defaultCompleteMode = ToilCompleteMode.Delay;
        toil.defaultDuration = 100;
        toil.activeSkill = () => SkillDefOf.Animals;
        __result = toil;

        // Return false to prevent the original method from running
        return false;
    }
}
