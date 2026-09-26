using System;
using System.Collections.Generic;
using System.Globalization;
using TheVestige.Scripts.LOC;
using UnityEngine;
using ExperimentPhase = TheVestige.Scripts.LOC.NTIInfoExperimentController.ExperimentPhase;

namespace TheVestige.Scripts;

public class TVNomaiComputer : NomaiComputer
{
    private static readonly Dictionary<string, Func<string>> _tags = new()
    {
        ["<FutureState>"] = () => NTIInfoExperimentController.Instance.GetFutureStateText(),
        ["<SecondsUntilPresent>"] = () =>
        {
            var instance = NTIInfoExperimentController.Instance;
            
            float secondsRemaining = instance.secondsTillPresent - instance.PhaseTimer;
            secondsRemaining = instance.Phase == ExperimentPhase.FutureRecorded ? Mathf.Max(0f, secondsRemaining) : 0f;
            return secondsRemaining.ToString("0.0", CultureInfo.InvariantCulture);
        }
    };

    public override string GetTextNode(int id)
    {
        var text = base.GetTextNode(id);

        foreach (var pair in _tags)
            text = text.Replace(pair.Key, pair.Value?.Invoke() ?? "");

        return text;
    }
}