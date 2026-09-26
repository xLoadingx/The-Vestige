using System.Collections.Generic;
using UnityEngine;
using static TheVestige.Scripts.TheVestige;

namespace TheVestige.Scripts.Modules;

public class BundleModule : MonoBehaviour
{
    public Dictionary<string, AssetBundle> loadedBundles = new();

    public AssetBundle Load(string path)
    {
        path = $"Resources/{path}";
        
        if (loadedBundles.TryGetValue(path, out var bundle))
            return bundle;
        
        bundle = Instance.ModHelper.Assets.LoadBundle(path);

        if (bundle == null)
            return null;
        
        Log.Info($"Loaded bundle | {path}");

        loadedBundles.Add(path, bundle);
        
        // Modification streams
        
        localizationModule.RegisterNomaiText(bundle);
        
        // --------------------
        
        return bundle;
    }
}