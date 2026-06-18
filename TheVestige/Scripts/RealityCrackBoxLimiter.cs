using UnityEngine;

namespace TheVestige.Scripts;

public class RealityCrackBoxLimiter : MonoBehaviour
{
    public Renderer[] voidCrackRenderers;

    private static readonly int WorldToBoxID = Shader.PropertyToID("_WorldToBox");
    private static readonly int BoxHalfSizeID = Shader.PropertyToID("_BoxHalfSize");

    private MaterialPropertyBlock _block;
    
    private void Awake()
    {
        _block = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        Matrix4x4 worldToBox = transform.worldToLocalMatrix;

        Vector3 halfSize = Vector3.one * 0.5f;

        foreach (Renderer r in voidCrackRenderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(_block);
            _block.SetMatrix(WorldToBoxID, worldToBox);
            _block.SetVector(BoxHalfSizeID, halfSize);
            r.SetPropertyBlock(_block);
        }
    }
}