using TheVestige.Scripts.Ending;
using UnityEngine;
using static TheVestige.Scripts.TheVestige;

namespace TheVestige.Scripts.Modules;

public class EyeStateModule : MonoBehaviour
{
    public enum VestigeEyeState
    {
        None,
        Exploration,
        Destruction
    }

    // Might have a transition state in the future, not sure yet
    public static readonly EyeState PreviousUniverse = (EyeState)45;

    private AssetBundle _eyeSceneBundle;
    
    private EyeDestructionController _eyeDestructionController;
    
    private VestigeEyeState _currentState = VestigeEyeState.None;

    public void Awake()
    {
        GlobalMessenger<EyeState>.AddListener("EyeStateChanged", OnEyeStateChanged);

        _eyeSceneBundle = bundleModule.Load("tv_eyescene");
    }

    // Temp: TESTING
    public void OnStarSystemLoaded(string systemName)
    {
        _eyeDestructionController = Instantiate(_eyeSceneBundle.LoadAsset<GameObject>("Destruction Skybox")).GetComponent<EyeDestructionController>();
        Instantiate(_eyeSceneBundle.LoadAsset<GameObject>("VestigeFadeScreenCanvas"));
        
        // Toggled off in bundle. Turning on here
        // makes it appear over all other skybox renderers
        // such as the stars
        
        _eyeDestructionController.GetComponent<SkyboxRenderer>().enabled = true;
    }
    // ------
    
    public void ResetState()
    {
        _currentState = VestigeEyeState.None;
    }

    private void OnEyeStateChanged(EyeState state)
    {
        _currentState = state == PreviousUniverse ? VestigeEyeState.Exploration : VestigeEyeState.None;
    }
}