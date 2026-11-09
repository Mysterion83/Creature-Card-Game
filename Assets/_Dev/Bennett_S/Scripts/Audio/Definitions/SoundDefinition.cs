using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Base class for all sound definitions
/// </summary>
public abstract class SoundDefinition : ScriptableObject
{
    [Header("Mixer Routing")]
    public AudioMixerGroup MixerGroupOverride;

    [Header("Playback")]
    [Tooltip("Base volume for the sound")]
    [Range(0f, 1f)] public float BaseVolume = 1.0f;
    [Tooltip("Base pitch for the sound")]
    [Range(-3f, 3f)] public float BasePitch = 1.0f;
    
    public virtual void ApplySource(AudioSource source)
    {
        source.outputAudioMixerGroup = MixerGroupOverride;
        
        // Playback
        source.volume = BaseVolume;
        source.pitch = BasePitch;
    }
    
    public virtual bool IsValid()
    {
        return true;
    }
}