using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MusicManager : MonoBehaviour
{
    [Header("Music Emitter Settings")]
    [SerializeField] private MusicEmitter _musicEmitterPrefab;
    [SerializeField] private Transform _container;
    
    [Header("Crossfade Defaults")]
    [Tooltip("Default time in seconds to fade into a track when switching if no override is specified.")]
    [SerializeField] private float _defaultFadeInTime = 1f;
    [Tooltip("Default time in seconds to fade out a track when switching if no override is specified.")]
    [SerializeField] private float _defaultFadeOutTime = 1f;

    [Header("Sting Ducking Settings")]
    [Tooltip("Volume multiplier applied to background music when a stinger plays, lower values equal more ducking.")]
    [SerializeField, Range(0f, 1f)] private float _stingerDuckMultiplier = 0.4f;
    
    [Header("Pool Settings")]
    [Tooltip("The default pool capacity, will reduce down to this size when extra capacity is not needed")]
    [SerializeField] private int _defaultCapacity = 2;
    [Tooltip("The absolute maximum pool capacity, will not increase past this size")]
    [SerializeField] private int _maxCapacity = 5;
    [Tooltip("Enable to double check the collection, helps prevent dual-release mistakes")]
    [SerializeField] private bool _doCollectionCheck;
    
    private IObjectPool<MusicEmitter> _musicEmitterPool;
    public readonly List<MusicEmitter> ActiveMusicEmitters = new();

    private Coroutine _fadeCoroutine;

    private int _activeStingerCount;
    
    private void Awake()
    {
        _musicEmitterPool = new ObjectPool<MusicEmitter>(
            createFunc: CreateMusicEmitter,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyedPoolObject,
            collectionCheck: _doCollectionCheck,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxCapacity
        );
    }
    
    #region Playing
    /// <summary>
    /// Play a music track from a given <c>MusicDefinition</c>
    /// </summary>
    /// <param name="musicDefinition">The <c>MusicDefinition</c> asset to play</param>
    /// <returns>The active or new <c>MusicEmitter</c> playing this music</returns>
    public MusicEmitter Play(MusicDefinition musicDefinition)
    {
        if (!musicDefinition || !musicDefinition.IsValid())
        {
            Debug.LogWarning("[MusicManager] Tried to play a null music definition or is missing an audio clip!", this);
            return null;
        }
    
        // Return its active emitter if already playing
        foreach (var emitter in ActiveMusicEmitters)
        {
            if (emitter.CurrentDefinition == musicDefinition)
            {
                return emitter;
            }
        }

        MusicEmitter nextEmitter = _musicEmitterPool.Get();
        nextEmitter.Initialise(musicDefinition);

        float fadeIn = musicDefinition.GetFadeInDuration(_defaultFadeInTime);
        float fadeOut = musicDefinition.GetFadeOutDuration(_defaultFadeOutTime);

        // Crossfade into or play the next music track
        switch (musicDefinition.TrackType)
        {
            case TrackType.Music:
            {
                if (_fadeCoroutine != null)
                {
                    StopCoroutine(_fadeCoroutine);
                    _fadeCoroutine = null;
                }

                _fadeCoroutine = StartCoroutine(CrossFadeRoutine(nextEmitter, fadeIn, fadeOut));
                break;
            }
            case TrackType.Ambience: // Layers on top of music
            {
                nextEmitter.Play(fadeIn);
                break;
            }
            case TrackType.Stinger: // Layers on top of everything
            {
                StartCoroutine(StingerDuckRoutine(nextEmitter, musicDefinition));
                break;
            }
        }

        return nextEmitter;
    }
    #endregion
    
    #region Stopping
    /// <summary>
    /// Stop a specified music track from playing from a provided <c>MusicDefinition</c>
    /// </summary>
    public void Stop(MusicDefinition musicDefinition)
    {
        if (!musicDefinition || !musicDefinition.IsValid()) return;

        MusicEmitter targetEmitter = ActiveMusicEmitters.Find(emitter => emitter.CurrentDefinition == musicDefinition);
        if (!targetEmitter) return;
        
        float duration = musicDefinition.GetFadeOutDuration(_defaultFadeOutTime);
        targetEmitter.Stop(duration);
    }

    /// <summary>
    /// Force stop a specified music track
    /// </summary>
    public void StopInstant(MusicDefinition musicDefinition)
    {
        if (!musicDefinition || !musicDefinition.IsValid()) return;

        MusicEmitter targetEmitter = ActiveMusicEmitters.Find(emitter => emitter.CurrentDefinition == musicDefinition);
        if (!targetEmitter) return;
        
        targetEmitter.Stop();
    }

    /// <summary>
    /// Stop all active music tracks over time
    /// <param name="fadeOutTime">Optional: Time to fade out, leave unchanged for the default fade-out duration</param>
    /// </summary>
    public void StopAll(float fadeOutTime = -1f)
    {
        if (_fadeCoroutine != null) 
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
    
        float duration = fadeOutTime >= 0f ? fadeOutTime : _defaultFadeOutTime;
        
        // Make a copy of the list to iterate over
        List<MusicEmitter> emittersToStop = new(ActiveMusicEmitters);
        foreach (var emitter in emittersToStop)
        {
            if (emitter) emitter.Stop(duration);
        }
    }

    /// <summary>
    /// Stop all active music tracks instantly
    /// </summary>
    public void StopAllInstant()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        // Make a copy of the list to iterate over
        List<MusicEmitter> emittersToStop = new(ActiveMusicEmitters);
        foreach (var emitter in emittersToStop)
        {
            if (emitter) emitter.Stop();
        }
    }
    #endregion
    
    #region Pause & Resume
    public void Pause(MusicDefinition musicDefinition)
    {
        if (!musicDefinition) return;
        MusicEmitter target = ActiveMusicEmitters.Find(emitter => emitter.CurrentDefinition == musicDefinition);
        if (target) target.Pause();
    }

    public void PauseAll()
    {
        foreach (var emitter in ActiveMusicEmitters)
        {
            if (emitter) emitter.Pause();
        }
    }

    public void Resume(MusicDefinition musicDefinition)
    {
        if (!musicDefinition) return;
        MusicEmitter target = ActiveMusicEmitters.Find(emitter => emitter.CurrentDefinition == musicDefinition);
        if (target) target.UnPause();
    }

    public void ResumeAll()
    {
        foreach (var emitter in ActiveMusicEmitters)
        {
            if (emitter) emitter.UnPause();
        }
    }
    #endregion
    
    #region Coroutines
    /// <summary>
    /// Crossfades music tracks together, excludes ambience and stingers.
    /// </summary>
    private IEnumerator CrossFadeRoutine(MusicEmitter nextEmitter, float fadeInTime, float fadeOutTime)
    {
        List<MusicEmitter> emittersToFadeOut = new();
        foreach (var emitter in ActiveMusicEmitters)
        {
            if (emitter != nextEmitter && 
                emitter.CurrentDefinition && 
                emitter.CurrentDefinition.TrackType == TrackType.Music)
            {
                emittersToFadeOut.Add(emitter);
            }
        }
        
        nextEmitter.Play(fadeInTime);
        foreach (var emitter in emittersToFadeOut)
        {
            emitter.Stop(fadeOutTime);
        }

        float maxFadeTime = Mathf.Max(fadeInTime, fadeOutTime);
        yield return new WaitForSecondsRealtime(maxFadeTime);

        _fadeCoroutine = null;
    }
    
    /// <summary>
    /// Plays a stinger and temporarily ducks active music tracks.
    /// </summary>
    private IEnumerator StingerDuckRoutine(MusicEmitter emitter, MusicDefinition musicDefinition)
    {
        _activeStingerCount++;
        emitter.Play(musicDefinition.GetFadeInDuration(_defaultFadeInTime));
    
        AudioClip clip = musicDefinition.Clip;
        if (!clip) 
        {
            _activeStingerCount--;
            yield break;
        }
    
        float stingerDuration = clip.length;
        float inDuration = musicDefinition.GetFadeInDuration(_defaultFadeInTime);
        float outDuration = musicDefinition.GetFadeOutDuration(_defaultFadeOutTime);

        // Collect emitters to duck
        Dictionary<MusicEmitter, float> originalVolumes = new Dictionary<MusicEmitter, float>();
        foreach (var activeEmitter in ActiveMusicEmitters)
        {
            if (activeEmitter != emitter && 
                activeEmitter.CurrentDefinition && 
                activeEmitter.CurrentDefinition.TrackType != TrackType.Stinger)
            {
                originalVolumes[activeEmitter] = activeEmitter.CurrentDefinition.BaseVolume;
            }
        }

        // Duck the music
        if (originalVolumes.Count > 0 && inDuration > 0f)
        {
            yield return FadeVolumes(originalVolumes, 1f, _stingerDuckMultiplier, inDuration);
        }

        // Await the main duration of the clip
        float waitTime = Mathf.Max(0f, stingerDuration - inDuration - outDuration);
        yield return new WaitForSecondsRealtime(waitTime);

        _activeStingerCount--;

        // Restore music only if all stingers are finished
        if (_activeStingerCount != 0 || originalVolumes.Count <= 0) yield break;
    
        if (outDuration > 0f)
        {
            yield return FadeVolumes(originalVolumes, _stingerDuckMultiplier, 1f, outDuration);
        }
        else
        {
            // Fallback if outDuration is 0
            foreach (var pair in originalVolumes)
            {
                if (pair.Key) pair.Key.SetVolume(pair.Value);
            }
        }
    }

    // Helper coroutine for fading volumes
    private IEnumerator FadeVolumes(Dictionary<MusicEmitter, float> volumes, float startMultiplier, float endMultiplier, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;

            foreach (var pair in volumes)
            {
                if (!pair.Key) continue;
                
                float startVol = pair.Value * startMultiplier;
                float endVol = pair.Value * endMultiplier;
                pair.Key.SetVolume(Mathf.Lerp(startVol, endVol, progress));
            }
            yield return null;
        }
    }
    #endregion
    
    #region Pooling Helpers
    /// <summary>
    /// Wrapper method for releasing a <c>SFXEmitter</c> back to the pool
    /// </summary>
    public void ReleaseToPool(MusicEmitter musicEmitter) => _musicEmitterPool.Release(musicEmitter);
    #endregion
    
    #region IObjectPool Callback Methods
    private MusicEmitter CreateMusicEmitter()
    {
        MusicEmitter emitter = Instantiate(_musicEmitterPrefab, _container);
        emitter.gameObject.SetActive(false);
        return emitter;
    }
    
    private void OnTakeFromPool(MusicEmitter emitter)
    {
        emitter.gameObject.SetActive(true);
        ActiveMusicEmitters.Add(emitter);
    }
    
    private void OnReturnedToPool(MusicEmitter emitter)
    {
        emitter.gameObject.SetActive(false);
        ActiveMusicEmitters.Remove(emitter);
    }
    
    private void OnDestroyedPoolObject(MusicEmitter emitter)
    {
        if (!emitter) return; 
        Destroy(emitter.gameObject);
    }
    #endregion
}