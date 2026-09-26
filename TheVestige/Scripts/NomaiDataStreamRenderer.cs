using System;
using UnityEngine;

namespace TheVestige.Scripts;

public class NomaiDataStreamRenderer : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _block;
    private bool _reversed = false;
    
    public Transform[] objects;

    public Vector2 tiling = Vector2.one;
    public Vector2 offset = Vector2.zero;

    public float streamLength = 0.3f;
    public float travelTime = 2.0f;
    public int repetitions = 1;
    public bool loop = false;

    public bool isTextureVertical = false;
    public bool isEmissionVertical = false;
    
    protected float _elapsedTime = 0.0f;
    protected bool _isPulsing = false;
    
    private static readonly int ELAPSED_TIME_ID = Shader.PropertyToID("_ElapsedTime");
    private static readonly int PULSE_LENGTH_ID = Shader.PropertyToID("_PulseLength");
    private static readonly int LOOP_ID = Shader.PropertyToID("_Loop");
    private static readonly int REPETITIONS_ID = Shader.PropertyToID("_Repetitions");
    private static readonly int TRAVEL_TIME_ID = Shader.PropertyToID("_TravelTime");
    private static readonly int TEXTURE_VERTICAL_ID = Shader.PropertyToID("_TextureVertical");
    private static readonly int EMISSION_VERTICAL_ID = Shader.PropertyToID("_EmissionVertical");
    private static readonly int REVERSED_ID = Shader.PropertyToID("_Reversed");
    private static readonly int MAIN_TEX_ST_ID = Shader.PropertyToID("_MainTex_ST");

    public void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
            return;
        
        _block = new MaterialPropertyBlock();
        _renderer.SetPropertyBlock(_block);
    }

    protected virtual void Start()
    {
        UpdateMaterialSettings();
    }

    public void UpdateMaterialSettings()
    {
        if (_renderer == null)
            return;
        
        _block.SetFloat(PULSE_LENGTH_ID, streamLength);
        _block.SetFloat(LOOP_ID, loop ? 1.0f : 0.0f);
        _block.SetFloat(TRAVEL_TIME_ID, travelTime);
        _block.SetFloat(REPETITIONS_ID, repetitions);
        _block.SetFloat(TEXTURE_VERTICAL_ID, isTextureVertical ? 1.0f : 0.0f);
        _block.SetFloat(EMISSION_VERTICAL_ID, isEmissionVertical ? 1.0f : 0.0f);
        
        _block.SetVector(MAIN_TEX_ST_ID, new Vector4(
            tiling.x,
            tiling.y,
            offset.x,
            offset.y
        ));
        
        _renderer.SetPropertyBlock(_block);
    }

    private void OnValidate()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        if (_renderer == null)
            return;
        
        _block ??= new MaterialPropertyBlock();
        
        UpdateMaterialSettings();
    }

    protected virtual void Update()
    {
        if (_renderer == null)
            return;
        
        if (_renderer is LineRenderer _lineRenderer && objects != null)
        {
            if (_lineRenderer.positionCount != objects.Length)
                _lineRenderer.positionCount = objects.Length;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null)
                    continue;
                
                _lineRenderer.SetPosition(i, objects[i].position);
            }
        }

        if (loop && !_isPulsing)
            _elapsedTime += Time.deltaTime;

        if (_isPulsing)
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= travelTime)
            {
                _elapsedTime = 0.0f;
                _isPulsing = false;
                _reversed = false;
            }
        }

        if (loop || _isPulsing)
        {
            _block.SetFloat(ELAPSED_TIME_ID, _elapsedTime);
            _renderer.SetPropertyBlock(_block);
        }
    }

    public void Pulse(bool overrideCurrentPulse = false, bool reversed = false)
    {
        if (loop)
            return;

        if (_isPulsing && !overrideCurrentPulse)
            return;

        if (_renderer == null)
            return;

        _elapsedTime = 0.0f;
        _block.SetFloat(ELAPSED_TIME_ID, _elapsedTime);
        _block.SetFloat(REVERSED_ID, reversed ? 1.0f : 0.0f);
        _renderer.SetPropertyBlock(_block);
        
        _isPulsing = true;
    }
}