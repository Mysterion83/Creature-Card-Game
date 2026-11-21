using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New VFXDeathProfile", menuName = "Project/VFX/VFXDeathProfile")]
public class VFXDeathProfile : ScriptableObject
{
    [Header("Profile List")]
    [SerializeField] private List<VFXProfile> _effects = new();
    public IReadOnlyList<VFXProfile> Effects => _effects;
}