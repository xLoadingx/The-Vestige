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

    public void Awake()
    {
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
        {
            GameObject tesseract = GameObject.Find("Tesseract(Clone)");

            if (tesseract == null)
                return;
            
            var dataStreamTesseract = Instantiate(dataStreamTitle, tesseract.transform);
            dataStreamTesseract.transform.localScale = new Vector3(386.666f, -386.666f, 4470);
            dataStreamTesseract.transform.localRotation = Quaternion.Euler(90, 0, 0);
            dataStreamTesseract.transform.localPosition = new Vector3(0, 1, 0);
            dataStreamTesseract.name = "Tesseract Data Stream";
            
            _tesseractStream = dataStreamTesseract.GetComponent<NomaiDataStreamRenderer>();
        }
    }

    public void OnStarSystemLoaded(string systemName)
    {
        _dataStreamPrefab = Instantiate(_nomDataStream.LoadAsset<GameObject>("NOM_DataStream_Probe"));
        _dataStreamPrefab.name = "Data Stream Prefab";
        _dataStreamPrefab.transform.localPosition = Vector3.zero;
        _dataStreamPrefab.transform.localRotation = Quaternion.identity;
        DontDestroyOnLoad(_dataStreamPrefab);

        _DataStreamParentGO = new GameObject("[The Vestige] Data Streams");
        
        var ATPToOPC_Stream_GO = Instantiate(_dataStreamPrefab, _DataStreamParentGO.transform);
        ATPToOPC_Stream_GO.name = "Data Stream ATP to OPC";

        _ATPToOPC_Stream = ATPToOPC_Stream_GO.GetComponent<NomaiDataStreamRenderer>();
        _ATPToOPC_Stream.travelTime = Patches.OPCFireDelay;
        _ATPToOPC_Stream.loop = false;
        _ATPToOPC_Stream.objects = [
            Locator.GetAstroObject(AstroObject.Name.TowerTwin).transform,
            Locator.GetAstroObject(AstroObject.Name.ProbeCannon).transform
        ];

        _ATPToOPC_Stream.Pulse();
        
        var ProbeToPTM_Stream_GO = Instantiate(_dataStreamPrefab, _DataStreamParentGO.transform);
        ProbeToPTM_Stream_GO.name = "Data Stream Probe to PTM";
        
        _ProbeToPTM_Stream = ProbeToPTM_Stream_GO.GetComponent<NomaiDataStreamRenderer>();
        _ProbeToPTM_Stream.loop = false;
        _ProbeToPTM_Stream.travelTime = 7f;
        _ProbeToPTM_Stream.tiling = new(800, 1);
        _ProbeToPTM_Stream.objects = [
            Locator.GetAstroObject(AstroObject.Name.ProbeCannon).GetComponent<OrbitalProbeLaunchController>()._probeBody.transform,
            Locator.GetAstroObject(AstroObject.Name.GiantsDeep).transform.Find("Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken/Prefab_NOM_StatueHead/")
        ];
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