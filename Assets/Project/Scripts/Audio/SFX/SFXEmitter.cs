using System.Collections;
using UnityEngine;

/// <summary>
/// Wrapper class for handling an audio source and an SFX audio clip
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SFXEmitter : MonoBehaviour
{
    private AudioSource _audioSource;
    private Coroutine _playingCoroutine;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Apply the audio source settings from a given <c>SFXDefinition</c>
    /// </summary>
    /// <param name="sfxDefinition"> The <c>SFXDefinition</c> to retrieve audio source settings from</param>
    public void Initialise(SFXDefinition sfxDefinition)
    {
        sfxDefinition.ApplySource(_audioSource);
        _audioSource.clip = sfxDefinition.GetAudioClip();
    }

    /// <summary>
    /// Play a given sound effect
    /// </summary>
    public void Play()
    {
        if (_playingCoroutine != null) StopCoroutine(_playingCoroutine);
        
        _audioSource.Play();
        _playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    /// <summary>
    /// Stop the active sound and return the emitter to the pool
    /// </summary>
    public void Stop()
    {
        if (_playingCoroutine != null)
        {
            StopCoroutine(_playingCoroutine);
            _playingCoroutine = null;
        }
        
        _audioSource.Stop();
        
        AudioManager.Instance.SFX.ReleaseToPool(this);
    }
    
    /// <summary>
    /// Waits for the audio source to finish playing, then automatically releases to the pool.
    /// </summary>
    private IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => _audioSource.isPlaying);
        AudioManager.Instance.SFX.ReleaseToPool(this);
    }
}