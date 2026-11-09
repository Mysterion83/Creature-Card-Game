using UnityEngine;

/// <summary>
/// Base class for SFX Definitions, inherits from <c>SoundDefinition</c>
/// </summary>
public abstract class SFXDefinition : SoundDefinition
{
    [Header("Playback Variance")]
    [Tooltip("Random volume multiplier applied on play")]
    public Vector2 VolumeVariance = new(0.95f, 1.05f);
    [Tooltip("Random pitch multiplier applied on play")]
    public Vector2 PitchVariance = new(0.95f, 1.05f);

    [Header("Spatial Settings")]
    [Tooltip("Pan position between left (-1) and right (1), leave 0 for equal panning (only works for stereo or mono)")]
    [Range(-1f, 1f)] public float StereoPanning;
    [Tooltip("0 for 2D UI audio (no spatialization), 1 for 3D spatialized audio.")]
    [Range(0f, 1f)] public float SpatialBlend;
    [Tooltip("The mix amount of reverb if this source is in a reverb zone")]
    [Range(0f, 1.1f)] public float ReverbZoneMix = 1.0f;
    
    [Header("Rolloff")]
    [Tooltip("The minimum distance at which the sound can be heard.")]
    public float MinDistance = 1f;
    [Tooltip("The maximum distance at which the sound can still be heard.")]
    public float MaxDistance = 20f;
    [Tooltip("The curve model used for how sound volume drops off over distance.")]
    public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Custom Rolloff Curve")]
    [Tooltip("Enable to use a custom AnimationCurve for distance attenuation instead of standard Unity rolloff modes.")]
    public bool UseCustomRolloffCurve;
    [Tooltip("X-axis represents a normalized distance (0 min, 1 max). Y-axis represents volume multiplier (0x to 1x).")]
    public AnimationCurve CustomRolloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    
    [Header("Bypasses")]
    [Tooltip("Bypass effect filters (Reverb, Chorus, etc.) applied to the Audio Mixer.")]
    public bool BypassEffects;
    [Tooltip("Ignores global audio filters attached to the AudioListener (useful for 2D and sounds that shouldn't be affected by environmental filters).")]
    public bool BypassListenerEffects;
    [Tooltip("Ignore any reverb zone this audio source may be in.")]
    public bool BypassReverbZones;
    
    [Header("Pooling and Priority")]
    [Tooltip("Priority of the sound source (0 = most important, 256 = least important). Used to rectify 'voice stealing'.")]
    [Range(0, 256)] public int Priority = 128;

    /*
    Above is an Audio priority field, from 0-256. Higher amounts mean the sound must absolutely be heard.
    I researched into how audio works in Unity, something called voice stealing occurs when the number of active
    sources is greater than the engine's limit (specified in project settings), so it cuts off or mutes active sounds.
    To fix this you can apparently set priority for Audio Sources to prioritise sounds over others:
    https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/audiosource/priority
    Due to how priority works, it is an integer value between 0 and 256.
    */
    
    public override void ApplySource(AudioSource source)
    {
        source.outputAudioMixerGroup = MixerGroupOverride;
        
        // Playback Randomisation
        // Swap Min/Max if entered wrong in the Inspector
        float minVol = Mathf.Min(VolumeVariance.x, VolumeVariance.y);
        float maxVol = Mathf.Max(VolumeVariance.x, VolumeVariance.y);
        source.volume = Random.Range(minVol, maxVol) * BaseVolume;

        float minPitch = Mathf.Min(PitchVariance.x, PitchVariance.y);
        float maxPitch = Mathf.Max(PitchVariance.x, PitchVariance.y);
        source.pitch = Random.Range(minPitch, maxPitch) * BasePitch;

        // Spatial
        source.panStereo = StereoPanning;
        source.spatialBlend = SpatialBlend;
        source.reverbZoneMix = ReverbZoneMix;
        
        // Rolloff
        source.minDistance = MinDistance;
        source.maxDistance = MaxDistance;
        source.rolloffMode = RolloffMode;

        if (UseCustomRolloffCurve)
        {
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, CustomRolloffCurve);
        }

        // Bypasses
        source.bypassEffects = BypassEffects;
        source.bypassListenerEffects = BypassListenerEffects;
        source.bypassReverbZones = BypassReverbZones;
        
        source.priority = Priority;
    }

    public override bool IsValid()
    {
        base.IsValid();
        
        if (MinDistance < 0f) return false;
        return !(MaxDistance <= MinDistance);
    }
    
    public abstract AudioClip GetAudioClip();
}