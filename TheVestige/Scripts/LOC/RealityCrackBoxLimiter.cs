using UnityEngine;

namespace TheVestige.Scripts.LOC;

public class RealityCrackBoxLimiter : MonoBehaviour
{
    [SerializeField] private Renderer[] voidCrackRenderers;

    private static readonly int WorldToBoxID = Shader.PropertyToID("_WorldToBox");
    private static readonly int BoxHalfSizeID = Shader.PropertyToID("_BoxHalfSize");

    private MaterialPropertyBlock block;
    
    private void Awake()
    {
        block = new MaterialPropertyBlock();
    }

    private void LateUpdate()
    {
        Matrix4x4 worldToBox = transform.worldToLocalMatrix;

        Vector3 halfSize = Vector3.one * 0.5f;

        foreach (Renderer r in voidCrackRenderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(block);
            block.SetMatrix(WorldToBoxID, worldToBox);
            block.SetVector(BoxHalfSizeID, halfSize);
            r.SetPropertyBlock(block);
        }
    }
}