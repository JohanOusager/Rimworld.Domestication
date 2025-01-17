using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

public class StatWorker_FilthRate : StatWorker
{
    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        Pawn pawn = req.Thing as Pawn;
        return pawn != null && IsCleanly(pawn) ? 1 : base.GetValueUnfinalized(req, applyPostProcess);
    }
    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        StringBuilder stringBuilder = new StringBuilder();
        float baseValueFor = GetBaseValueFor(req);
        if (baseValueFor != 0f || stat.showZeroBaseValue)
        {
            stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(baseValueFor, numberSense));
        }

        Pawn pawn = req.Thing as Pawn;
        if (pawn != null)
        {
            if (IsCleanly(pawn))
            {
                stringBuilder.AppendLine("Domesticated:");
                stringBuilder.AppendLine("    Cleanly: => 1.0"); 
            }
            return stringBuilder.ToString();
        }
        else
        {
            return base.GetExplanationUnfinalized(req, numberSense);
        }
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        Pawn pawn = req.Thing as Pawn;
        if (pawn != null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(GetValueUnfinalized(req), stat.toStringNumberSense));
            return stringBuilder.ToString();
        }
        else
        {
            return base.GetExplanationFinalizePart(req, numberSense, finalVal);
        }
    }

    private static bool IsCleanly(Pawn pawn)
    {
        return pawn.health.hediffSet.HasHediff(DomesticatedDefOf.Cleanly);
    }
}