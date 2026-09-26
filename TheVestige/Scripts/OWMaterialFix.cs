using System.Linq;
using UnityEngine;

namespace TheVestige.Scripts;

// The materials in my unity project seem to not work unless I reapply them
// from their equivalent runtime materials, *unless* I reselect the shader
public static class OWMaterialFix
{
    private static Material[] _materials;

    public static void RecacheMaterials() => _materials = Resources.FindObjectsOfTypeAll<Material>();
    
    public static void FixMaterials(this GameObject root)
    {
        if (_materials == null)
            return;
        
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var fixedMats = renderer.sharedMaterials;

            foreach (var oldMat in fixedMats)
            {
                if (oldMat == null) continue;

                var runtimeMaterial = FindRuntimeVariant(oldMat.shader.name);
                if (runtimeMaterial != null)
                    oldMat.shader = runtimeMaterial.shader;
            }

            renderer.sharedMaterials = fixedMats;
        }
    }

    private static Material FindRuntimeVariant(string bundledName)
    {
        return _materials.FirstOrDefault(m => m?.shader?.name == bundledName);
    }
}