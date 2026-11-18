using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureVFXController : MonoBehaviour
{
    [Header("Attack VFX")]
    [SerializeField] private GameObject _attackVFXPrefab;
    
    [Header("Damage VFX")]
    [SerializeField] private GameObject _hitVfxPrefab;
    [SerializeField] private GameObject _deathVfxPrefab;
    [SerializeField] private Transform _vfxTransform;

    private Creature _creature;

    private void Awake() => _creature = GetComponent<Creature>();

    private void OnEnable()
    {
        _creature.OnCreatureDamaged += HandleDamaged;
        _creature.OnCreatureDeath += HandleDeath;
    }

    private void OnDisable()
    {
        _creature.OnCreatureDamaged -= HandleDamaged;
        _creature.OnCreatureDeath -= HandleDeath;
    }

    private void HandleDamaged(Creature creature, float damage)
    {
        if (!_hitVfxPrefab) return;
        
        Vector3 spawnPosition = _vfxTransform ? _vfxTransform.position : transform.position;
        Instantiate(_hitVfxPrefab, spawnPosition, Quaternion.identity);
    }

    private void HandleDeath(Creature creature)
    {
        if (!_deathVfxPrefab) return;
        
        Vector3 spawnPosition = _vfxTransform ? _vfxTransform.position : transform.position;
        Instantiate(_deathVfxPrefab, spawnPosition, Quaternion.identity);
    }
}