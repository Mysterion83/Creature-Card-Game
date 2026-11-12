using UnityEngine;

public class DamageHandler : MonoBehaviour
{
    [SerializeField] private GameObject _damageIndicatorPrefab;
    private Creature _owningCreature;
    
    private void Awake()
    {
        _owningCreature = GetComponent<Creature>();
    }

    private void OnEnable()
    {
        if (_owningCreature) _owningCreature.OnCreatureDamaged += DisplayIndicator;
    }

    private void OnDisable()
    {
        if (_owningCreature) _owningCreature.OnCreatureDamaged -= DisplayIndicator;
    }

    // Change this in future
    public void DisplayIndicator(Creature creature, float damage)
    {
        if (!_damageIndicatorPrefab) return;
        DamageType damageType = DamageType.Normal; // Temp while this gets reworked
        
        GameObject prefab = Instantiate(_damageIndicatorPrefab, transform.position, Quaternion.identity);
        if (prefab.TryGetComponent(out DamageIndicator damageIndicator))
        {
            damageIndicator.Display(damageType, damage);
        }
    }
}