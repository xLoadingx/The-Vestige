using System.Linq;
using System.Reflection;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using TheVestige.Scripts.Modules;
using UnityEngine;

namespace TheVestige.Scripts;

public static class Log
{
    public const bool DEBUG = true;

    public static void Info(string msg, bool debug = true)
    {
        if (!DEBUG && debug)
            return;
        
        TheVestige.Instance.ModHelper.Console.WriteLine(msg, MessageType.Info);
    } 
    
    public static void Warning(string msg, bool debug = true)
    {
        if (!DEBUG && debug)
            return;
        
        TheVestige.Instance.ModHelper.Console.WriteLine(msg, MessageType.Warning);
    }

    public static void Error(string msg, bool debug = true)
    {
        if (!DEBUG && debug)
            return;
        
        TheVestige.Instance.ModHelper.Console.WriteLine(msg, MessageType.Error);
    }
}

public class TheVestige : ModBehaviour
{
    public static TheVestige Instance;
    
    public static INewHorizons NewHorizons;
    
    // Modules
    
    public static GameObject ModuleParentGO;
    
    public static DataStreamModule dataStreamModule;
    public static BundleModule bundleModule;
    public static LocalizationModule localizationModule;
    public static ContentModule contentModule;
    public static EyeStateModule eyeStateModule;
    
    // Other
    
    public static bool AstralCodecInstalled;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        new Harmony("ERROR.TheVestige").PatchAll(Assembly.GetExecutingAssembly());

        NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        NewHorizons.LoadConfigs(this);
        
        AstralCodecInstalled = NewHorizons.GetInstalledAddons().Contains("Walker.AstralCodex");
        
        // Module Init

        ModuleParentGO ??= new GameObject("[The Vestige] Modules");
        DontDestroyOnLoad(ModuleParentGO);
        
        bundleModule ??= new GameObject("Bundle Module").AddComponent<BundleModule>();
        localizationModule ??= new GameObject("Localization Module").AddComponent<LocalizationModule>();
        dataStreamModule ??= new GameObject("Data Stream Module").AddComponent<DataStreamModule>();
        contentModule ??= new GameObject("Content Module").AddComponent<ContentModule>();
        eyeStateModule ??= new GameObject("Eye State Module").AddComponent<EyeStateModule>();
        
        dataStreamModule.transform.SetParent(ModuleParentGO.transform);
        bundleModule.transform.SetParent(ModuleParentGO.transform);
        localizationModule.transform.SetParent(ModuleParentGO.transform);
        contentModule.transform.SetParent(ModuleParentGO.transform);
        eyeStateModule.transform.SetParent(ModuleParentGO.transform);
        
        // Hooks
        
        NewHorizons.GetStarSystemLoadedEvent().AddListener((starSystem) =>
        {
            OWMaterialFix.RecacheMaterials();
            
            contentModule.OnStarSystemLoaded(starSystem);
            dataStreamModule.OnStarSystemLoaded(starSystem);
            localizationModule.OnStarSystemLoaded(starSystem);
            eyeStateModule.OnStarSystemLoaded(starSystem);
            eyeStateModule.ResetState();
        });
        
        // -----------
    }
}