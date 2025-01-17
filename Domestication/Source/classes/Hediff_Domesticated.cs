namespace Verse;
using System.Text;

public class Hediff_Domesticated : HediffWithComps
{
    private static readonly StringBuilder tipSb = new StringBuilder();

    public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo) {
        string base_string = base.GetTooltip(pawn, showHediffsDebugInfo);
        tipSb.Clear();
        tipSb.AppendLine(base_string);
        float wildnessoffset = WildnessUtility.GetWildnessOffset(pawn);
        tipSb.AppendLine(" - " + wildnessoffset.ToStringPercent() + " wildness");

        return tipSb.ToString().TrimEnd();
    }
}