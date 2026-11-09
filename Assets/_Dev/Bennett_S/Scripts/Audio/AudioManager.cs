using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(MusicManager), typeof(SFXManager))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    public MusicManager Music { get; private set; }
    public SFXManager SFX { get; private set; }
    
    [Header("Audio Mixer Settings")]
    [Tooltip("The main Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;
    
    [Header("Mixer Snapshots")]
    [Tooltip("Dictionary of Audio Mixer Snapshots, used for audio effects for different game scenarios, like muffling on pause or in a menu")]
    [SerializeField] private Dictionary<string, AudioMixerSnapshot> _snapshots = new();
    [Tooltip("The default time it takes in seconds to transition between Audio Mixer Snapshots")]
    [SerializeField, Min(0)] private float _defaultTransitionTime = 0.25f;
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); 
            return;
        }
        
        Music = GetComponent<MusicManager>();
        SFX = GetComponent<SFXManager>();
    }
    
    /// <summary>
    /// Transition to a new Audio Mixer Snapshot
    /// </summary>
    /// <param name="snapshotName">The name of the snapshot to transition to</param>
    /// <param name="customTime">Optional parameter for transition time, uses default time if not specified</param>
    public void TransitionToSnapshot(string snapshotName, float customTime = -1f)
    {
        if (string.IsNullOrEmpty(snapshotName)) return;

        if (_snapshots.TryGetValue(snapshotName, out var snapshot) && snapshot)
        {
            float time = customTime >= 0f ? customTime : _defaultTransitionTime;
            snapshot.TransitionTo(time);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Snapshot '{snapshotName}' not found in the dictionary!", this);
        }
    }
}