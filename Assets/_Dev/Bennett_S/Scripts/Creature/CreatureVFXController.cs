using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureVFXController : MonoBehaviour
{
    [Header("Attack VFX")]
    [SerializeField] private GameObject _windupVfxPrefab;
    [SerializeField] private GameObject _attackFireVfxPrefab;
    [SerializeField] private GameObject _hitSmokeVfxPrefab;
    [SerializeField] private GameObject _damageIndicatorPrefab;
    
    [Header("Hit Reaction VFX")]
    [SerializeField] private GameObject _deathVfxPrefab;
    [SerializeField] private Transform _vfxTransform;

    private Creature _creature;
    private Transform SpawnTransform => _vfxTransform ? _vfxTransform : transform;
    
    private void Awake() => _creature = GetComponent<Creature>();

    private void OnEnable()
    {
        _creature.OnAttackStarted += HandleAttackStarted;
        _creature.OnAttackFired += HandleAttackFired;
        Creature.OnAnyAttackImpact += HandleAttackImpact;
        _creature.OnCreatureDamaged += HandleDamaged;
        _creature.OnCreatureDeath += HandleDeath;
    }

    private void OnDisable()
    {
        _creature.OnAttackStarted -= HandleAttackStarted;
        _creature.OnAttackFired -= HandleAttackFired;
        Creature.OnAnyAttackImpact -= HandleAttackImpact;
        _creature.OnCreatureDamaged -= HandleDamaged;
        _creature.OnCreatureDeath -= HandleDeath;
    }
    
    private void HandleAttackStarted(Creature attacker, Creature target)
    {
        if (!_windupVfxPrefab) return;
        Instantiate(_windupVfxPrefab, SpawnTransform.position, SpawnTransform.rotation);
    }
    
    private void HandleAttackFired(Creature attacker, Creature target)
    {
        if (!_attackFireVfxPrefab) return;
        
        Vector3 targetPos = target.TryGetComponent(out CreatureVFXController targetVfx)
            ? targetVfx.SpawnTransform.position
            : target.transform.position;

        Instantiate(_attackFireVfxPrefab, targetPos, Quaternion.identity);
    }

    private void HandleAttackImpact(AttackResult result)
    {
        if (result.Attacker != _creature || !_hitSmokeVfxPrefab || !result.Defender) return;
        
        Vector3 targetPos = result.Defender.TryGetComponent(out CreatureVFXController targetVfx)
            ? targetVfx.SpawnTransform.position
            : result.Defender.transform.position;

        GameObject hitParticle = Instantiate(_hitSmokeVfxPrefab, targetPos, Quaternion.identity);

        if (!result.Attacker || !result.Attacker.Data.Element) return;
        
        if (hitParticle.TryGetComponent(out Renderer rend))
        {
            rend.material.color = result.Attacker.Data.Element.Color;
        }
    }
    
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
        if (!_deathVfxPrefab) return;
        Instantiate(_deathVfxPrefab, _vfxTransform.position, _vfxTransform.rotation);
    }
}