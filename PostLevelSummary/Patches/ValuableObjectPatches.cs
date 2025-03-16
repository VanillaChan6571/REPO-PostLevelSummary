using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace PostLevelSummary.Patches
{
    [HarmonyPatch(typeof(ValuableObject))]
    class ValuableObjectPatches
    {
        [HarmonyPatch("DollarValueSetLogic")]
        [HarmonyPostfix]
        static void DollarValueSetLogicPostfix(ValuableObject __instance)
        {
            if (__instance.name.ToLower().Contains("surplus"))
                return;

            PostLevelSummary.AddValuable(__instance);
        }
    }
}
