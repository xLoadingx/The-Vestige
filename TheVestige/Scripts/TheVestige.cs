using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace TheVestige.Scripts;

public class TheVestige : ModBehaviour
{
    public static TheVestige Instance;

    public INewHorizons NewHorizons;

    public void Awake() => Instance = this;

    public void Start()
    {
        new Harmony("ERROR.TheVestige").PatchAll(Assembly.GetExecutingAssembly());

        NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        NewHorizons.LoadConfigs(this);

        // Example of accessing game code.
        OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
        LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
    }

    public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
    {
        if (newScene != OWScene.SolarSystem) return;
        ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
        
        OWMaterialFix.RecacheMaterials();
        
       SpawnTest();
    }

    private AssetBundle LoadEmbeddedBundle(string bundleName)
    {
        string resourceName = "TheVestige.Resources." + bundleName; 
        using var stream = typeof(TheVestige).Assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            ModHelper.Console.WriteLine($"Missing embedded bundle: {resourceName}", MessageType.Error);

            foreach (string resource in typeof(TheVestige).Assembly.GetManifestResourceNames())
                ModHelper.Console.WriteLine($"Embedded resource: {resource}");

            return null;
        }
        
        byte[] bundleData = new byte[stream.Length];
        stream.Read(bundleData, 0, bundleData.Length);

        var bundle = AssetBundle.LoadFromMemory(bundleData);

        if (bundle == null)
            ModHelper.Console.WriteLine($"Failed to load AssetBundle: {resourceName}", MessageType.Error);

        return bundle;
    }

    private void SpawnTest()
    {
        var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth);

        var bundle = LoadEmbeddedBundle("timelineendtest");
        
        var prefab = bundle.LoadAsset<GameObject>("Paradox_Sphere");
        var obj = Instantiate(prefab, timberHearth.transform.Find("Sector_TH/Sector_Village/Interactables_Village/LaunchTower"));
        
        obj.FixMaterials();
        obj.transform.localPosition = new Vector3(-4.1031f, -0.8293f, 11.9366f);
        obj.transform.localRotation = Quaternion.identity;

        var comp = obj.AddComponent<TimelineObliterationTest>();
        
        comp.voidCrackEffectList = obj.GetComponentsInChildren<TimelineObliterationEffect>();
        comp.voidCrackAudioSource = obj.GetComponentInChildren<OWAudioSource>();
        comp.targetObject = obj.GetComponent<VoidShadowEffectController>();
        comp.voidShadowEffect = comp.targetObject;
        var fillRoomObj = obj.transform.Find("TV_Room_ParadoxFill").gameObject;
        comp.fillRoomMaterial = fillRoomObj.GetComponent<MeshRenderer>().material;
        comp.fillRoomClip = bundle.LoadAsset<AudioClip>("TV_FabricSFX_FillRoom_Muffled");

        var limiter = obj.transform.Find("Room_Volume").gameObject.AddComponent<RealityCrackBoxLimiter>();
        
        var renderers = new List<Renderer>();
        renderers.AddRange(obj.transform.Find("Effects_TV_TimelineCracksGroup").gameObject.GetComponentsInChildren<Renderer>());
        renderers.Add(fillRoomObj.GetComponent<Renderer>());
        limiter.voidCrackRenderers = renderers.ToArray();

        comp.limiter = limiter;
        
        // obj.GetComponentInChildren<AudioSignal>()._name = Patches.CustomSignals.QuantumShardSignal;
        
        // if (obj != null)
        // {
        //     DialogueConditionManager.SharedInstance.SetConditionState("TV_SLATE_SCARED_OF_ROCKS", true); 
        // }
        
        bundle.Unload(false);
    }
}