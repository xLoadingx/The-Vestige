using System.Collections;
using UnityEngine;
using static TheVestige.Scripts.TheVestige;

namespace TheVestige.Scripts.Modules;

public class DataStreamModule : MonoBehaviour
{
    private float _nextProbeStreamTime = 0f;

    private AssetBundle _nomDataStream;
    private AssetBundle _titleScreenBundle;
    
    private GameObject _dataStreamPrefab;

    private NomaiDataStreamRenderer _titleScreenStream;
    private NomaiDataStreamRenderer _tesseractStream;

    private GameObject _DataStreamParentGO;

    private NomaiDataStreamRenderer _ATPToOPC_Stream;
    private NomaiDataStreamRenderer _ProbeToPTM_Stream;
    private NomaiDataStreamRenderer _PTMToATP_Stream;

    public void Awake()
    {
        // Hooks
        
        GlobalMessenger.AddListener("SunExploded", OnSunExploded);
        
        // Bundle loading
        
        _nomDataStream = bundleModule.Load("nomdataprobe");
        _titleScreenBundle = bundleModule.Load("tv_titlescreen");
    }
    
    public void OnTitleScreenLoaded()
    {
        var dataStreamTitle = Instantiate(_titleScreenBundle.LoadAsset<GameObject>("DataStreamRing_TitleScreen Variant"));
        _titleScreenStream = dataStreamTitle.GetComponent<NomaiDataStreamRenderer>();
        dataStreamTitle.name = "[The Vestige] Data Stream Title";

        if (AstralCodecInstalled)
            StartCoroutine(WaitForTesseract());
    }

    public IEnumerator WaitForTesseract()
    {
        GameObject tesseract;
        
        while ((tesseract = GameObject.Find("Tesseract(Clone)")) == null)
            yield return null;
            
        var dataStreamTesseract = Instantiate(_titleScreenStream.gameObject, tesseract.transform);
        dataStreamTesseract.transform.localScale = new Vector3(386.666f, -386.666f, 4470);
        dataStreamTesseract.transform.localRotation = Quaternion.Euler(90, 0, 0);
        dataStreamTesseract.transform.localPosition = new Vector3(0, 1, 0);
        dataStreamTesseract.name = "Tesseract Data Stream";
            
        _tesseractStream = dataStreamTesseract.GetComponent<NomaiDataStreamRenderer>();
    }

    public void OnStarSystemLoaded(string systemName)
    {
        if (systemName != "SolarSystem")
            return;
        
        // Reset Variables

        _nextProbeStreamTime = 0f;
        
        // ---------------
        
        if (_dataStreamPrefab == null)
        {
            _dataStreamPrefab = Instantiate(_nomDataStream.LoadAsset<GameObject>("NOM_DataStream_Probe"));
            _dataStreamPrefab.name = "Data Stream Prefab";
            _dataStreamPrefab.transform.localPosition = Vector3.zero;
            _dataStreamPrefab.transform.localRotation = Quaternion.identity;
            DontDestroyOnLoad(_dataStreamPrefab);
        }
        
        var PTMStatue = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).transform.Find("Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken/Prefab_NOM_StatueHead/");
        var ATPMask = GameObject.Find("TimeLoopRing_Body/Interactibles_TimeLoopRing/MaskPlatform/MaskMonoliths/Mask (7)").transform;

        _DataStreamParentGO = new GameObject("[The Vestige] Data Streams");
        
        // Ash Twin Project to Orbital Probe Cannon (sends command to fire probe)
        
        var ATPToOPC_Stream_GO = Instantiate(_dataStreamPrefab, _DataStreamParentGO.transform);
        ATPToOPC_Stream_GO.name = "Data Stream ATP to OPC";

        _ATPToOPC_Stream = ATPToOPC_Stream_GO.GetComponent<NomaiDataStreamRenderer>();
        _ATPToOPC_Stream.travelTime = Patches.OPCFireDelay;
        _ATPToOPC_Stream.loop = false;
        _ATPToOPC_Stream.objects = [
            Locator.GetAstroObject(AstroObject.Name.TowerTwin).transform,
            Locator.GetAstroObject(AstroObject.Name.ProbeCannon).GetComponent<OrbitalProbeLaunchController>()._probeBody.transform
        ];
        
        // The Orbital Probe Cannon does not have any statue to communicate like I thought it did,
        // so I'll just fire it at the probe  as the command to fire.
        
        // The center of Ash Twin is also right where the black hole is, so no need to change that.

        _ATPToOPC_Stream.Pulse();
        
        // Probe to Probe Tracking Module between End Times and when the sun explodes (probe data)
        
        var ProbeToPTM_Stream_GO = Instantiate(_dataStreamPrefab, _DataStreamParentGO.transform);
        ProbeToPTM_Stream_GO.name = "Data Stream Probe to PTM";
        
        _ProbeToPTM_Stream = ProbeToPTM_Stream_GO.GetComponent<NomaiDataStreamRenderer>();
        _ProbeToPTM_Stream.loop = false;
        _ProbeToPTM_Stream.travelTime = 7f;
        _ProbeToPTM_Stream.tiling = new(800, 1);
        _ProbeToPTM_Stream.objects = [
            Locator.GetAstroObject(AstroObject.Name.ProbeCannon).GetComponent<OrbitalProbeLaunchController>()._probeBody.transform,
            PTMStatue
        ];
        
        // Probe Tracking Module to ATP (after sun explodes to send probe data to Ash Twin Project)
        
        var PTMToATP_Stream_GO = Instantiate(_dataStreamPrefab, _DataStreamParentGO.transform);
        PTMToATP_Stream_GO.name = "Data Stream PTM to ATP";
        
        _PTMToATP_Stream = PTMToATP_Stream_GO.GetComponent<NomaiDataStreamRenderer>();
        _PTMToATP_Stream.loop = false;
        _PTMToATP_Stream.travelTime = 3f;
        _PTMToATP_Stream.objects = [
            PTMStatue,
            ATPMask
        ];
    }

    public void OnSunExploded()
    {
        _PTMToATP_Stream?.Pulse();
    }
    
    public void Update()
    {
        if (TimeLoop.GetSecondsRemaining() <= 85f && TimeLoop.GetSecondsRemaining() >= 0f && Time.time >= _nextProbeStreamTime)
        {
            _ProbeToPTM_Stream?.Pulse();
            _nextProbeStreamTime = Time.time + 10f;
        }
    }
}