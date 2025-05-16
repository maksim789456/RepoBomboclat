using HarmonyLib;

namespace RepoBomboclat.Patches;

[HarmonyPatch]
public class ExtractPointPatch
{
    [HarmonyPatch(typeof(ExtractionPoint), "Start")]
    [HarmonyPostfix]
    public static void StartPostfix(ExtractionPoint __instance)
    {
        __instance.gameObject.AddComponent<ExtractPointBomboclat>();
    }
}