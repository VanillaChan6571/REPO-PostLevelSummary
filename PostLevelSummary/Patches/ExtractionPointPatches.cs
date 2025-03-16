using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace PostLevelSummary.Patches
{
    [HarmonyPatch(typeof(ExtractionPoint))]
    class ExtractionPointPatches
    {
        [HarmonyPatch("StateComplete")]
        [HarmonyPostfix]
        public static void StateCompletePostfix()
        {
            if (PostLevelSummary.InGame)
            {
                PostLevelSummary.Level.Extracted();
            }
        }
    }
}
