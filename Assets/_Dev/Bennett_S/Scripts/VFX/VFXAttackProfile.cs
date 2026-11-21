using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New VFXAttackProfile", menuName = "Project/VFX/VFXAttackProfile")]
public class VFXAttackProfile : ScriptableObject
{
    [Header("Profile Lists")]
    [SerializeField] private List<VFXProfile> _windupEffects = new();
    [SerializeField] private List<VFXProfile> _attackEffects = new();
    [SerializeField] private List<VFXProfile> _impactEffects = new();
    [SerializeField] private List<VFXProfile> _recoveryEffects = new();

    public IReadOnlyList<VFXProfile> WindupEffects => _windupEffects;
    public IReadOnlyList<VFXProfile> AttackEffects => _attackEffects;
    public IReadOnlyList<VFXProfile> ImpactEffects => _impactEffects;
    public IReadOnlyList<VFXProfile> RecoveryEffects => _recoveryEffects;
}