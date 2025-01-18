// Class implementing/copying some TrainableUtility methods for Pawn arg istead of ThingDef 
using Verse;
using RimWorld;
using UnityEngine;
using System.Text;
using System.Reflection;

public class TrainableUtilityPawn
{
    // Static field to hold the DecayIntervalDaysFromWildnessCurve
    private static readonly SimpleCurve DecayIntervalDaysFromWildnessCurve;

    // Static constructor (runs once when the class is first accessed)
    static TrainableUtilityPawn()
    {
        // Use reflection to access the private field
        var fieldInfo = typeof(TrainableUtility).GetField("DecayIntervalDaysFromWildnessCurve", BindingFlags.NonPublic | BindingFlags.Static);

        if (fieldInfo == null)
        {
            Log.Error("Failed to access DecayIntervalDaysFromWildnessCurve in TrainableUtility.");
        }
        else
        {
            DecayIntervalDaysFromWildnessCurve = fieldInfo.GetValue(null) as SimpleCurve;

            if (DecayIntervalDaysFromWildnessCurve == null)
            {
                Log.Error("DecayIntervalDaysFromWildnessCurve is null.");
            }
        }
    }

    // Method using the curve to calculate degradation period ticks
    public static int TrainingDegradationPeriodTicks(Pawn pawn)
    {
        if (DecayIntervalDaysFromWildnessCurve == null)
        {
            Log.Error("DecayIntervalDaysFromWildnessCurve is not initialized.");
            return 0; // Return a default value or handle the error
        }

        float wildness = WildnessUtility.GetWildness(pawn);
        return Mathf.RoundToInt(DecayIntervalDaysFromWildnessCurve.Evaluate(wildness) * 60000f);
    }

    // copies TrainableUtility.GetWildnessExplanation but with Pawn arg instead of ThingDef
    public static string GetWildnessExplanation(Pawn pawn)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("WildnessExplanation".Translate());
        stringBuilder.AppendLine();
        if (pawn.RaceProps != null && !pawn.RaceProps.Humanlike)
        {
            stringBuilder.AppendLine(string.Format("{0}: {1}", "TrainingDecayInterval".Translate(), TrainingDegradationPeriodTicks(pawn).ToStringTicksToDays()));
            stringBuilder.AppendLine();
        }
        if (!TrainableUtility.TamenessCanDecay(pawn.def))
        {
            string key = (pawn.RaceProps.FenceBlocked ? "TamenessWillNotDecayFenceBlocked" : "TamenessWillNotDecay");
            stringBuilder.AppendLine(key.Translate());
        }
        return stringBuilder.ToString();
    }
} // end of utility class