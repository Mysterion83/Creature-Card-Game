using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Base class for all sound definitions
/// </summary>
public abstract class SoundDefinition : ScriptableObject
{
    [Header("Mixer Routing")]
    public AudioMixerGroup MixerGroupOverride;
    
    [Header("Pooling and Priority")]
    [Tooltip("Priority of the sound source (0 = most important, 256 = least important). Used to rectify 'voice stealing'.")]
    [Range(0, 256)] public int Priority = 128;

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
                
        source.priority = Priority;
    }
    
    public virtual bool IsValid()
    {
        return true;
    }
}