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
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    [Header("Visual Modifiers")]
    public bool TintWithElementColor;
}