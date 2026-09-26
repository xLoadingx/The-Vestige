using UnityEngine;

namespace TheVestige.Scripts.LOC;

// Modified version of the TimelineObliterationController
public class CustomTimelineObliteration : MonoBehaviour
{
    public enum Effect
    {
        NONE = 0,
        VOID_SHADOW = 1,
        WORLD_CRACKS = 2,
        FILL_ROOM = 3,
    }
    
    [SerializeField] private TimelineObliterationEffect[] voidCrackEffectList;
    
    [SerializeField] private float timeToCrackEffect = 0.6f;
    [SerializeField] private float timeToFillEffect = 8.7f;
    
    [SerializeField] private OWAudioSource voidCrackAudioSource;

    private Effect[] triggeredEffects = new Effect[3];

    public bool effectStarted;

    private float totalEffectTime;
    private float totalCrackTime;
    
    private int completedCracks;

    [SerializeField] private VoidShadowEffectController voidShadowEffect;
    
    public Material fillRoomMaterial;

    private bool hasPlayedShadowAudio;
    private bool hasPlayedCrackAudio;

    public AudioClip fillRoomClip;

    private void Update()
    {
        if (effectStarted)
        {
            totalEffectTime += Time.deltaTime;
                
            if (!hasPlayedShadowAudio && totalEffectTime >= 1f)
            {
                voidCrackAudioSource.PlayOneShot(AudioType.TimelineEndEffect_Shadow);
                hasPlayedShadowAudio = true;
            }
            if (!hasPlayedCrackAudio && totalEffectTime > timeToCrackEffect + 2f)
            {
                voidCrackAudioSource.PlayOneShot(AudioType.TimelineEndEffect_Cracks);
                hasPlayedCrackAudio = true;
            }
            if (totalEffectTime > timeToCrackEffect && triggeredEffects[1] == Effect.NONE)
            {
                TriggerCrackEffect();
            }
            if (totalEffectTime > timeToFillEffect)
            {
                if (triggeredEffects[2] == Effect.NONE)
                {
                    triggeredEffects[2] = Effect.FILL_ROOM;
                    voidCrackAudioSource.PlayOneShot(fillRoomClip);
                }

                float elapsed = totalEffectTime - (timeToFillEffect + 0.6f);
                float progress = Mathf.Clamp01(elapsed / 0.2f);
                fillRoomMaterial.SetFloat("_Progression", progress);
            }
        }        
    }

    public void BeginTimelineObliteration()
    {
        effectStarted = true;

        if (voidShadowEffect != null)
        {
            triggeredEffects[0] = Effect.VOID_SHADOW;
            voidCrackAudioSource.transform.parent = voidShadowEffect.transform;
            voidShadowEffect.PlayEffect();
            voidCrackAudioSource.transform.localPosition = Vector3.zero;
        }
    }

    // Debug
    public void ResetTimelineObliteration()
    {
        effectStarted = false;

        totalEffectTime = 0f;
        totalCrackTime = 0f;
        completedCracks = 0;

        hasPlayedShadowAudio = false;
        hasPlayedCrackAudio = false;

        for (int i = 0; i < triggeredEffects.Length; i++)
            triggeredEffects[i] = Effect.NONE;
        
        fillRoomMaterial.SetFloat("_Progression", 0f);

        foreach (var voidCrack in voidCrackEffectList)
        {
            voidCrack.OnCrackEffectComplete -= OnCrackEffectComplete;

            voidCrack._isPlaying = false;
            voidCrack._isComplete = false;
            voidCrack._timer = 0f;
            voidCrack._renderer.material.SetFloat("_Progression", 0f);
        }

        voidShadowEffect._isPlaying = false;
        voidShadowEffect._isComplete = false;
        voidShadowEffect._effectStartTime = 0f;
        voidShadowEffect.Awake();
    }
    
    private void TriggerCrackEffect()
    {
        triggeredEffects[1] = Effect.WORLD_CRACKS;
        completedCracks = 0;
        totalCrackTime = 0f;
        
        foreach (var voidCrack in voidCrackEffectList)
        {
            if (voidCrack.effectTime > totalCrackTime)
            {
                totalCrackTime = voidCrack.effectTime;
            }
        }
        
        foreach (var voidCrack in voidCrackEffectList)
        {
            voidCrack.OnCrackEffectComplete += OnCrackEffectComplete;
            voidCrack.transform.SetParent(voidShadowEffect.transform);
            voidCrack.transform.localPosition = voidShadowEffect.voidCracksParentOffset;
            voidCrack.PlayEffect();
        }
    }

    private void OnCrackEffectComplete()
    {
        completedCracks++;
        if (completedCracks == voidCrackEffectList.Length)
        {
            foreach (var voidCrack in voidCrackEffectList)
            {
                voidCrack.OnCrackEffectComplete -= OnCrackEffectComplete;
            }
        }
    }
}
