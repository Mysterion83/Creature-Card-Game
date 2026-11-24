using UnityEngine;

/// <summary>
/// Struct for defining a SFXDefinition with extra settings
/// </summary>
[System.Serializable]
public struct SFXProfile
{
    [Header("Audio")]
    public SFXDefinition SoundDefinition;

    [Header("Timing")]
    [Min(0f)] public float Delay;

    [Header("Placement Settings")]
    [Tooltip("Where this SFX should play: Target Object (True) or This Object (False)")]
    public bool PlayOnTarget;
    public Vector3 PositionOffset;
}