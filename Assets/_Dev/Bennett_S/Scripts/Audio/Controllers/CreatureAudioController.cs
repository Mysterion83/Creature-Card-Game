using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureAudioController : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private SFXDefinition _damageSound;
    [SerializeField] private SFXDefinition _deathSound;
    private Creature _creature;
    
    private void Awake()
    {
        _creature = GetComponent<Creature>();
    }

    private void OnEnable()
    {
        _creature.OnCreatureDamaged += OnDamage;
        _creature.OnCreatureDeath += OnDeath;
    }
    
    private void OnDisable()
    {
        _creature.OnCreatureDamaged -= OnDamage;
        _creature.OnCreatureDeath -= OnDeath;
    }
    
    private void OnDamage(Creature creature, float damage)
    {
        //AudioManager.Instance.SFX.Play(_damageSound);
    }

    private void OnDeath(Creature creature)
    {
        AudioManager.Instance.SFX.Play(_deathSound);
    }
}
