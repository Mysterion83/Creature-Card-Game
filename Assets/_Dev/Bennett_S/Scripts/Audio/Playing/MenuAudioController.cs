using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private MusicDefinition _menuMusic;
    
    [Header("UI SFX")]
    [SerializeField] private SFXClipDefinition _navigation;
    [SerializeField] private SFXClipDefinition _open;
    [SerializeField] private SFXClipDefinition _close;
    [SerializeField] private SFXClipDefinition _next;
    [SerializeField] private SFXClipDefinition _back;

    private AudioManager _audioManager;

    private void Start()
    {
        _audioManager = AudioManager.Instance;
        _audioManager.Music.StopAllInstant();
        _audioManager.Music.Play(_menuMusic);
    }

    public void PlayNavigation()
    {
        _audioManager.SFX.Play(_navigation);
    }
    public void PlayOpen()
    {
        _audioManager.SFX.Play(_open);
    }
    public void PlayClose()
    {
        _audioManager.SFX.Play(_close);
    }
    public void PlayNext()
    {
        _audioManager.SFX.Play(_next);
    }
    public void PlayBack()
    {
        _audioManager.SFX.Play(_back);
    }
}
