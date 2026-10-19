using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TheVestige.Scripts.TheVestige;

namespace TheVestige.Scripts.Modules;

public class ContentModule : MonoBehaviour
{
    private AssetBundle _timberHearthBundle;
    private AssetBundle _brittleHollowBundle;
    private AssetBundle _LOCBundle;

    private void Awake()
    {
        NewHorizons.GetAllTitleScreensLoadedEvent().AddListener(dataStreamModule.OnTitleScreenLoaded);
        
        // Bundle loading

        _timberHearthBundle = bundleModule.Load("tv_timberhearth");
        _brittleHollowBundle = bundleModule.Load("tv_brittlehollow");
        _LOCBundle = bundleModule.Load("low_light_observation_chamber");
    }

    public void OnStarSystemLoaded(string systemName)
    {
        if (systemName != "SolarSystem")
            return;
        
        var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth);
  
        var tv_timberHearth = Instantiate(_timberHearthBundle.LoadAsset<GameObject>("TV_TimberHearth"), timberHearth.gameObject.transform);
        tv_timberHearth.transform.localPosition = Vector3.zero;
        tv_timberHearth.SetActive(false);
        
        tv_timberHearth.FixMaterials();
        
        foreach (var receiver in tv_timberHearth.GetComponentsInChildren<InteractReceiver>())
            receiver.EnableInteraction();
  
        tv_timberHearth.SetActive(true);
  
        var tv_LOC = Instantiate(_LOCBundle.LoadAsset<GameObject>("Sector_Low_Light_Observation_Chamber"), timberHearth.gameObject.transform);
        tv_LOC.transform.localPosition = new Vector3(-134.5f, 41.2f, 210.2104f);
        tv_LOC.transform.localRotation = Quaternion.Euler(-2.147f, -126.498f, -82.146f);
        
        tv_LOC.FixMaterials();
        
        // ------------------------------------------------------------------------------------------------------------------------

        var brittleHollow = Locator.GetAstroObject(AstroObject.Name.BrittleHollow);
        
        var tv_brittlehollow = Instantiate(_brittleHollowBundle.LoadAsset<GameObject>("NOM_Fragment_Area"), brittleHollow.gameObject.transform);
        tv_brittlehollow.transform.localPosition = new Vector3(-1.689968f, 280.773f, 106.2336f);
        tv_brittlehollow.transform.localRotation = Quaternion.Euler(-27.58f, 210.858f, -21.824f);

        tv_brittlehollow.FixMaterials();

        foreach (var state in tv_brittlehollow.transform.Find("Quantum_Death").GetComponentsInChildren<QuantumState>(true))
            NewHorizons.SpawnSignal(Instance, state.transform.GetChild(1).gameObject, "planets/Audio/TV_QuantumSignal.ogg", "Trail Shard", "Quantum");
    }
}