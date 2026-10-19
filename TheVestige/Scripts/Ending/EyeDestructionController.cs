using UnityEngine;

namespace TheVestige.Scripts.Ending;

public class EyeDestructionController : MonoBehaviour
{
    public static EyeDestructionController Instance { get; private set; }
    
    public Renderer destructionSkyboxRenderer;
    public OWAudioSource oneShotAudioSource;
    
    public float CurrentProgress { get; private set; }
    
    [SerializeField] private AnimationCurve _flashProgressCurve;
    [SerializeField] private float _flashTime;
    
    [SerializeField] private AnimationCurve _destructionProgressCurve;
    [SerializeField] private float _destructionTime;

    [SerializeField] private float _nuclearSoundTime;
    
    // After flash time
    [SerializeField] private float _destructionDelay;

    [SerializeField] private float _exposureStart = 0.6f;
    [SerializeField] private float _exposureEnd = 0.7f;
    
    private Material _skyboxMaterial => destructionSkyboxRenderer.sharedMaterial;
    private MaterialPropertyBlock _block;

    private float _timer;
    private bool _started;

    private bool _nuclearSoundPlayed;

    private OWCamera _camera;
    private bool _endFadeStarted;
    
    private static readonly int PROGRESS_ID = Shader.PropertyToID("_Progress");
    private static readonly int FLASH_STRENGTH_ID = Shader.PropertyToID("_FlashStrength");
    
    public EyeDestructionController() => Instance = this;
    
    private void Awake()
    {
        _block = new MaterialPropertyBlock();
    }

    private void Start()
    {
        _timer = 0f;
        _started = false;
        _endFadeStarted = false;
        _nuclearSoundPlayed = false;
        _camera = Locator.GetPlayerCamera();
        _camera.postProcessingSettings.colorGrading.postExposure = _camera.postProcessingSettings.colorGradingDefault.postExposure;
    }

    [ContextMenu("Destroy Universe")]
    private void DestroyUniverse()
    {
        _timer = 0f;
        _started = true;
        _nuclearSoundPlayed = false;
        CurrentProgress = 0f;
        
        GlobalMessenger.FireEvent("OnDestroyUniverse");
    }

    public void Update()
    {
        if (!_started)
            return;
        
        _timer += Time.deltaTime;

        if (destructionSkyboxRenderer == null)
            return;
        
        destructionSkyboxRenderer.GetPropertyBlock(_block);
        
        float flashT = _timer / _flashTime;
        _block.SetFloat(FLASH_STRENGTH_ID, _flashProgressCurve.Evaluate(flashT));
        
        if (_timer > _nuclearSoundTime && !_nuclearSoundPlayed)
        {
            oneShotAudioSource.PlayOneShot(AudioType.Sun_Explosion);
            _nuclearSoundPlayed = true;
        }
        
        float destructionT = (_timer - (_flashTime + _destructionDelay)) / _destructionTime;
        CurrentProgress = _destructionProgressCurve.Evaluate(destructionT);
        _block.SetFloat(PROGRESS_ID, CurrentProgress);
        
        destructionSkyboxRenderer.SetPropertyBlock(_block);

        // ----------------------------------------------------------------------------------
        // Camera "Big Bang" flash

        if (CurrentProgress >= _exposureStart)
        {
            float exposureT = Mathf.InverseLerp(_exposureStart, _exposureEnd, CurrentProgress);
            _camera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(0f, 15f, exposureT);

            if (!_endFadeStarted)
            {
                oneShotAudioSource.PlayOneShot(AudioType.Death_BigBang);
                GlobalMessenger<float>.FireEvent("OnVestigeEndFade", (_exposureEnd - _exposureStart) * _destructionTime);
                _endFadeStarted = true;
            }
        }
    }
}