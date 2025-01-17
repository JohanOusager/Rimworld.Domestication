using Verse;
using RimWorld;
using UnityEngine;

// StatWorker_MinimumHandlingSkill replacement class
public class StatWorker_MinimumHandlingSkillWithDomestication : StatWorker_MinimumHandlingSkill
{
    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        Pawn pawn = req.Pawn;
        return pawn != null ? ValueFromPawn(pawn) : base.GetValueUnfinalized(req, applyPostProcess);
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        Pawn pawn = req.Pawn;
        if (pawn != null){
            float wildness = WildnessUtility.GetWildness(pawn);
            return "Wildness".Translate() + " " + wildness.ToStringPercent() + ": " + ValueFromPawn(pawn).ToString("F0");
        }
        return base.GetExplanationUnfinalized(req, numberSense);
    }

    private float ValueFromPawn(Pawn pawn)
    {
        float wildness = WildnessUtility.GetWildness(pawn);
        return Mathf.Clamp(GenMath.LerpDouble(0.15f, 1f, 0f, 10f, wildness), 0f, 20f);
    }
}