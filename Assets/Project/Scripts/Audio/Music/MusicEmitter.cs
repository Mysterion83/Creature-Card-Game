using System.Collections;
using UnityEngine;

/// <summary>
/// Wrapper class for handling an audio source and a Music audio clip
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicEmitter : MonoBehaviour
{
    private AudioSource _audioSource;
    private MusicDefinition _currentDefinition;
    private Coroutine _fadeCoroutine;
    private Coroutine _playbackCoroutine;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public MusicDefinition CurrentDefinition => _currentDefinition;

    /// <summary>
    /// Apply the audio source settings from a given <c>MusicDefinition</c>
    /// </summary>
    /// <param name="musicDefinition"> The <c>MusicDefinition</c> to retrieve audio source settings from</param>
    public void Initialise(MusicDefinition musicDefinition)
    {
        _currentDefinition = musicDefinition;
        _currentDefinition.ApplySource(_audioSource);
    }

    /// <summary>
    /// Play this track
    /// </summary>
    /// <param name="fadeInDuration">Optional: fade in duration, leave if sound is instant</param>
    public void Play(float fadeInDuration = 0f)
    {
        if (_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        // If looping is disabled, automatically release back to the pool when finished
        if (_currentDefinition && !_currentDefinition.Loop)
        {
            _playbackCoroutine = StartCoroutine(WaitForTrackToEnd());
        }

        if (fadeInDuration <= 0f)
        {
            _audioSource.volume = _currentDefinition ? _currentDefinition.BaseVolume : 1f;
            if (!_audioSource.isPlaying) _audioSource.Play();
        }
        else
        {
            float targetVolume = _currentDefinition ? _currentDefinition.BaseVolume : 1f;
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, targetVolume, fadeInDuration, false));
        }
    }

    /// <summary>
    /// Stop this music track
    /// </summary>
    /// <param name="fadeOutDuration">Optional: fade out duration, leave if sound is instant</param>
    public void Stop(float fadeOutDuration = 0f)
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        if (fadeOutDuration <= 0f)
        {
            ReleaseToPool();
        }
        else
        {
            _fadeCoroutine = StartCoroutine(FadeRoutine(_audioSource.volume, 0f, fadeOutDuration, true));
        }
    }

    // Wrapper methods
    public void SetVolume(float volume) => _audioSource.volume = volume;
    public void Pause() => _audioSource.Pause();
    public void UnPause() => _audioSource.UnPause();

    #region Coroutines
    private IEnumerator FadeRoutine(float startVol, float targetVol, float duration, bool releaseOnComplete)
    {
        if (!_audioSource.isPlaying && targetVol > 0f)
        {
            _audioSource.Play();
        }

        float timer = 0f;
        _audioSource.volume = startVol;

        // Use unscaledDeltaTime as coroutines mess up time, lerp the volume over the start and target volume
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVol, targetVol, timer / duration);
            yield return null;
        }
        _audioSource.volume = targetVol;

        if (releaseOnComplete)
        {
            ReleaseToPool();
        }
        _fadeCoroutine = null;
    }

    /// <summary>
    /// Waits for the audio source to finish playing, then automatically releases to the pool
    /// </summary>
    private IEnumerator WaitForTrackToEnd()
    {
        yield return new WaitUntil(() => _audioSource.isPlaying);
        yield return new WaitWhile(() => _audioSource.isPlaying);
        ReleaseToPool();
    }
    #endregion
    
    private void ReleaseToPool()
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        _audioSource.Stop();
        _audioSource.volume = 0f;
        
        AudioManager.Instance.Music.ReleaseToPool(this);
    }
}