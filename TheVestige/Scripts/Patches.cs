using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static TheVestige.Scripts.TheVestige;

namespace TheVestige.Scripts;

public class Patches
{
    public const float OPCFireDelay = 1.2f;

    public static HashSet<string> _shipLogFactIDs = [];

    // Data Stream Fire Delay
    [HarmonyPatch(typeof(OrbitalProbeLaunchController), nameof(OrbitalProbeLaunchController.OnStartOfTimeLoop))]
    public static class OPCController_OnStartOfTimeLoop_Patch
    {
        public static void Postfix(OrbitalProbeLaunchController __instance)
        {
            __instance._probeLaunchTime = Time.time + OPCFireDelay;
        }
    }
    
    /// Overrides NH translations with my own localization module
    [HarmonyPatch(typeof(ShipLogFact), nameof(ShipLogFact.GetText))]
    public static class ShipLogFact_GetText_Patch
    {
        public static void Postfix(ShipLogFact __instance, ref string __result)
        {
            string id = __instance.GetID();

            if (!_shipLogFactIDs.Contains(id))
                return;

            __result = localizationModule.GetTranslation($"{id}:Text", __instance._text);
        }
    }
}