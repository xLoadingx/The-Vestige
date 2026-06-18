using TheVestige.Scripts;
using UnityEngine;

namespace TheVestige;

public class TimelineObliterationTest : MonoBehaviour
{
    public enum Effect
    {
        NONE = 0,
        VOID_SHADOW = 1,
        WORLD_CRACKS = 2,
        FILL_ROOM = 3,
    }
    
    public VoidShadowEffectController targetObject;
    public TimelineObliterationEffect[] voidCrackEffectList;
    public RealityCrackBoxLimiter limiter;
    
    public float timeToCrackEffect = 0.6f;
    public float timeToFillEffect = 8.7f;
    
    public OWAudioSource voidCrackAudioSource;

    public Effect[] triggeredEffects = new Effect[3];

    public bool effectStarted;

    public float totalEffectTime;
    public float totalCrackTime;
    
    public int completedCracks;

    public VoidShadowEffectController voidShadowEffect;
    
    public Material fillRoomMaterial;

    public bool hasPlayedShadowAudio;
    public bool hasPlayedCrackAudio;

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
        voidShadowEffect = null;
        effectStarted = true;

        voidShadowEffect = targetObject;
        if (voidShadowEffect != null)
        {
            triggeredEffects[0] = Effect.VOID_SHADOW;
            voidCrackAudioSource.transform.parent = voidShadowEffect.transform;
            voidShadowEffect.PlayEffect();
            voidCrackAudioSource.transform.localPosition = Vector3.zero;
        }
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
        if (voidCrackEffectList != null)
        {
            foreach (var voidCrack in voidCrackEffectList)
            {
                voidCrack.OnCrackEffectComplete += OnCrackEffectComplete;
                voidCrack.transform.SetParent(voidShadowEffect.transform);
                voidCrack.transform.localPosition = voidShadowEffect.voidCracksParentOffset;
                voidCrack.PlayEffect();
            }
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
