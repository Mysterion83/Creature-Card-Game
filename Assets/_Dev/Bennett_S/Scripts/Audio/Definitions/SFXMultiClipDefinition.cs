using UnityEngine;

[CreateAssetMenu(fileName = "New SFX Multi-Clip Definition", menuName = "Project/Audio/SFX Multi-Clip Definition")]
public class SFXMultiClipDefinition : SFXDefinition
{
    [Header("Clips Playlist")]
    [Tooltip("Array of audio clips where one will be selected randomly on play.")]
    [SerializeField] private AudioClip[] _audioClips;
    
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (_audioClips != null && _audioClips.Length != 0) return true;
        
        Debug.LogWarning($"[SFXMultiClipDefinition] '{name}' is missing audio clips!");
        return false;
    }

    public override AudioClip GetAudioClip()
    {
        if (_audioClips == null || _audioClips.Length == 0) return null;
        // Return a random clip or index 0 if its only one clip
        return _audioClips.Length == 1 ? 
            _audioClips[0] :
            _audioClips[Random.Range(0, _audioClips.Length)];
    }
}