using UnityEngine;

public enum FadeBehaviour
{
    Default,
    Instant,
    CustomDuration
}

public enum TrackType
{
    Music,
    Ambience,
    Stinger
}

[CreateAssetMenu(fileName = "New Music Definition", menuName = "Project/Audio/Music Definition")]
public class MusicDefinition : SoundDefinition
{
    [Header("Music Clip")]
    [Tooltip("The music clip to define")]
    public AudioClip Clip;

    [Header("Music Settings")]
    [Tooltip("What layer this track should operate on.\n\n" +
             "Music: Main music track, overrides the previous music track\n" +
             "Ambience: Layers over music, used for background noise like wind\n" +
             "Stinger: Ducks existing tracks while playing, used for short clips like winning a level")]
    public TrackType TrackType = TrackType.Music;
    [Tooltip("Whether this sound should loop or not")]
    public bool Loop;
    
    [Header("Fade In Settings")]
    [Tooltip("How this track should behave when starting.\n\n" +
             "Default: Use the default fade time specified in MusicManager\n" +
             "Instant: Instantly play when started with no fade\n" +
             "CustomDuration: Use a specified custom fade duration")]
    public FadeBehaviour FadeInMode = FadeBehaviour.Default;
    [Tooltip("Time in seconds to fade in this track.")]
    [Min(0)] public float FadeInTime = 1f;
    
    [Header("Fade Out Settings")]
    [Tooltip("How this track should behave when stopping.\n\n" +
             "Default: Use the default fade time specified in MusicManager\n" +
             "Instant: Instantly play when started with no fade\n" +
             "CustomDuration: Use a specified custom fade duration\n\n" +
             "Warning: Ensure fade out for Stingers, as using 'Instant' can cause popping")]
    public FadeBehaviour FadeOutMode = FadeBehaviour.Default;
    [Tooltip("Custom fade out duration used if FadeOutMode is set to CustomDuration.")]
    [Min(0)] public float FadeOutTime = 1f;
    
    public override void ApplySource(AudioSource source)
    {
        base.ApplySource(source);
        
        source.clip = Clip;
        source.loop = TrackType != TrackType.Stinger && Loop; // Don't enable looping for stingers
    }

    public override bool IsValid()
    {
        base.IsValid();

        if (Clip) return true;
        
        Debug.LogWarning($"[MusicDefinition] Music definition '{name}' is missing an AudioClip!", this);
        return false;
    }
    
    public float GetFadeInDuration(float defaultFadeInTime) => 
        GetFadeDuration(FadeInMode, FadeInTime, defaultFadeInTime);

    public float GetFadeOutDuration(float defaultFadeOutTime) => 
        GetFadeDuration(FadeOutMode, FadeOutTime, defaultFadeOutTime);

    private float GetFadeDuration(FadeBehaviour behavior, float customTime, float defaultTime)
    {
        return behavior switch
        {
            FadeBehaviour.Instant => 0f,
            FadeBehaviour.CustomDuration => customTime,
            _ => defaultTime
        };
    }
}