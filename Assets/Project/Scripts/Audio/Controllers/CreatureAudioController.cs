using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureAudioController : MonoBehaviour
{
    [Header("SFX Profiles")]
    [SerializeField] private SFXAttackProfile _attackProfile;
    [SerializeField] private SFXDamageProfile _damageProfile;
    [SerializeField] private SFXDeathProfile _deathProfile;
    [SerializeField] private SFXBoardProfile _boardProfile;
    
    private Creature _creature;
    private void Awake()
    {
        _creature = GetComponent<Creature>();
    }

    private void OnEnable()
    {
        _creature.OnAttackStarted += HandleAttackWindup;
        _creature.OnAttackFired += HandleAttackAction;
        Creature.OnAnyAttackImpact += HandleAnyAttackImpact;
        Creature.OnAnyAttackFinished += HandleAnyAttackFinished;
        _creature.OnCreatureDamaged += HandleDamaged;
        _creature.OnCreatureDeath += HandleDeath;
        
        BoardController.OnCreatureSummoned += HandleSummoned;
        BoardController.OnCreatureSelected += HandleSelected;
        BoardController.OnCreatureMoved += HandleMoved;
    }
    
    private void OnDisable()
    {
        _creature.OnAttackStarted -= HandleAttackWindup;
        _creature.OnAttackFired -= HandleAttackAction;
        Creature.OnAnyAttackImpact -= HandleAnyAttackImpact;
        Creature.OnAnyAttackFinished -= HandleAnyAttackFinished;
        _creature.OnCreatureDamaged -= HandleDamaged;
        _creature.OnCreatureDeath -= HandleDeath;
        
        BoardController.OnCreatureSummoned -= HandleSummoned;
        BoardController.OnCreatureSelected -= HandleSelected;
        BoardController.OnCreatureMoved -= HandleMoved;
    }
    
    #region Combat
    private void HandleAttackWindup(Creature attacker, Creature target)
    {
        if (!_attackProfile) return;
        PlayProfileList(_attackProfile.WindupSounds, target ? target.transform : null);
    }

    private void HandleAttackAction(Creature attacker, Creature target)
    {
        if (!_attackProfile) return;
        PlayProfileList(_attackProfile.AttackSounds, target ? target.transform : null);
    }
    
    private void HandleAnyAttackImpact(AttackResult result)
    {
        if (result.Attacker != _creature || !_attackProfile) return;
        PlayProfileList(_attackProfile.ImpactSounds, result.Defender ? result.Defender.transform : null);
    }

    private void HandleAnyAttackFinished(Creature attacker)
    {
        if (attacker != _creature || !_attackProfile) return;
        PlayProfileList(_attackProfile.RecoverySounds);
    }
    
    private void HandleDamaged(Creature creature, AttackResult result)
    {
        if (!_damageProfile) return;
        Transform attackerTransform = result.Attacker ? result.Attacker.transform : null;
        PlayProfileList(_damageProfile.GetList(result.DamageType), attackerTransform);
    }

    private void HandleDeath(Creature creature)
    {
        if (!_deathProfile) return;
        PlayProfileList(_deathProfile.Sounds);
    }
    #endregion

    #region Board/Movement
    private void HandleSummoned(Creature creature)
    {
        if (creature != _creature || !_boardProfile) return; 
        PlayProfileList(_boardProfile.SummonSounds);
    }
    
    private void HandleSelected(Creature creature)
    {
        if (creature != _creature || !_boardProfile) return; 
        PlayProfileList(_boardProfile.SelectSounds);
    }
    
    private void HandleMoved(Creature creature, Tile oldTile, Tile newTile)
    {
        if (creature != _creature || !_boardProfile) return; 
        PlayProfileList(_boardProfile.MoveSounds);
    }
    #endregion
    
    private void PlayProfileList(IReadOnlyList<SFXProfile> profiles, Transform target = null)
    {
        if (profiles == null || !AudioManager.Instance) return;

        foreach (var profile in profiles)
        {
            if (!profile.SoundDefinition) continue;

            Transform anchor = profile.PlayOnTarget && target ? target : transform;
            AudioManager.Instance.SFX.PlayDelayed(profile.SoundDefinition, profile.Delay, anchor, profile.PositionOffset);
        }
    }
}