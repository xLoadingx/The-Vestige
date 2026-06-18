using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OWML.Utils;

namespace TheVestige.Scripts;

public class Patches
{
    public static class CustomSignals
    {
        public static readonly SignalName QuantumShardSignal =
            EnumUtils.Create<SignalName>("TheVestige_RemnantShard");

        public static readonly Dictionary<SignalName, string> AllSignals = new()
        {
            { QuantumShardSignal, "Remnant Shard" }
        };
    }

    public static class PlayerDataAccess
    {
        private static readonly FieldInfo CurrentGameSaveField =
            AccessTools.Field(typeof(PlayerData), "_currentGameSave");

        public static GameSave CurrentGameSave =>
            (GameSave)CurrentGameSaveField.GetValue(null);
    }

    // Signals

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.LearnSignal))]
    public class PlayerData_LearnSignal_Patch
    {
        public static void Prefix(SignalName signalName)
        {
            if (!CustomSignals.AllSignals.ContainsKey(signalName))
                return;

            var save = PlayerDataAccess.CurrentGameSave;
            var key = (int)signalName;

            save.knownSignals.TryAdd(key, false);
        }
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.KnowsSignal))]
    public class PlayerData_KnowsSignal_Patch
    {
        public static bool Prefix(SignalName signalName, ref bool __result)
        {
            if (!CustomSignals.AllSignals.ContainsKey(signalName))
                return true;

            var save = PlayerDataAccess.CurrentGameSave;
            var key = (int)signalName;

            save.knownSignals.TryAdd(key, false);

            __result = save.knownSignals[key];
            return false;
        }
    }

    [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.SignalNameToString))]
    public static class AudioSignal_SignalNameToString_Patch
    {
        public static bool Prefix(SignalName name, ref string __result)
        {
            if (CustomSignals.AllSignals.TryGetValue(name, out var signalName))
            {
                __result = signalName;
                return false;
            }

            return true;
        }
    }
}