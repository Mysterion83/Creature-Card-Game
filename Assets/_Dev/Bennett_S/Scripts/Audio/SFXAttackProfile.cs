using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFXAttackProfile", menuName = "Project/Audio/Profiles/SFXAttackProfile")]
public class SFXAttackProfile : ScriptableObject
{
    [Header("Profile Lists")]
    [SerializeField] private List<SFXProfile> _windupSounds = new();
    [SerializeField] private List<SFXProfile> _attackSounds = new();
    [SerializeField] private List<SFXProfile> _impactSounds = new();
    [SerializeField] private List<SFXProfile> _recoverySounds = new();

    public IReadOnlyList<SFXProfile> WindupSounds => _windupSounds;
    public IReadOnlyList<SFXProfile> AttackSounds => _attackSounds;
    public IReadOnlyList<SFXProfile> ImpactSounds => _impactSounds;
    public IReadOnlyList<SFXProfile> RecoverySounds => _recoverySounds;
}