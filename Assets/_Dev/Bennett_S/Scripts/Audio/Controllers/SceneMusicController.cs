using System.Collections.Generic;
using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [Header("Music to Play")]
    [SerializeField] private MusicDefinition _musicTrack;
    [SerializeField] private bool _stopEverythingOnPlay;

    [Header("Ambience to Play")]
    [SerializeField] private List<MusicDefinition> _ambienceTracks = new();
    
    private void Start()
    {
        if (_stopEverythingOnPlay) AudioManager.Instance.Music.StopAll();
        
        if (_musicTrack)
        {
            AudioManager.Instance.Music.Play(_musicTrack);
        }
        
        if (_ambienceTracks == null || _ambienceTracks.Count == 0)
        {
            return;
        }
        
        foreach (MusicDefinition ambienceTrack in _ambienceTracks)
        {
            if (!ambienceTrack) continue;
            AudioManager.Instance.Music.Play(ambienceTrack);
        }
    }
}