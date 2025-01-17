// wildness static utility class
using Verse;
using RimWorld;
using UnityEngine;

public class WildnessUtility
{
    public static float GetWildnessOffset(Pawn pawn)
    {
        if (pawn.RaceProps.Animal && pawn.health.hediffSet.HasHediff(DomesticatedDefOf.Domesticated))
        {
            return pawn.health.hediffSet.GetFirstHediffOfDef(DomesticatedDefOf.Domesticated).TryGetComp<HediffComp_WildnessOffset>().wildnessOffset;
        }
        return 0f;
    }

    public static float GetWildness(Pawn pawn)
    {
        return pawn.RaceProps.wildness + GetWildnessOffset(pawn);
    }

    private static readonly float offspringWildnessChangePerTrainedSkill = -0.01f;
    private static readonly float wildnessDrift = -0.02f;

    private static float GetOffspringWildnessOffsetContribution(Pawn parent)
    {
        float parentWildnessOffset = GetWildnessOffset(parent);

        // count number of fully trained skills
        int nlearned = 0;
        foreach (TrainableDef trainable in TrainableUtility.TrainableDefsInListOrder)
        {
            if (parent.training.HasLearned(trainable))
            {
                nlearned += 1;
            }
        }

        // apply some change based on n learned skills
        return parentWildnessOffset + nlearned * offspringWildnessChangePerTrainedSkill;
    }

    private static float GetOffspringWildnessOffset(Pawn mother, Pawn father)
    {
        float inheritedWildnessOffset = GetOffspringWildnessOffsetContribution(mother);

        if (father != null)
        {
            inheritedWildnessOffset += GetOffspringWildnessOffsetContribution(father);
            inheritedWildnessOffset /= 2f;
        }

        // add some randomness
        return inheritedWildnessOffset + Rand.Range(-wildnessDrift, wildnessDrift);
    }

    private static void ApplyPottyTraining(ref Pawn child)
    {
        if (GetWildness(child) < 0.02)
        {
            switch (child.GetStatValue(StatDefOf.FilthRate))
            {
                // the filth rates of various medium-large predators 
                case 2: // wolves, panther etc.
                case 3: // No vanilla animals have this filth rate but modded ones might
                case 4: // bears
                    child.health.AddHediff(DomesticatedDefOf.Cleanly);
                    break;
            }
        }
    }

    public static void ApplyDomestication(ref Pawn child, Pawn mother, Pawn father)
    {
        if (CanDomesticate(child))
        {
            float inheritedWildnessOffset = GetOffspringWildnessOffset(mother, father);

            if (inheritedWildnessOffset < 0f)
            {
                inheritedWildnessOffset = child.RaceProps.wildness + inheritedWildnessOffset > 0 ? inheritedWildnessOffset : -child.RaceProps.wildness;

                Hediff hediff_Domesticated = (Hediff)HediffMaker.MakeHediff(DomesticatedDefOf.Domesticated, child);
                hediff_Domesticated.TryGetComp<HediffComp_WildnessOffset>().wildnessOffset = inheritedWildnessOffset;
                child.health.AddHediff(hediff_Domesticated);
            }
            ApplyPottyTraining(ref child);
        }
    }

    public static bool CanDomesticate(Pawn pawn)
    {
        // non-pen animals born to the player faction
        return pawn.RaceProps.Animal && !pawn.RaceProps.FenceBlocked && pawn.Faction == Faction.OfPlayer;
    }
} // end of utility class