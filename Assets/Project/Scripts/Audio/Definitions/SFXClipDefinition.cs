using UnityEngine;

[CreateAssetMenu(fileName = "New SFX Clip Definition", menuName = "Project/Audio/SFX Clip Definition")]
public class SFXClipDefinition : SFXDefinition
{
    [Header("Audio Clip")]
    public AudioClip AudioClip;

    public override bool IsValid()
    {
        if (!base.IsValid()) return false;

        if (AudioClip) return true;
        
        Debug.LogWarning($"[SFXClipDefinition] '{name}' is missing an audio clip!");
        return false;
    }

    public override AudioClip GetAudioClip() => AudioClip;
}