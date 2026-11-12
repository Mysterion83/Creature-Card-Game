using UnityEngine;

public class MusicTest : MonoBehaviour
{
    [Header("Test Definitions")]
    [SerializeField] private MusicDefinition _trackA;
    [SerializeField] private MusicDefinition _trackB;
    [SerializeField] private MusicDefinition _ambienceTrack;
    [SerializeField] private MusicDefinition _stingerTrack;
    
    private MusicManager _musicManager;
    
    private void Start()
    { 
        _musicManager = AudioManager.Instance.Music;
    }

    public void PlayTrackA()
    {
        if (_musicManager && _trackA) _musicManager.Play(_trackA);
    }

    public void PlayTrackB()
    {
        if (_musicManager && _trackB) _musicManager.Play(_trackB);
    }

    public void PlayAmbience()
    {
        if (_musicManager && _ambienceTrack) _musicManager.Play(_ambienceTrack);
    }
    
    public void PlayStinger()
    {
        if (_musicManager && _stingerTrack) _musicManager.Play(_stingerTrack);
    }

    public void StopTest()
    {
        if (_musicManager) _musicManager.StopAll();
    }
}
