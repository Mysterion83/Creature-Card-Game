using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFXDamageProfile", menuName = "Project/Audio/Profiles/SFXDamageProfile")]
public class SFXDamageProfile : ScriptableObject
{
    [Header("Profile Lists (By DamageType)")]
    [SerializeField] private List<SFXProfile> _normalSounds = new();
    [SerializeField] private List<SFXProfile> _advantageSounds = new();
    [SerializeField] private List<SFXProfile> _disadvantageSounds = new();

    public IReadOnlyList<SFXProfile> NormalSounds => _normalSounds;
    public IReadOnlyList<SFXProfile> AdvantageSounds => _advantageSounds;
    public IReadOnlyList<SFXProfile> DisadvantageSounds => _disadvantageSounds;

    /// <summary>
    /// Get a specific sound list from a provided DamageType
    /// </summary>
    /// <param name="damageType">The damageType to compare</param>
    /// <returns>The sound list depending on the damage type</returns>
    public IReadOnlyList<SFXProfile> GetList(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Advantage when _advantageSounds.Count > 0 => _advantageSounds,
            DamageType.Disadvantage when _disadvantageSounds.Count > 0 => _disadvantageSounds,
            _ => _normalSounds
        };
    }
}
