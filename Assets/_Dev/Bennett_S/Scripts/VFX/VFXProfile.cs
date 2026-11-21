using UnityEngine;

/// <summary>
/// Struct for defining a VFX prefab with extra settings
/// </summary>
[System.Serializable]
public struct VFXProfile
{
    [Header("Prefab")]
    public GameObject VFXPrefab;

    [Header("Timing")]
    [Min(0f)] public float Delay;

    [Header("Placement Settings")]
    [Tooltip("What this VFX should spawn on: \n\nTarget Object (True)\nThis Object (False)")]
    public bool SpawnOnTarget;
    [Tooltip("If this VFX attaches to an Anchor so it follows a parent object's movement and rotation." +
             "\n\nIMPORTANT: Disable this for death effects to avoid VFX becoming destroyed with the parent object.")]
    public bool ParentToAnchor;
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    [Header("Visual Modifiers")]
    public bool TintWithElementColor;
}