using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using Verse.AI.Group;

[HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
internal class Patch_Hediff_Pregnant_DoBirthSpawn
{
    [HarmonyPrefix]
    public static bool ReplaceDoBirthSpawn(Pawn mother, Pawn father)
    {
        if (!WildnessUtility.CanDomesticate(mother))
        {
            return true; // run the original function
        }
        else
        {
            if (mother.RaceProps.Humanlike && !ModsConfig.BiotechActive)
            {
                return false; // dont run original method
            }
            int num = ((mother.RaceProps.litterSizeCurve == null) ? 1 : Mathf.RoundToInt(Rand.ByCurve(mother.RaceProps.litterSizeCurve)));
            if (num < 1)
            {
                num = 1;
            }
            PawnGenerationRequest request = new PawnGenerationRequest(mother.kindDef, mother.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
            Pawn pawn = null;
            for (int i = 0; i < num; i++)
            {
                pawn = PawnGenerator.GeneratePawn(request);
                if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, mother))
                {
                    WildnessUtility.ApplyDomestication(ref pawn, mother, father);
                    if (pawn.playerSettings != null && mother.playerSettings != null)
                    {
                        pawn.playerSettings.AreaRestrictionInPawnCurrentMap = mother.playerSettings.AreaRestrictionInPawnCurrentMap;
                    }
                    if (pawn.RaceProps.IsFlesh)
                    {
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                        if (father != null)
                        {
                            pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
                        }
                    }
                    if (mother.Spawned)
                    {
                        mother.GetLord()?.AddPawn(pawn);
                    }
                }
                else
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
                TaleRecorder.RecordTale(TaleDefOf.GaveBirth, mother, pawn);
            }
            if (mother.Spawned)
            {
                FilthMaker.TryMakeFilth(mother.Position, mother.Map, ThingDefOf.Filth_AmnioticFluid, mother.LabelIndefinite(), 5);
                if (mother.caller != null)
                {
                    mother.caller.DoCall();
                }
                if (pawn.caller != null)
                {
                    pawn.caller.DoCall();
                }
            }
            return false; // dont run original method
        }
    }
}
