using HarmonyLib;
using Verse;
using RimWorld;

namespace MinotaurWayBetterRomancePatch
{
    [StaticConstructorOnStartup]
    public static class MWBRPatch
    {
        static MWBRPatch()
        {
            // This static constructor will be called immediately when the game starts
            Log.Message("lowmates patch is starting up!");

            // Initialize Harmony or patch methods here
            Harmony harmony = new("jbou.minotaurWayBetterRomancePatch");
            harmony.PatchAll();  // This applies all patches in the mod automatically
        }
    }
}