using HarmonyLib;
using UnityEngine;

namespace TheVestige.Scripts;

public class Patches
{
    public const float OPCFireDelay = 1.2f;

    [HarmonyPatch(typeof(OrbitalProbeLaunchController), nameof(OrbitalProbeLaunchController.OnStartOfTimeLoop))]
    public static class OPCController_OnStartOfTimeLoop_Patch
    {
        public static void Postfix(OrbitalProbeLaunchController __instance)
        {
            __instance._probeLaunchTime = Time.time + OPCFireDelay;
        }
    }
}