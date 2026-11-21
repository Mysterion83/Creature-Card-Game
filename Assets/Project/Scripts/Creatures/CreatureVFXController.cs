using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureVFXController : MonoBehaviour
{
    [Header("VFX Transform")]
    [SerializeField] private Transform _vfxTransform;

    [Header("VFX Profiles")]
    [SerializeField] private VFXAttackProfile _defaultAttackProfile;
    [SerializeField] private VFXDeathProfile _deathProfile;
    
    [Header("UI")]
    [SerializeField] private GameObject _damageIndicatorPrefab;
    
    private Creature _creature;
    private Transform SpawnTransform => _vfxTransform ? _vfxTransform : transform;
    private VFXAttackProfile _activeAttackProfile;
    private VFXAttackProfile _attackProfile;
    private VFXAttackProfile CurrentAttackProfile => _activeAttackProfile ? _activeAttackProfile : _defaultAttackProfile;
    
    private void Awake() => _creature = GetComponent<Creature>();

    private void OnEnable()
    {
        _creature.OnAttackStarted += HandleAttackStarted;
        _creature.OnAttackFired += HandleAttackFired;
        Creature.OnAnyAttackImpact += HandleAttackImpact;
        Creature.OnAnyAttackFinished += HandleAttackRecovery;
        _creature.OnCreatureDamaged += HandleDamaged;
        _creature.OnCreatureDeath += HandleDeath;
    }

    private void OnDisable()
    {
        _creature.OnAttackStarted -= HandleAttackStarted;
        _creature.OnAttackFired -= HandleAttackFired;
        Creature.OnAnyAttackImpact -= HandleAttackImpact;
        Creature.OnAnyAttackFinished -= HandleAttackRecovery;
        _creature.OnCreatureDamaged -= HandleDamaged;
        _creature.OnCreatureDeath -= HandleDeath;
    }

    #region Spawn Functions
    private void RunVFXProfile(IReadOnlyList<VFXProfile> effects, Creature target)
    {
        if (effects == null || effects.Count == 0) return;

        // Call relevant spawning function based on if the effect has a set delay
        foreach (VFXProfile entry in effects)
        {
            if (entry.Delay <= 0f) SpawnVFX(entry, target);
            else StartCoroutine(SpawnVFXDelayed(entry, target));
        }
    }

    private IEnumerator SpawnVFXDelayed(VFXProfile entry, Creature target)
    {
        yield return new WaitForSeconds(entry.Delay);
        SpawnVFX(entry, target);
    }

    private void SpawnVFX(VFXProfile profile, Creature target)
    {
        if (!profile.VFXPrefab) return;

        Transform parent = ResolveAnchor(profile.SpawnOnTarget, target);
        Vector3 spawnPos = parent.TransformPoint(profile.PositionOffset);
        Quaternion spawnRot = parent.rotation * Quaternion.Euler(profile.RotationOffset);

        GameObject vfxInstance = profile.ParentToAnchor
            ? Instantiate(profile.VFXPrefab, spawnPos, spawnRot, parent)
            : Instantiate(profile.VFXPrefab, spawnPos, spawnRot);

        // Change renderer colour to tint the given VFX
        if (profile.TintWithElementColor 
            && _creature 
            && _creature.Data.Element 
            && vfxInstance.TryGetComponent(out Renderer rend))
        {
            rend.material.color = _creature.Data.Element.Color;
        }
    }
    #endregion

    #region Helper Methods
    private Transform ResolveAnchor(bool spawnOnTarget, Creature target)
    {
        if (spawnOnTarget && target)
        {
            return target.TryGetComponent(out CreatureVFXController targetVfx)
                ? targetVfx.SpawnTransform
                : target.transform;
        }
        return SpawnTransform;
    }
    #endregion
    
    #region Attack VFX
    private void HandleAttackStarted(Creature attacker, Creature target)
    {
        if (CurrentAttackProfile) RunVFXProfile(CurrentAttackProfile.WindupEffects, target);
    }

    private void HandleAttackFired(Creature attacker, Creature target)
    {
        if (CurrentAttackProfile) RunVFXProfile(CurrentAttackProfile.AttackEffects, target);
    }

    private void HandleAttackImpact(AttackResult result)
    {
        if (result.Attacker != _creature || !result.Defender || !CurrentAttackProfile) return;
        RunVFXProfile(CurrentAttackProfile.ImpactEffects, result.Defender);
    }

    private void HandleAttackRecovery(Creature self)
    {
        if (self != _creature) return;
        if (CurrentAttackProfile) RunVFXProfile(CurrentAttackProfile.RecoveryEffects, self);
    }
    #endregion
    
    #region Hit/Death Reactions
    private void HandleDamaged(Creature creature, AttackResult result)
    {
        if (!_damageIndicatorPrefab) return;
        DamageType damageType = result.DamageType;
        
        GameObject prefab = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        if (prefab.TryGetComponent(out DamageIndicator damageIndicator))
        {
            damageIndicator.Display(damageType, result.DamageDealt);
        }    
    }

    private void HandleDeath(Creature creature)
    {
        if (!_deathProfile) return;
        StopAllCoroutines();
        RunVFXProfile(_deathProfile.Effects, null);
    }
    #endregion
}