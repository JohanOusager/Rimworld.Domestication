using Verse;

// HediffComp for storing wildness offset
public class HediffComp_WildnessOffset : HediffComp
{
    public float wildnessOffset;

    public HediffComp_WildnessOffset(float initialWildnessOffset)
    {
        wildnessOffset = initialWildnessOffset;
    }

    public HediffComp_WildnessOffset()
    {
        wildnessOffset = 0;
    }
}