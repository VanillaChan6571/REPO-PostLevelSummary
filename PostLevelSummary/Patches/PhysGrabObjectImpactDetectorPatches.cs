using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace PostLevelSummary.Patches
{
    [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
    class PhysGrabObjectImpactDetectorPatches
    {
        [HarmonyPatch("BreakRPC")]
        [HarmonyPostfix]
        static void StartPostFix(PhysGrabObjectImpactDetector? __instance)
        {
            ValuableObject? vo = __instance?.GetComponent<ValuableObject>();
            if (vo == null) return;

            PostLevelSummary.Level.CheckValueChange(vo);
        }

        [HarmonyPatch(typeof(PhysGrabObject), "DestroyPhysGrabObjectRPC")]
        [HarmonyPrefix]
        public static void DestroyPhysGrabObjectPrefix(PhysGrabObject __instance)
        {
            if (PostLevelSummary.InGame)
            {
                ValuableObject? vo = __instance?.GetComponent<ValuableObject>();
                if (vo == null) return;

                PostLevelSummary.Level.ItemBroken(vo);
            }
        }
    }
}
