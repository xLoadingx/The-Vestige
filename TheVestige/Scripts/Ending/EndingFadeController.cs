using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

namespace TheVestige.Scripts.Ending;

// TODO:
// Fix the text not showing up

// A lot of this is similar to the EchoesOverController :P
public class EndingFadeController : MonoBehaviour
{
    [SerializeField] private Image _fadeImage;

    private float _fadeStartTime;
    private float _fadeDuration;
    private bool _fading;

    [SerializeField] private Text _titleText;

    private bool _fadeInTextStarted;
    private bool _fadeOutTextStarted;

    private float _fadeInTextTime = float.PositiveInfinity;
    private float _fadeOutTextTime = float.PositiveInfinity;
    private float _toTitleScreenTime = float.PositiveInfinity;

    private void Awake()
    {
        _fadeImage.color = Color.clear;
        _titleText.canvasRenderer.SetAlpha(0f);
        
        GlobalMessenger<float>.AddListener("OnVestigeEndFade", StartEndingFade);
    }

    private void OnDestroy()
    {
        GlobalMessenger<float>.RemoveListener("OnVestigeEndFade", StartEndingFade);
    }

    public void StartEndingFade(float duration)
    {
        _fadeDuration = Max(duration, 0.001f);
        _fadeStartTime = Time.unscaledTime;
        _fading = true;
    }

    private void Update()
    {
        if (_fading)
        {
            float progress = (Time.unscaledTime - _fadeStartTime) / _fadeDuration;

            _fadeImage.color = progress < 1f
                ? new Color(1f, 1f, 1f, Clamp01(progress))
                : Color.Lerp(Color.white, Color.black, progress - 1f);
            
            AudioListener.volume = 1f - Clamp01(progress - 1f);

            if (progress >= 2f)
            {
                _fading = false;
                _fadeInTextTime = Time.time + 2f;
            }
        }

        // Title text
        
        if (!_fadeInTextStarted && Time.time >= _fadeInTextTime)
        {
            _titleText.canvasRenderer.SetAlpha(0f);
            _fadeInTextStarted = true;
            _fadeOutTextTime = Time.time + 4f;
            _titleText.CrossFadeAlpha(1f, 2f, true);
        } else if (!_fadeOutTextStarted && Time.time > _fadeOutTextTime)
        {
            _fadeOutTextStarted = true;
            _toTitleScreenTime = Time.time + 4f;
            _titleText.CrossFadeAlpha(0f, 2f, true);
        } else if (Time.time >= _toTitleScreenTime)
        {
            LoadManager.LoadScene(OWScene.TitleScreen);
        }
    }

    public void Reset()
    {
        _toTitleScreenTime = float.PositiveInfinity;
        _fadeInTextStarted = false;
        _fadeOutTextStarted = false;
        _fadeInTextTime = float.PositiveInfinity;
        _fadeOutTextTime = float.PositiveInfinity;
        _titleText.canvasRenderer.SetAlpha(0f);
        _fadeImage.color = Color.clear;
    }
}