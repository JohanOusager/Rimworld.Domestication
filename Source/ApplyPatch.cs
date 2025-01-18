using HarmonyLib;
using Verse;

   
[StaticConstructorOnStartup]
public static class ApplyPatches
{
    static ApplyPatches()
    {
        Log.Message("domestication patches are starting up!");

        // Initialize Harmony or patch methods here
        Harmony harmony = new("jbou.DomesticationPatches");
           
        harmony.PatchAll();  // This applies all patches in the mod automatically
        
    }
    
}