using UnityEngine;

namespace TheVestige.Scripts.Ending;

public class DestructionShockController : MonoBehaviour
{
    private static MaterialPropertyBlock _shockLayerBlock;
    private static int _Color = Shader.PropertyToID("_Color");
    private static int _WorldToShockLocalMatrix = Shader.PropertyToID("_WorldToShockLocalMatrix");
    private static int _Dir = Shader.PropertyToID("_Dir");
    private static int _Length = Shader.PropertyToID("_Length");
    private static int _Flare = Shader.PropertyToID("_Flare");
    private static int _TrailFade = Shader.PropertyToID("_TrailFade");
    private static int _GradientLerp = Shader.PropertyToID("_GradientLerp");
    private static int _MainTex_ST = Shader.PropertyToID("_MainTex_ST");

    private bool _destructionStarted;

    [SerializeField] private MeshRenderer _shockLayer;
    [SerializeField] private Color _shockLayerColor = Color.white;
    [SerializeField] private Transform _eyePosition;
    [SerializeField] private float _shockLayerTrailLength = 100f;
    [SerializeField] private float _shockLayerTrailFlare = 100f;
    
    public void Start()
    {
        _shockLayerBlock = new MaterialPropertyBlock();
        GlobalMessenger.AddListener("OnDestroyUniverse", OnDestroyUniverse);
        
        if (_shockLayer == null)
            return;

        _shockLayer.enabled = false;
    }

    public void OnDestroy()
    {
        GlobalMessenger.RemoveListener("OnDestroyUniverse", OnDestroyUniverse);
    }

    private void OnDestroyUniverse() => _destructionStarted = true;
    
    // Mostly taken from the original PlanetShockController
    public void Update()
    {
        if (!_destructionStarted || _shockLayer == null)
            return;
        
        if (!_shockLayer.enabled)
            _shockLayer.enabled = true;

        float fadeValue = 1f - Mathf.InverseLerp(0f, 0.6f, EyeDestructionController.Instance.CurrentProgress);
        
        Vector3 direction = Vector3.Normalize(transform.position - _eyePosition.position);
        Matrix4x4 shockTransform = Matrix4x4.TRS(
            transform.position,
            Quaternion.LookRotation(direction, Vector3.up),
            Vector3.one
        );
        
        Vector4 textureST = _shockLayer.sharedMaterial.GetVector(_MainTex_ST);
        textureST.w = 0f - Time.timeSinceLevelLoad;

        _shockLayerBlock.SetColor(_Color, _shockLayerColor);
        _shockLayerBlock.SetMatrix(_WorldToShockLocalMatrix, shockTransform.inverse);
        _shockLayerBlock.SetVector(_Dir, direction);
        _shockLayerBlock.SetFloat(_Length, _shockLayerTrailLength);
        _shockLayerBlock.SetFloat(_Flare, _shockLayerTrailFlare);
        _shockLayerBlock.SetFloat(_TrailFade, fadeValue);
        _shockLayerBlock.SetFloat(_GradientLerp, 0f); // Alright?? Idk this is just in the original
        _shockLayerBlock.SetVector(_MainTex_ST, textureST);
        _shockLayer.SetPropertyBlock(_shockLayerBlock);
    }
}